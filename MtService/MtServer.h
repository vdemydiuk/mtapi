#pragma once

#include <iostream>
#include <memory>
#include <vector>
#include <unordered_map>
#include <boost/signals2/connection.hpp>
#include <boost/asio.hpp>
#include <boost/beast.hpp>
#include <boost/signals2.hpp>

#include "Logger.h"
#include "MtMessage.h"

class MtConnection;
class MtExpert;

struct MtExpertSlot
{
    MtExpert* expert;
    boost::signals2::connection event_connection;
    boost::signals2::connection deinit_connection;

    ~MtExpertSlot()
    {
        event_connection.disconnect();
        deinit_connection.disconnect();
    }
};

class MtServer : public std::enable_shared_from_this<MtServer>
{
public:
    MtServer(
        boost::asio::io_context& context,
        unsigned short port);
    ~MtServer();

    void AddExpert(int handle, MtExpert* expert);

    boost::signals2::signal<void(unsigned short)> OnStopped;

private:
    void Start();
    void Stop();

    void DoAccept();
    void OnAccept(boost::beast::error_code ec, 
            boost::asio::ip::tcp::socket socket);

    void OnEvent(const MtEvent& event);
    void OnDeinitExpert(int handle);

    void Send(const MtMessage& message);

    void ProcessMessage(const std::string& msg, std::weak_ptr<MtConnection> con);

    Logger log_{ "MtServer" };
    boost::asio::io_context& context_;
    unsigned short port_;
    boost::asio::ip::tcp::acceptor acceptor_;
    std::vector<std::shared_ptr<MtConnection>> pending_connections_;
    std::vector<std::shared_ptr<MtConnection>> connections_;
    std::unordered_map<int, std::unique_ptr<MtExpertSlot>> experts_;
    boost::asio::steady_timer stop_timer_;
};