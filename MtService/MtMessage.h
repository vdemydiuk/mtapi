#pragma once

#include <string>
#include <sstream>

const std::string MT_MESSAGE_DELIMETER{ ";" };

enum MessageType
{
    COMMAND = 0,
    RESPONSE = 1,
    EVENT = 2,
    EXPERT_LIST = 3,
    EXPERT_ADDED = 4,
    EXPERT_REMOVED = 5
};

class MtMessage
{
public:
    std::string Serialize() const
    {
        return std::to_string(GetType()) + MT_MESSAGE_DELIMETER + GetBody();
    }

protected:
    virtual MessageType GetType() const = 0;
    virtual std::string GetBody() const = 0;
};

class MtResponse : public MtMessage
{
public:
    MtResponse(int expert_handle, int id, const std::string& payload)
        : expert_handle_(expert_handle)
        , id_(id)
        , payload_(payload)
    {
    }

    int getExpertHandle() const { return expert_handle_; }

    int getCommandId() const { return id_; }

    std::string getPayload() const { return payload_; }

private:
    MessageType GetType() const override
    {
        return MessageType::RESPONSE;
    }

    std::string GetBody() const override
    {
        return std::to_string(expert_handle_)
            + MT_MESSAGE_DELIMETER + std::to_string(id_)
            + MT_MESSAGE_DELIMETER + payload_;
    }

    int expert_handle_;
    int id_;
    std::string payload_;
};

class MtCommand : public MtMessage 
{
public:
    MtCommand(int expert_handle, int command_id, int command_type, const std::string& payload)
        : expert_handle_(expert_handle) 
        , command_id_(command_id)
        , command_type_(command_type)
        , payload_(payload)
    {
    }

    int getExpertHandle() const { return expert_handle_; }

    int getCommandId() const { return command_id_; }

    int getCommandType() const { return command_type_; }

    std::string getPayload() const { return payload_; }

private:
    MessageType GetType() const override
    {
        return MessageType::COMMAND;
    }

    std::string GetBody() const override
    {
        return std::to_string(expert_handle_)
            + MT_MESSAGE_DELIMETER + std::to_string(command_id_)
            + MT_MESSAGE_DELIMETER + std::to_string(command_type_)
            + MT_MESSAGE_DELIMETER + payload_;
    }

    int expert_handle_;
    int command_id_;
    int command_type_;
    std::string payload_;
};

class MtEvent : public MtMessage 
{
public:
    MtEvent(int expert_handle, int event_type, const std::string& payload)
        : expert_handle_(expert_handle)
        , event_type_(event_type)
        , payload_(payload)
    {
    }

private:
    MessageType GetType() const override
    {
        return MessageType::EVENT;
    }

    std::string GetBody() const override
    {
        return std::to_string(expert_handle_)
            + MT_MESSAGE_DELIMETER + std::to_string(event_type_)
            + MT_MESSAGE_DELIMETER + payload_;
    }

    int expert_handle_;
    int event_type_;
    std::string payload_;
};

class MtExpertListMsg : public MtMessage
{
public:
    MtExpertListMsg(std::vector<int> experts)
        : experts_(std::move(experts))
    {
    }

private:
    MessageType GetType() const override
    {
        return MessageType::EXPERT_LIST;
    }

    std::string GetBody() const override
    {
        std::stringstream ss;
        for (size_t i = 0; i < experts_.size(); ++i)
        {
            if (i != 0)
                ss << ",";
            ss << experts_[i];
        }
        return ss.str();
    }

    int expert_handle_;
    std::vector<int> experts_;
};

class MtExpertAddedMsg : public MtMessage
{
public:
    MtExpertAddedMsg(int handle)
        : handle_(handle)
    {
    }

private:
    MessageType GetType() const override
    {
        return MessageType::EXPERT_ADDED;
    }

    std::string GetBody() const override
    {
        return std::to_string(handle_);
    }

    int handle_;
};

class MtExpertRemovedMsg : public MtMessage
{
public:
    MtExpertRemovedMsg(int handle)
        : handle_(handle)
    {
    }

private:
    MessageType GetType() const override
    {
        return MessageType::EXPERT_REMOVED;
    }

    std::string GetBody() const override
    {
        return std::to_string(handle_);
    }

    int handle_;
};