#include "pch.h"
#include "MtServer.h"

#include <iostream>
#include <cstdlib>

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
            try
            {
                ProcessMessage(msg, con);
            }
            catch (std::exception e)
            {
                log_.Error("MtServer::OnMessageReceived: Failed to process message: %s", msg.c_str());
            }

        });
        pending_connections_.push_back(connection);
        connection->Accept();

        // Accept another connection
        DoAccept();
    }
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

void MtServer::ProcessMessage(const std::string& msg, std::weak_ptr<MtConnection> con)
{
    log_.Trace("%s: %s", __FUNCTION__, msg.c_str());

    std::string::size_type pos = msg.find(';');
    if (pos == std::string::npos)
    {
        log_.Warning("%s: : invalid message type.", __FUNCTION__);
        return;
    }

    std::string msg_type_str = msg.substr(0, pos);;
    auto msg_type = std::atoi(msg_type_str.c_str());
    if (msg_type == MessageType::COMMAND)
    {
        auto command = MtCommand::Parse(msg);
        if (command)
        {
            auto expert_handle = command->getExpertHandle();
            if (experts_.size() == 0)
            {
                log_.Error("%s: Expert list is empty. There is no any command executor.", __FUNCTION__);
            }
            else if (expert_handle == 0)
            {
                // use default expert
                auto expert = experts_.begin();
                log_.Debug("%s: using default expert for executing command - %d" , __FUNCTION__, expert->first);
                expert->second->expert->Process(std::move(command), [con = con, this](const MtResponse& response) {
                    auto connection = con.lock();
                    if (connection)
                        connection->Send(response.Serialize());
                    });
            }
            else if (experts_.count(expert_handle) > 0)
            {
                experts_[expert_handle]->expert->Process(std::move(command), [con = con, this](const MtResponse& response) {
                    auto connection = con.lock();
                    if (connection)
                        connection->Send(response.Serialize());
                });
            }
            else
                log_.Warning("%s: Expert is not found with handle %d", __FUNCTION__, command->getExpertHandle());
        }
        else
            log_.Warning("%s: Failed to parse command from message: %s", __FUNCTION__, msg.c_str());
    }
    else if (msg_type == MessageType::NOTIFICATION)
    {
        auto notification = MtNotification::Parse(msg);
        if (notification)
        {
            if (notification->GetNotificationType() == NotificationType::CLIENT_READY)
            {
                std::vector<int> expert_list;
                for (const auto& e : experts_)
                    expert_list.push_back(e.first);

                MtExpertListMsg msg(std::move(expert_list));
                auto connection = con.lock();
                if (connection)
                    connection->Send(msg.Serialize());
            }
        }
        else
            log_.Warning("%s: Failed to parse notification from message: %s", __FUNCTION__, msg.c_str());
    }
    else
    {
        log_.Warning("%s: unsupported message type %d", __FUNCTION__, msg_type);
        return;
    }
}