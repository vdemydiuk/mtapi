#include "pch.h"
#include "MtServer.h"

#include <boost/beast/core/flat_buffer.hpp>
#include <iostream>
#include <cstdlib>
#include <string>

#include "MtConnection.h"
#include "MtExpert.h"

// MtService headers
#include "MessageHeader.h"
#include "MtCommand.h"
#include "MtEvent.h"
#include "MtExpertAdded.h"
#include "MtExpertList.h"
#include "MtExpertRemoved.h"
#include "MtResponse.h"
#include "MtRequestExpertList.h"

const std::string ADDRESS = "0.0.0.0";
constexpr int STOP_EXPERT_INTERVAL = 1; // 1 sec

using namespace MtService;

constexpr std::size_t MESSAGE_HEADER_SIZE = MessageHeader::encodedLength();

static std::string ToHex(const boost::beast::flat_buffer& buffer, size_t maxBytes = 256)
{
    static const char* digits = "0123456789ABCDEF";

    const auto data = buffer.data();
    const auto* p = static_cast<const unsigned char*>(data.data());
    const size_t size = data.size();
    const size_t n = std::min(size, maxBytes);

    std::string out;
    out.reserve(n * 3 + 16);

    for (size_t i = 0; i < n; ++i)
    {
        out.push_back(digits[p[i] >> 4]);
        out.push_back(digits[p[i] & 0x0F]);
        if (i + 1 < n)
            out.push_back(' ');
    }

    if (size > n)
        out += " ...(" + std::to_string(size - n) + " more bytes)";

    return out;
}

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
        slot->event_connection = expert->OnEvent.connect(boost::bind(&MtServer::OnEvent, this, std::placeholders::_1,
            std::placeholders::_2, std::placeholders::_3));
        slot->deinit_connection = expert->OnDeinit.connect(boost::bind(&MtServer::OnDeinitExpert, this, std::placeholders::_1));
        experts_[handle] = std::move(slot);

        SendExpertAdded(handle);
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
        connection->OnMessageReceived.connect(
            [con = connection->weak_from_this(), this](const boost::beast::flat_buffer& buffer)
        {
            try
            {
                ProcessMessage(buffer, con);
            }
            catch (const std::exception& e)
            {
                log_.Error("MtServer::OnMessageReceived: Failed to process message: %s | size=%zu | hex: %s",
                    e.what(),
                    buffer.size(),
                    ToHex(buffer).c_str());
            }
            catch (...)
            {
                log_.Error("MtServer::OnMessageReceived: Unknown exception | size=%zu | hex: %s",
                    buffer.size(),
                    ToHex(buffer).c_str());
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

void MtServer::OnEvent(int handle, int event_type, const std::string& payload)
{
    log_.Trace("%s: entry.", __FUNCTION__);

    SendEvent(handle, event_type, payload);
}

void MtServer::OnDeinitExpert(int handle)
{
    log_.Debug("%s: entry. handle = %d", __FUNCTION__, handle);

    if (experts_.count(handle) > 0)
    {
        log_.Debug("%s: remove expert %d from collection", __FUNCTION__, handle);
        experts_.erase(handle);
        SendExpertRemoved(handle);
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

void MtServer::SendEvent(int expert_handle, int event_type, const std::string& payload)
{
    log_.Trace("%s: expert_handle=%d, event_type=%d, payload=%s", __FUNCTION__, expert_handle, event_type, payload.c_str());

    for (auto& c : connections_)
    {
        boost::beast::flat_buffer event_buf;

        MessageHeader header_encoder;
        MtEvent event_encoder;

        // Estimate max space needed and prepare Beast buffer memory
        std::size_t required_size = MessageHeader::encodedLength() +
            MtEvent::sbeBlockLength() +
            4 + payload.size(); // 4 bytes for length prefix

        auto mutable_buffer = event_buf.prepare(required_size);
        char* dest = static_cast<char*>(mutable_buffer.data());

        // 1. Wrap and write Message Header
        header_encoder.wrap(dest, 0, MessageHeader::sbeSchemaVersion(), required_size)
            .blockLength(MtEvent::sbeBlockLength())
            .templateId(MtEvent::sbeTemplateId())
            .schemaId(MtEvent::sbeSchemaId())
            .version(MtEvent::sbeSchemaVersion());

        // 2. Wrap and write Static Fields
        event_encoder.wrapForEncode(dest, header_encoder.encodedLength(), required_size)
            .expert_handle(expert_handle)
            .event_type(event_type);

        // 3. Write Variable-Length JSON Data
        event_encoder.putStringPayload(payload.data(), static_cast<std::uint32_t>(payload.size()));

        // Commit written bytes into Beast flat buffer
        event_buf.commit(event_encoder.sbePosition());

        assert(event_buf.size() > 0);
        if (event_buf.size() == 0)
        {
            log_.Error("%s: Event buffer is empty after encoding.", __FUNCTION__);
        }
        else
        {
            c->Send(std::move(event_buf));
        }
    }
}

void MtServer::SendExpertAdded(int handle)
{
    log_.Trace("%s: entry. handle = %d", __FUNCTION__, handle);

    for (auto& c : connections_)
    {
        boost::beast::flat_buffer event_buf;

        MessageHeader header_encoder;
        MtExpertAdded event_encoder;

        // Estimate max space needed and prepare Beast buffer memory
        constexpr std::size_t required_size = MessageHeader::encodedLength() +
            MtExpertAdded::sbeBlockLength();

        auto mutable_buffer = event_buf.prepare(required_size);
        char* dest = static_cast<char*>(mutable_buffer.data());

        // 1. Wrap and write Message Header
        header_encoder.wrap(dest, 0, MessageHeader::sbeSchemaVersion(), required_size)
            .blockLength(MtExpertAdded::sbeBlockLength())
            .templateId(MtExpertAdded::sbeTemplateId())
            .schemaId(MtExpertAdded::sbeSchemaId())
            .version(MtExpertAdded::sbeSchemaVersion());

        // 2. Wrap and write Static Fields
        event_encoder.wrapForEncode(dest, header_encoder.encodedLength(), required_size)
            .expert_handle(handle);

        // Commit written bytes into Beast flat buffer
        event_buf.commit(event_encoder.sbePosition());

        assert(event_buf.size() > 0);
        if (event_buf.size() == 0)
        {
            log_.Error("%s: Event ExpertAdded buffer is empty after encoding.", __FUNCTION__);
        }
        else
        {
            c->Send(std::move(event_buf));
        }
    }
}

void MtServer::SendExpertRemoved(int handle)
{
    log_.Trace("%s: entry. handle = %d", __FUNCTION__, handle);

    for (auto& c : connections_)
    {
        boost::beast::flat_buffer event_buf;

        MessageHeader header_encoder;
        MtExpertRemoved event_encoder;

        // Estimate max space needed and prepare Beast buffer memory
        constexpr std::size_t required_size = MessageHeader::encodedLength() +
            MtExpertRemoved::sbeBlockLength();

        auto mutable_buffer = event_buf.prepare(required_size);
        char* dest = static_cast<char*>(mutable_buffer.data());

        // 1. Wrap and write Message Header
        header_encoder.wrap(dest, 0, MessageHeader::sbeSchemaVersion(), required_size)
            .blockLength(MtExpertRemoved::sbeBlockLength())
            .templateId(MtExpertRemoved::sbeTemplateId())
            .schemaId(MtExpertRemoved::sbeSchemaId())
            .version(MtExpertRemoved::sbeSchemaVersion());

        // 2. Wrap and write Static Fields
        event_encoder.wrapForEncode(dest, header_encoder.encodedLength(), required_size)
            .expert_handle(handle);

        // Commit written bytes into Beast flat buffer
        event_buf.commit(event_encoder.sbePosition());

        assert(event_buf.size() > 0);
        if (event_buf.size() == 0)
        {
            log_.Error("%s: Event ExpertRemoved buffer is empty after encoding.", __FUNCTION__);
        }
        else
        {
            c->Send(std::move(event_buf));
        }
    }
}

void MtServer::ProcessMessage(const boost::beast::flat_buffer& buffer, std::weak_ptr<MtConnection> con)
{
    auto readable = buffer.data();
    char* src = static_cast<char*>(const_cast<void*>(readable.data()));
    std::size_t length = readable.size();

    // 1. Ensure buffer has enough data for at least the header
    if (length < MessageHeader::encodedLength()) {
        throw std::runtime_error("Buffer underflow: complete header not received");
    }

    // 2. Decode the MessageHeader
    MessageHeader header_decoder;
    header_decoder.wrap(src, 0, MessageHeader::sbeSchemaVersion(), length);

    // 3. Extract header parameters
    std::uint16_t templateId = header_decoder.templateId();
    std::uint16_t blockLength = header_decoder.blockLength();
    std::uint16_t schemaId = header_decoder.schemaId();
    std::uint16_t version = header_decoder.version();

    // 4. Dispatch based on templateId
    switch (templateId) {
    case MtCommand::sbeTemplateId():
    {
        MtCommand cmd_decoder;
        cmd_decoder.wrapForDecode(src, header_decoder.encodedLength(), blockLength, version, length);

        std::cout << "Decoded MtCommand (ID: " << templateId << ")\n";

        if (experts_.size() == 0)
        {
            log_.Error("%s: Expert list is empty. There is no any command executor.", __FUNCTION__);
        }
        else
        {
            auto expert_handle = cmd_decoder.expert_handle();
            auto command_type = cmd_decoder.command_type();
            auto command_id = cmd_decoder.command_id();

            const char* payload_ptr = cmd_decoder.stringPayload();
            std::uint32_t payload_len = cmd_decoder.stringPayloadLength();
            std::string payload(payload_ptr, payload_len);

            auto cmd_callback = [con = con, this](int handle, int command_id, const std::string& payload) {
                auto connection = con.lock();
                if (connection)
                {
                    boost::beast::flat_buffer response_buf;

                    MessageHeader header_encoder;
                    MtResponse response_encoder;

                    // Estimate max space needed and prepare Beast buffer memory
                    std::size_t required_size = MessageHeader::encodedLength() +
                        MtResponse::sbeBlockLength() +
                        4 + payload.size(); // 4 bytes for length prefix

                    auto mutable_buffer = response_buf.prepare(required_size);
                    char* dest = static_cast<char*>(mutable_buffer.data());

                    // 1. Wrap and write Message Header
                    header_encoder.wrap(dest, 0, MessageHeader::sbeSchemaVersion(), required_size)
                        .blockLength(MtResponse::sbeBlockLength())
                        .templateId(MtResponse::sbeTemplateId())
                        .schemaId(MtResponse::sbeSchemaId())
                        .version(MtResponse::sbeSchemaVersion());

                    // 2. Wrap and write Static Fields
                    response_encoder.wrapForEncode(dest, header_encoder.encodedLength(), required_size)
                        .expert_handle(handle)
                        .command_id(command_id);

                    // 3. Write Variable-Length JSON Data
                    response_encoder.putStringPayload(payload.data(), static_cast<std::uint32_t>(payload.size()));

                    // Commit written bytes into Beast flat buffer
                    response_buf.commit(response_encoder.sbePosition());

                    // Verification
                    assert(response_buf.size() > 0);
                    if (response_buf.size() == 0)
                    {
                        log_.Error("%s: Response buffer is empty after encoding.", __FUNCTION__);
                    }
                    else
                    {
                        connection->Send(std::move(response_buf));
                    }
                }
            };

            if (expert_handle == 0)
            {
                // use default expert
                auto expert = experts_.begin();
                log_.Debug("%s: using default expert for executing command - %d", __FUNCTION__, expert->first);
                expert->second->expert->ProcessCommand(command_id, command_type, std::move(payload), std::move(cmd_callback));
            }
            else if (experts_.count(expert_handle) > 0)
            {
                experts_[expert_handle]->expert->ProcessCommand(command_id, command_type, std::move(payload), std::move(cmd_callback));
            }
            else
            {
                log_.Warning("%s: Expert is not found with handle %d", __FUNCTION__, expert_handle);
            }
        }

        break;
    }
    case MtRequestExpertList::sbeTemplateId():
    {
        MtRequestExpertList request_decoder;
        request_decoder.wrapForDecode(src, header_decoder.encodedLength(), blockLength, version, length);

        auto request_id = request_decoder.request_id();

        std::cout << "Decoded MtRequestExpertList (ID: " << templateId << ", request_id: " << request_id << ")\n";

        auto connection = con.lock();
        if (connection)
        {
            boost::beast::flat_buffer response_buf;
            MessageHeader header_encoder;
            MtExpertList response_encoder;

            // Allocate memory: Header + Main Block + (Group Header + Elements * int32)
            std::size_t required_size = MessageHeader::encodedLength() +
                MtExpertList::sbeBlockLength() +
                4 + (experts_.size() * sizeof(int32_t));

            auto mutableBuf = response_buf.prepare(required_size);
            char* dest = static_cast<char*>(mutableBuf.data());

            // 1. Write Header
            header_encoder.wrap(dest, 0, MessageHeader::sbeSchemaVersion(), required_size)
                .blockLength(MtExpertList::sbeBlockLength())
                .templateId(MtExpertList::sbeTemplateId())
                .schemaId(MtExpertList::sbeSchemaId())
                .version(MtExpertList::sbeSchemaVersion());

            // 2. Write Static Fields
            response_encoder.wrapForEncode(dest, header_encoder.encodedLength(), required_size)
                .request_id(request_id);

            // 3. Write Vector Elements into SBE Group
            auto& expert_list_encoder = response_encoder.expert_listCount(static_cast<std::uint16_t>(experts_.size()));
            for (const auto& e : experts_) {
                expert_list_encoder.next().expert_handle(e.first);
            }

            response_buf.commit(response_encoder.sbePosition());
            assert(response_buf.size() > 0);
            if (response_buf.size() == 0)
            {
                log_.Error("%s: Event ExpertRemoved buffer is empty after encoding.", __FUNCTION__);
            }
            else
            {
                connection->Send(std::move(response_buf));
            }
        }
        else
        {
            log_.Warning("%s: Connection is no longer valid when sending expert list response.", __FUNCTION__);
        }

        break;
    }
    default:
        log_.Error("%s: Unknown SBE template ID: %d.", __FUNCTION__, templateId);
        break;
    }
}