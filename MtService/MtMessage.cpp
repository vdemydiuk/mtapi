#include "pch.h"
#include "MtMessage.h"

#include <boost/spirit/include/qi.hpp>
#include <boost/phoenix/core.hpp>
#include <boost/phoenix/operator.hpp>

std::unique_ptr<MtCommand> MtCommand::Parse(const std::string& msg)
{
    std::string::size_type pos = msg.find(';');

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

    std::unique_ptr<MtCommand> command;

    bool ok = qi::parse(msg.begin(), msg.end(), message_type_p >> -expert_handle_p >> -command_id_p >> -command_type_p >> -payload_p);
    if (ok)
    {
        command = std::make_unique<MtCommand>(std::stoi(expert_handle),
            std::stoi(command_id), std::stoi(command_type), std::move(payload));
    }

    return command;
}

std::unique_ptr<MtNotification> MtNotification::Parse(const std::string& msg)
{
    std::string::size_type pos = msg.find(';');

    using namespace boost::spirit;
    using rule_s = qi::rule<std::string::const_iterator, std::string()>;
    using rule = qi::rule<std::string::const_iterator>;

    std::string message_type;
    std::string notification_type;

    rule_s word_p = qi::as_string[+(qi::char_ - qi::char_(';'))];
    rule message_type_p = word_p[boost::phoenix::ref(message_type) = qi::_1];
    rule notification_type_p = +qi::char_(';') >> word_p[boost::phoenix::ref(notification_type) = qi::_1];

    std::unique_ptr<MtNotification> command;

    bool ok = qi::parse(msg.begin(), msg.end(), message_type_p >> -notification_type_p);
    if (ok)
        command = std::make_unique<MtNotification>((NotificationType)std::stoi(notification_type));

    return command;
}