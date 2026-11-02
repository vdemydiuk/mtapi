#include "pch.h"
#include "MtConnection.h"

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

void MtConnection::Send(boost::beast::flat_buffer&& buffer)
{
    log_.Trace("%s: msg = %s", __FUNCTION__, boost::beast::buffers_to_string(buffer.data()).c_str());

    // TODO: Optimize this to avoid copying the buffer data into a string. Instead, we can use the buffer directly in the async_write call.
    send_queue_.push(std::move(buffer));
    if (send_queue_.size() == 1)
        DoWrite();
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
    if (send_queue_.empty())
        return;

    log_.Trace("%s: msg = %s", __FUNCTION__, boost::beast::buffers_to_string(send_queue_.front().data()).c_str());

    //ws_.text(ws_.got_text());
    auto& buffer = send_queue_.front();
    ws_.async_write(
        boost::asio::buffer(buffer.data(), buffer.size()),
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
        return;
    }

    send_queue_.pop();
    DoWrite();
}

