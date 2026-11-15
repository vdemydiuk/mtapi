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

namespace
{
// varDataEncoding.length has maxValue=2147483647 in the spec; the generated
// putStringPayload() does not enforce it, so callers must.
bool IsPayloadSizeValid(const std::string& payload)
{
    return payload.size() <= VarDataEncoding::lengthMaxValue();
}
}

void MtServer::Broadcast(boost::beast::flat_buffer&& buffer)
{
    // Encode once, share the same immutable buffer between all connections.
    auto shared_buf = std::make_shared<const boost::beast::flat_buffer>(std::move(buffer));
    for (auto& c : connections_)
        c->Send(shared_buf);
}

void MtServer::SendEvent(int expert_handle, int event_type, const std::string& payload)
{
    log_.Trace("%s: expert_handle=%d, event_type=%d, payload=%s", __FUNCTION__, expert_handle, event_type, payload.c_str());

    if (connections_.empty())
        return;

    if (!IsPayloadSizeValid(payload))
    {
        log_.Error("%s: event payload is too large (%zu bytes), event dropped. expert_handle=%d, event_type=%d",
            __FUNCTION__, payload.size(), expert_handle, event_type);
        return;
    }

    boost::beast::flat_buffer event_buf;

    MessageHeader header_encoder;
    MtEvent event_encoder;

    // Estimate max space needed and prepare Beast buffer memory
    std::size_t required_size = MessageHeader::encodedLength() +
        MtEvent::sbeBlockLength() +
        MtEvent::stringPayloadHeaderLength() + payload.size();

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

    assert(event_encoder.sbePosition() <= required_size);
    event_buf.commit(event_encoder.sbePosition());
    Broadcast(std::move(event_buf));
}

void MtServer::SendExpertAdded(int handle)
{
    log_.Trace("%s: entry. handle = %d", __FUNCTION__, handle);

    if (connections_.empty())
        return;

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

    assert(event_encoder.sbePosition() <= required_size);
    event_buf.commit(event_encoder.sbePosition());
    Broadcast(std::move(event_buf));
}

void MtServer::SendExpertRemoved(int handle)
{
    log_.Trace("%s: entry. handle = %d", __FUNCTION__, handle);

    if (connections_.empty())
        return;

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

    assert(event_encoder.sbePosition() <= required_size);
    event_buf.commit(event_encoder.sbePosition());
    Broadcast(std::move(event_buf));
}

void MtServer::ProcessMessage(const boost::beast::flat_buffer& buffer, std::weak_ptr<MtConnection> con)
{
    log_.Trace("MtServer::ProcessMessage: size=%zu | hex: %s", buffer.size(), ToHex(buffer).c_str());

    auto readable = buffer.data();
    // The generated SBE API requires char*, but decoders are read-only (they only index into the pointer).
    // flat_buffer's internal storage is heap-allocated writable memory; casting away const here is safe.
    char* src = static_cast<char*>(const_cast<void*>(readable.data()));
    std::size_t length = readable.size();

    // 1. Ensure buffer has enough data for at least the header
    if (length < MessageHeader::encodedLength()) {
        throw std::runtime_error("Buffer underflow: complete header not received");
    }

    // 2. Decode the MessageHeader
    MessageHeader header_decoder;
    header_decoder.wrap(src, 0, MessageHeader::sbeSchemaVersion(), length);

    // 3. Extract and validate header parameters
    std::uint16_t templateId = header_decoder.templateId();
    std::uint16_t blockLength = header_decoder.blockLength();
    std::uint16_t schemaId = header_decoder.schemaId();
    std::uint16_t version = header_decoder.version();

    if (schemaId != MtCommand::sbeSchemaId())
        throw std::runtime_error("Unexpected SBE schema ID: " + std::to_string(schemaId));
    // A message with a newer schema version is NOT rejected: SBE is forward compatible.
    // The decoder is wrapped with the sender's blockLength/version, so fields added in newer
    // versions are skipped and known fields are still read correctly. The per-message
    // minimum blockLength checks below guarantee that all fields this build reads are present.
    if (version != MtCommand::sbeSchemaVersion())
        log_.Debug("%s: message schema version %d differs from local version %d", __FUNCTION__,
            version, static_cast<int>(MtCommand::sbeSchemaVersion()));

    // 4. Dispatch based on templateId
    switch (templateId) {
    case MtCommand::sbeTemplateId():
    {
        if (experts_.size() == 0)
        {
            log_.Error("%s: Expert list is empty. There is no any command executor.", __FUNCTION__);
        }
        else
        {
            // The generated field getters read at fixed offsets without bounds checks,
            // so the client-supplied blockLength must cover the whole fixed block.
            if (blockLength < MtCommand::sbeBlockLength())
                throw std::runtime_error("Invalid MtCommand blockLength: " + std::to_string(blockLength));

            MtCommand cmd_decoder;
            cmd_decoder.wrapForDecode(src, header_decoder.encodedLength(), blockLength, version, length);

            auto expert_handle = cmd_decoder.expert_handle();
            auto command_id = cmd_decoder.command_id();
            auto command_type = cmd_decoder.command_type();

            // sbePosition() points at the varData length prefix (right after the fixed block).
            // stringPayloadLength() reads it without a bounds check, so verify it is present.
            const std::size_t length_prefix_pos = static_cast<std::size_t>(cmd_decoder.sbePosition());
            if (length_prefix_pos + MtCommand::stringPayloadHeaderLength() > length)
                throw std::runtime_error("Buffer underflow: MtCommand stringPayload length prefix missing");

            std::string payload;
            std::uint32_t payload_len = cmd_decoder.stringPayloadLength();
            if (payload_len > 0)
            {
                // The varData bytes follow the length prefix and must lie within [0, length).
                if (length_prefix_pos + MtCommand::stringPayloadHeaderLength() + payload_len > length)
                    throw std::runtime_error("Buffer underflow: MtCommand stringPayload exceeds buffer");

                const char* payload_ptr = cmd_decoder.stringPayload();
                payload = std::string(payload_ptr, payload_len);
            }

            log_.Trace("%s: Decoded MtCommand (ID: %d), expert_handle = %d, command_id = %llu, command_type = %d", __FUNCTION__,
                templateId, expert_handle, static_cast<unsigned long long>(command_id), command_type);

            auto cmd_callback = [con = con, self = shared_from_this()](int handle, uint64_t command_id, const std::string& payload) {
                auto connection = con.lock();
                if (connection)
                {
                    if (!IsPayloadSizeValid(payload))
                    {
                        // Must not throw here: this runs inside an io_context handler.
                        self->log_.Error("MtServer: response payload is too large (%zu bytes), response dropped. handle=%d, command_id=%llu",
                            payload.size(), handle, static_cast<unsigned long long>(command_id));
                        return;
                    }

                    boost::beast::flat_buffer response_buf;

                    MessageHeader header_encoder;
                    MtResponse response_encoder;

                    // Estimate max space needed and prepare Beast buffer memory
                    std::size_t required_size = MessageHeader::encodedLength() +
                        MtResponse::sbeBlockLength() +
                        MtResponse::stringPayloadHeaderLength() + payload.size();

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

                    assert(response_encoder.sbePosition() <= required_size);
                    response_buf.commit(response_encoder.sbePosition());
                    connection->Send(std::move(response_buf));
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
        if (blockLength < MtRequestExpertList::sbeBlockLength())
            throw std::runtime_error("Invalid MtRequestExpertList blockLength: " + std::to_string(blockLength));

        MtRequestExpertList request_decoder;
        request_decoder.wrapForDecode(src, header_decoder.encodedLength(), blockLength, version, length);

        auto request_id = request_decoder.request_id();

        log_.Trace("%s: Decoded MtRequestExpertList (ID: %d, request_id: %llu)", __FUNCTION__, templateId,
            static_cast<unsigned long long>(request_id));

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

            assert(response_encoder.sbePosition() <= required_size);
            response_buf.commit(response_encoder.sbePosition());
            connection->Send(std::move(response_buf));
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