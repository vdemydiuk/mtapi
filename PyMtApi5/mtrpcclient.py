import logging
from enum import IntEnum
from threading import Condition, Lock, Thread

import websockets
from websockets.sync.client import connect as ws_connect


class MtNotification(IntEnum):
    ClientReady = 0

class MtMessageType(IntEnum):
    Command = 0
    Response = 1
    Event = 2
    ExpertList = 3
    ExpertAdded = 4
    ExpertRemoved = 5
    Notification = 6

class CommandTask:
    def __init__(self):
        self.locker = Lock()
        self.waiter = Condition()
        self.response = None

    def wait_response(self, time):
        with self.waiter:
            self.waiter.wait(time)
        with self.locker:
            return self.response

    def set_response(self, response):
        with self.locker:
            self.response = response
        with self.waiter:
            self.waiter.notify()

class MtRpcClient:
    def __init__(self, callback=None):
        self.__logger = logging.getLogger(__name__)
        self.__callback = callback
        self.__notification_tasks = dict()
        self.__tasks = dict()
        self.__next_command_id = 0
        self.__lock = Lock()

    def connect(self, url):
        self.__logger.debug(f"connecting to {url}")
        self.__ws = ws_connect(url);
        self.__receive_thread = Thread(target = self.__receive_messages_thread)
        self.__receive_thread.start()

    def disconnect(self):
        self.__ws.close()
        self.__receive_thread.join()
        self.__logger.debug("disconnected")

    def request_expert_list(self):
        task = CommandTask()
        with self.__lock:
            self.__notification_tasks[MtNotification.ClientReady] = task
        self.__ws.send(self.__create_notification(MtNotification.ClientReady))
        response = task.wait_response(10)
        with self.__lock:
            self.__notification_tasks.pop(MtNotification.ClientReady)
        return response

    def send_command(self, expert_handle, command_type, payload = None):
        command_id = self.__next_command_id
        self.__next_command_id += 1
        task = CommandTask()
        with self.__lock:
            self.__tasks[command_id] = task
        self.__ws.send(self.__create_mt_command(expert_handle, command_id, command_type, payload))
        response = task.wait_response(10)
        with self.__lock:
            self.__tasks.pop(command_id)
        return response

    # Private methods

    def __process_message(self, message):
        self.__logger.debug(f"process_message: {message}")
        pieces = message.split(';', 1)
        if len(pieces) != 2 or not pieces[0] or not pieces[1]:
            self.__logger.warning("process_message: Invalid message format");
            return
        message_type = MtMessageType(int(pieces[0]))
        if message_type == MtMessageType.ExpertList:
            self.__process_expert_list(pieces[1])
        elif message_type == MtMessageType.Event:
            self.__process_event(pieces[1])
        elif message_type == MtMessageType.Response:
            self.__process_response(pieces[1])
        elif message_type == MtMessageType.ExpertAdded:
            self.__process_expert_added(pieces[1])
        elif message_type == MtMessageType.ExpertRemoved:
            self.__process_expert_removed(pieces[1])
        else:
            self.__logger.warning(f"received unknown message type: {message_type}")

    def __process_expert_list(self, payload):
        pieces = payload.split(',')
        experts = list()
        for p in pieces:
            experts.append(int(p))
        with self.__lock:
            task = self.__notification_tasks.get(MtNotification.ClientReady)
            if task is not None:
                task.set_response(experts)

    def __process_event(self, payload):
        pieces = payload.split(';', 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            self.__logger.warning("process_event: Invalid message format");
            return
        if self.__callback is not None:
            self.__callback.mt_rpc_on_event(int(pieces[0]), int(pieces[1]), pieces[2])

    def __process_response(self, payload):
        pieces = payload.split(';', 2)
        if len(pieces) != 3 or not pieces[0] or not pieces[1] or not pieces[2]:
            self.__logger.warning("process_response: Invalid message format");
            return
        command_id = int(pieces[1])
        with self.__lock:
            task = self.__tasks.get(command_id)
            if task is not None:
                task.set_response(pieces[2])

    def __process_expert_added(self, payload):
        if self.__callback is not None:
            self.__callback.mt_rpc_on_expert_added(int(payload))

    def __process_expert_removed(self, payload):
        if self.__callback is not None:
            self.__callback.mt_rpc_on_expert_removed(int(payload))

    def __receive_messages_thread(self):
        self.__logger.debug("started receive_messages thread")
        while True:
            try:
                message = self.__ws.recv()
                self.__process_message(message)
            except websockets.exceptions.ConnectionClosed:
                self.__logger.info("Connection closed")
                if self.__callback is not None:
                    self.__callback.mt_rcp_on_disconnect()
                break
            except Exception as e:
                self.__logger.error(e)
                if self.__callback is not None:
                    self.__callback.mt_rpc_on_connection_failed(str(e))
                break
        self.__logger.debug("function receive_messages finished")

    def __create_notification(self, notification_type):
        return f"{int(MtMessageType.Notification)};{notification_type}"

    def __create_mt_command(self, expert_handle, command_id, command_type, payload):
        if (payload is None):
            return f"{MtMessageType.Command};{expert_handle};{command_id};{command_type}";
        return f"{MtMessageType.Command};{expert_handle};{command_id};{command_type};{payload}";
