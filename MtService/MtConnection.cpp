#include "pch.h"
#include "MtConnection.h"

#include <algorithm>
#include <iostream>

MtConnection::MtConnection(WebsocketStream&& ws)
    : ws_(std::move(ws))
    , resolver_(ws_.get_executor())
{
    log_.Debug("%s: connection %p created.", __FUNCTION__, this);
}

MtConnection::~MtConnection()
{
    log_.Debug("%s: connection %p destroyed.", __FUNCTION__, this);
}

void MtConnection::Accept()
{
    // Set suggested timeout settings for the websocket
    ws_.set_option(
        boost::beast::websocket::stream_base::timeout::suggested(
            boost::beast::role_type::server));

    // Set a decorator to change the Server of the handshake
    ws_.set_option(boost::beast::websocket::stream_base::decorator(
        [](boost::beast::websocket::response_type& res)
        {
        res.set(boost::beast::http::field::server,
                std::string(BOOST_BEAST_VERSION_STRING) +
                    " websocket-server-async");
    }));
    // Accept the websocket handshake
    ws_.async_accept(
        std::bind(
            &MtConnection::OnAccept,
            shared_from_this(),
            std::placeholders::_1));
}

void MtConnection::Close()
{
    log_.Debug("%s: entry.", __FUNCTION__);

    ws_.async_close(boost::beast::websocket::close_code::normal,
            [](boost::beast::error_code const ec){ });
}

void MtConnection::Send(boost::beast::flat_buffer&& buffer, SendPriority priority)
{
    Send(std::make_shared<const boost::beast::flat_buffer>(std::move(buffer)), priority);
}

void MtConnection::Send(std::shared_ptr<const boost::beast::flat_buffer> buffer, SendPriority priority)
{
    // Send queues must only be touched on the io_context thread (same as DoWrite / OnWrite).
    // All callers already run there, so dispatch executes the handler inline without
    // allocating and queueing it; a call from another thread would still be posted.
    boost::asio::dispatch(ws_.get_executor(),
        [this, self = shared_from_this(), buf = std::move(buffer), priority]() mutable {
            Enqueue(std::move(buf), priority);
        });
}

void MtConnection::Enqueue(std::shared_ptr<const boost::beast::flat_buffer> buffer, SendPriority priority)
{
    if (Logger::IsEnabled(LogLevel::Trace))
        log_.Trace("%s: priority = %d, msg size = %zu, msg = %s", __FUNCTION__,
            static_cast<int>(priority),
            buffer->size(),
            ToHex(*buffer).c_str());

    switch (priority)
    {
    case SendPriority::Response:
        response_queue_.push(std::move(buffer));
        break;
    case SendPriority::Notification:
        notification_queue_.push_back({ std::move(buffer), false });
        break;
    case SendPriority::Event:
        if (queued_events_ >= MaxQueuedEvents)
            DropOldestEvent();
        notification_queue_.push_back({ std::move(buffer), true });
        ++queued_events_;
        break;
    }

    if (!writing_)
        DoWrite();
}

void MtConnection::DropOldestEvent()
{
    // Notifications are rare, so the oldest event is at (or very close to) the front.
    auto it = std::find_if(notification_queue_.begin(), notification_queue_.end(),
        [](const QueuedMessage& m) { return m.droppable; });
    if (it == notification_queue_.end())
        return;

    notification_queue_.erase(it);
    --queued_events_;
    ++dropped_events_;

    if (dropped_events_ == 1)
        log_.Warning("%s: connection %p: client reads slower than events are produced, "
            "more than %zu events are queued. Dropping the oldest events.",
            __FUNCTION__, this, MaxQueuedEvents);
    else if (dropped_events_ % 10000 == 0)
        log_.Warning("%s: connection %p: %zu events dropped so far.",
            __FUNCTION__, this, dropped_events_);
}

void MtConnection::OnAccept(boost::beast::error_code ec)
{
    log_.Trace("%s: entry.", __FUNCTION__);

    if (ec)
    {
        log_.Error("%s: %s", __FUNCTION__, ec.what().c_str());
        OnConnectionFailed(ec.message());
    }
    else
        DoRead();

    OnConnected();
}

void MtConnection::DoRead()
{
    log_.Trace("%s: enter.", __FUNCTION__);

    // Read a message into our buffer
    ws_.async_read(
        read_buffer_,
        std::bind(
            &MtConnection::OnRead,
            shared_from_this(),
            std::placeholders::_1,
            std::placeholders::_2));
}

void MtConnection::OnRead(
    boost::beast::error_code ec,
    std::size_t bytes_transferred)
{
    boost::ignore_unused(bytes_transferred);

    // This indicates that the session was closed
    if (ec == boost::beast::websocket::error::closed)
    {
        log_.Info("%s: session was closed.", __FUNCTION__);
        OnDisconnected();
        return;
    }

    if (ec == boost::asio::error::operation_aborted)
    {
        log_.Info("%s: session was aborted.", __FUNCTION__);
        return;
    }

    if (ec == boost::asio::error::eof)
    {
        log_.Info("%s: %s. Remote peer gracefully closes the connection.", __FUNCTION__, ec.message().c_str());
        return;
    }

    if (ec)
    {
        log_.Error("%s: %s", __FUNCTION__, ec.message().c_str());
        OnConnectionFailed(ec.message());
    }
    else
    {
        OnMessageReceived(read_buffer_);

        // Clear the buffer
        read_buffer_.consume(read_buffer_.size());

        boost::asio::dispatch(ws_.get_executor(),
                              std::bind(
                                  &MtConnection::DoRead,
                                  shared_from_this()));
    }
}

void MtConnection::DoWrite()
{
    // Responses go first: they must not wait behind queued events.
    if (!response_queue_.empty())
    {
        writing_ = std::move(response_queue_.front());
        response_queue_.pop();
    }
    else if (!notification_queue_.empty())
    {
        auto& msg = notification_queue_.front();
        writing_ = std::move(msg.buffer);
        if (msg.droppable)
            --queued_events_;
        notification_queue_.pop_front();
    }
    else
    {
        writing_.reset();
        if (dropped_events_ > 0)
        {
            log_.Warning("%s: connection %p: send queue drained, %zu events were dropped.",
                __FUNCTION__, this, dropped_events_);
            dropped_events_ = 0;
        }
        return;
    }

    log_.Trace("%s: msg size = %zu", __FUNCTION__, writing_->size());

    ws_.binary(true);
    // writing_ keeps the data alive until OnWrite is called.
    ws_.async_write(
        writing_->data(),
        std::bind(
            &MtConnection::OnWrite,
            shared_from_this(),
            std::placeholders::_1,
            std::placeholders::_2));
}

void MtConnection::OnWrite(
    boost::beast::error_code ec,
    std::size_t bytes_transferred)
{
    boost::ignore_unused(bytes_transferred);

    if (ec)
    {
        log_.Error("%s, %s", __FUNCTION__, ec.what().c_str());
        // Keep writing_ set so that no new write is started on the failed stream.
        response_queue_ = {};
        notification_queue_.clear();
        queued_events_ = 0;
        OnConnectionFailed(ec.message());
        return;
    }

    DoWrite();
}
