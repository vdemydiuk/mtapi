#pragma once

#include <boost/asio.hpp>
#include <boost/beast.hpp>
#include <boost/signals2.hpp>
#include <memory>
#include <string>
#include <deque>
#include <queue>

#include "Logger.h"

typedef boost::beast::websocket::stream<boost::asio::ip::tcp::socket> WebsocketStream;

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

// How a queued outgoing message is treated when the client reads slower than messages are produced.
enum class SendPriority
{
    // Reply to a client request (MtResponse, MtExpertList). Sent before any queued notification
    // and never dropped.
    Response,
    // Notification that the client relies on to keep its state (MtExpertAdded, MtExpertRemoved).
    // Kept in order with events and never dropped.
    Notification,
    // MtEvent (ticks, trade events...). Kept in order with notifications. The oldest ones are
    // dropped when more than MaxQueuedEvents are waiting to be sent.
    Event
};

class MtConnection : public std::enable_shared_from_this<MtConnection>
{
public:
    // Max number of MtEvent messages waiting in the send queue of one connection.
    static constexpr std::size_t MaxQueuedEvents = 3000;

    MtConnection(WebsocketStream&& ws);
    ~MtConnection();

    void Accept();
    void Close();
    void Send(boost::beast::flat_buffer&& buffer, SendPriority priority);
    // Queue an already-encoded message that may be shared with other connections (broadcast).
    void Send(std::shared_ptr<const boost::beast::flat_buffer> buffer, SendPriority priority);

    boost::signals2::signal<void()> OnConnected;
    boost::signals2::signal<void(const std::string&)> OnConnectionFailed;
    boost::signals2::signal<void()> OnDisconnected;
    boost::signals2::signal<void(const boost::beast::flat_buffer& buffer)> OnMessageReceived;

private:
    void OnAccept(boost::beast::error_code ec);
    void DoRead();
    void OnRead(
        boost::beast::error_code ec,
        std::size_t bytes_transferred);
    void Enqueue(std::shared_ptr<const boost::beast::flat_buffer> buffer, SendPriority priority);
    void DropOldestEvent();
    void DoWrite();
    void OnWrite(
        boost::beast::error_code ec,
        std::size_t bytes_transferred);

    Logger log_{ "MtConnection" };
    WebsocketStream ws_;
    boost::asio::ip::tcp::resolver resolver_; 
    std::string host_;
    boost::beast::flat_buffer read_buffer_;

    struct QueuedMessage
    {
        std::shared_ptr<const boost::beast::flat_buffer> buffer;
        bool droppable;
    };

    // Send queues are only touched on the io_context thread.
    std::queue<std::shared_ptr<const boost::beast::flat_buffer>> response_queue_;
    std::deque<QueuedMessage> notification_queue_;   // events and notifications, in order
    std::size_t queued_events_ = 0;                  // droppable entries in notification_queue_
    std::size_t dropped_events_ = 0;                 // events dropped since the last overflow began
    // Message being written now; it must stay alive until OnWrite is called.
    std::shared_ptr<const boost::beast::flat_buffer> writing_;
};
