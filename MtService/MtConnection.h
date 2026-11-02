#pragma once

#include <boost/asio.hpp>
#include <boost/beast.hpp>
#include <boost/signals2.hpp>
#include <memory>
#include <string>
#include <queue>

#include "Logger.h"

typedef boost::beast::websocket::stream<boost::asio::ip::tcp::socket> WebsocketStream;

class MtConnection : public std::enable_shared_from_this<MtConnection>
{
public:
    MtConnection(WebsocketStream&& ws);
    ~MtConnection();

    void Accept();
    void Close();
    void Send(boost::beast::flat_buffer&& buffer);

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
    void DoWrite();
    void OnWrite(
        boost::beast::error_code ec,
        std::size_t bytes_transferred);

    Logger log_{ "MtConnection" };
    WebsocketStream ws_;
    boost::asio::ip::tcp::resolver resolver_; 
    std::string host_;
    boost::beast::flat_buffer read_buffer_;
    std::queue<boost::beast::flat_buffer> send_queue_;
};
