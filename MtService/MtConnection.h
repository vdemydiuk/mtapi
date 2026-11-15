#pragma once

#include <boost/asio.hpp>
#include <boost/beast.hpp>
#include <boost/signals2.hpp>
#include <memory>
#include <string>
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
