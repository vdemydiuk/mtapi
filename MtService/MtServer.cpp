#include "pch.h"
#include "MtServer.h"

#include <iostream>
#include <boost/spirit/include/qi.hpp>
#include <boost/phoenix/core.hpp>
#include <boost/phoenix/operator.hpp>

#include "MtConnection.h"
#include "MtExpert.h"

const std::string ADDRESS = "0.0.0.0";
constexpr int STOP_EXPERT_INTERVAL = 1; // 1 sec

MtServer::MtServer(
    boost::asio::io_context& context,
    unsigned short port)
    : context_(context)
    , port_(port)
    , acceptor_(context)
    , stop_timer_(context)
{
    boost::beast::error_code ec;

    boost::asio::ip::tcp::endpoint endpoint{boost::asio::ip::make_address(ADDRESS), port};

    // Open the acceptor
    acceptor_.open(endpoint.protocol(), ec);
    if (ec)
    {
        log_.Error("%s: open failed! %s", __FUNCTION__, ec.what().c_str());
        return;
    }

    // Allow address reuse
    acceptor_.set_option(boost::asio::socket_base::reuse_address(true), ec);
    if (ec)
    {
        log_.Error("%s: set_option failed! %s", __FUNCTION__, ec.what().c_str());
        return;
    }

    // Bind to the server address
    acceptor_.bind(endpoint, ec);
    if (ec)
    {
        log_.Error("%s: bind failed! %s", __FUNCTION__, ec.what().c_str());
        return;
    }

    // Start listening for connections
    acceptor_.listen(
        boost::asio::socket_base::max_listen_connections, ec);
    if (ec)
    {
        log_.Error("%s: listen failed! %s", __FUNCTION__, ec.what().c_str());
        return;
    }
}

MtServer::~MtServer()
{
    log_.Debug("%s: destructor called.", __FUNCTION__);
}

void MtServer::AddExpert(int handle, MtExpert* expert)
{
    log_.Debug("%s: handle = %d", __FUNCTION__, handle);

    if (experts_.count(handle) == 0)
    {
        if (experts_.size() == 0)
        {
            Start();
        }

        auto slot = std::make_unique<MtExpertSlot>();
        slot->expert = expert;
        slot->event_connection = expert->OnEvent.connect(boost::bind(&MtServer::OnEvent, this, std::placeholders::_1));
        slot->deinit_connection = expert->OnDeinit.connect(boost::bind(&MtServer::OnDeinitExpert, this, std::placeholders::_1));
        experts_[handle] = std::move(slot);

        Send(MtExpertAddedMsg(handle));
    }
}

void MtServer::DoAccept()
{
    // The new connection gets its own strand
    acceptor_.async_accept(
        boost::asio::make_strand(context_),
        boost::beast::bind_front_handler(
            &MtServer::OnAccept,
            shared_from_this()));
}

void MtServer::OnAccept(boost::beast::error_code ec,
                        boost::asio::ip::tcp::socket socket)
{
    log_.Trace("%s: enter", __FUNCTION__);

    if (ec == boost::asio::error::operation_aborted)
    {
        log_.Info("%s: session was aborted.", __FUNCTION__);
        return;
    }

    if (ec)
    {
        log_.Error("%s: %s", __FUNCTION__, ec.what().c_str());
    }
    else
    {
        WebsocketStream ws(std::move(socket));
        auto connection = std::make_shared<MtConnection>(std::move(ws));
        connection->OnConnected.connect([con = connection->weak_from_this(), this]() {
            auto connection = con.lock();
            if (connection)
            {
                auto it = std::find(pending_connections_.begin(), pending_connections_.end(), connection);
                if (it != pending_connections_.end())
                {
                    pending_connections_.erase(it);
                    connections_.push_back(connection);
                    log_.Trace("%s: Pushed connection %p to collection", __FUNCTION__, connection.get());

                    std::vector<int> expert_list;
                    for (const auto& e : experts_)
                        expert_list.push_back(e.first);

                    MtExpertListMsg msg(std::move(expert_list));
                    connection->Send(msg.Serialize());
                }
            }
        });
        connection->OnConnectionFailed.connect([con = connection->weak_from_this(), this](const std::string& msg) {
            log_.Warning("MtServer::OnConnectionFailed: %s", msg.c_str());
            auto connection = con.lock();
            if (connection)
            {
                if (pending_connections_.size() > 0)
                {
                    auto it = std::find(pending_connections_.begin(), pending_connections_.end(), connection);
                    if (it != pending_connections_.end())
                    {
                        pending_connections_.erase(it);
                        log_.Trace("MtServer::OnConnectionFailed: Removed connection %p from pending collection", connection.get());
                    }
                }
                if (connections_.size() > 0)
                {
                    auto it = std::find(connections_.begin(), connections_.end(), connection);
                    if (it != connections_.end())
                    {
                        connections_.erase(it);
                        log_.Trace("MtServer::OnConnectionFailed: Removed connection %p from collection", connection.get());
                    }
                }
            }
        });
        connection->OnDisconnected.connect([con = connection->weak_from_this(), this]() {
            auto connection = con.lock();
            if (connection)
            {
                auto it = std::find(connections_.begin(), connections_.end(), connection); 
                if (it != connections_.end())
                {
                    connections_.erase(it);
                    log_.Trace("MtServer::OnDisconnected: Removed connection %p from collection", connection.get());
                }
            }
        });
        connection->OnMessageReceived.connect([con = connection->weak_from_this(), this](const std::string& msg) {
            log_.Trace("MtServer::OnMessageReceived: %s", msg.c_str());
            auto command = ParseMessage(msg);
            if (command)
            {
                if (experts_.count(command->getExpertHandle()) > 0)
                {
                    experts_[command->getExpertHandle()]->expert->Process(std::move(command), [&](const MtResponse& response) {
                        auto connection = con.lock();
                        if (connection)
                            connection->Send(response.Serialize());        
                        else
                            log_.Warning("MtServer::OnMessageReceived: connection is not valid (null)");
                    });
                }
                else
                {
                    log_.Warning("MtServer::OnMessageReceived: Expert is not found with handle %d", command->getExpertHandle());
                }
            }
            else
            {
                log_.Warning("MtServer::OnMessageReceived: Failed to parse command from message: %s", msg.c_str());
            }
        });
        pending_connections_.push_back(connection);
        connection->Accept();

        // Accept another connection
        DoAccept();
    }
}

std::unique_ptr<MtCommand> MtServer::ParseMessage(const std::string& msg)
{
    std::unique_ptr<MtCommand> command;

    using namespace boost::spirit;
    using rule_s = qi::rule<std::string::const_iterator, std::string()>;
    using rule = qi::rule<std::string::const_iterator>;

    std::string message_type;
    std::string expert_handle;
    std::string command_id;
    std::string command_type;
    std::string payload;

    rule_s word_p = qi::as_string[+(qi::char_ - qi::char_(';'))];
    rule message_type_p = word_p[boost::phoenix::ref(message_type) = qi::_1];
    rule expert_handle_p = +qi::char_(';') >> word_p[boost::phoenix::ref(expert_handle) = qi::_1];
    rule command_id_p = +qi::char_(';') >> word_p[boost::phoenix::ref(command_id) = qi::_1];
    rule command_type_p = +qi::char_(';') >> word_p[boost::phoenix::ref(command_type) = qi::_1];
    rule payload_p = +qi::char_(';') >> word_p[boost::phoenix::ref(payload) = qi::_1];

    bool ok = qi::parse(msg.begin(), msg.end(), message_type_p >> -expert_handle_p >> -command_id_p >> -command_type_p >> -payload_p);
    if (ok)
    {
        command = std::make_unique<MtCommand>(std::stoi(expert_handle),
                std::stoi(command_id), std::stoi(command_type), std::move(payload));
    }

    return command;
}

// Start accepting incoming connections
void MtServer::Start()
{
    log_.Debug("%s: entry. port = %d", __FUNCTION__, port_);

    DoAccept();
}

void MtServer::Stop()
{
    log_.Debug("%s: entry. port = %d", __FUNCTION__, port_);

    for(auto& c : connections_)
        c->Close();

    connections_.clear();
    pending_connections_.clear();

    acceptor_.cancel();

    OnStopped(port_);
}

void MtServer::OnEvent(const MtEvent& event)
{
    log_.Trace("%s: entry.", __FUNCTION__);

    Send(event);
}

void MtServer::OnDeinitExpert(int handle)
{
    log_.Debug("%s: entry. handle = %d", __FUNCTION__, handle);

    if (experts_.count(handle) > 0)
    {
        log_.Debug("%s: remove expert %d from collection", __FUNCTION__, handle);
        experts_.erase(handle);
        Send(MtExpertRemovedMsg(handle));
    }

    if (experts_.size() == 0)
    {
        log_.Debug("%s: expert count = 0. Starting stop timer (1 sec) ...", __FUNCTION__, handle);
        stop_timer_.expires_from_now(boost::asio::chrono::seconds(STOP_EXPERT_INTERVAL));
        stop_timer_.async_wait([this](const boost::system::error_code& /*ec*/) {
            if (experts_.size() == 0)
                Stop();
        });
    }
}

void MtServer::Send(const MtMessage& message)
{
    auto msg = message.Serialize();
    log_.Trace("%s: %s", __FUNCTION__, msg.c_str());
    for (auto& c : connections_)
        c->Send(msg);
}