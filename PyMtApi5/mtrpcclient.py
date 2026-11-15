import logging
from threading import Condition, Lock, Thread

import websockets
from websockets.sync.client import connect as ws_connect

from .mtprotocol_sbe import (
    MtCommand,
    MtEvent,
    MtExpertAdded,
    MtExpertList,
    MtExpertRemoved,
    MtRequestExpertList,
    MtResponse,
    ResponseStatus,
    decode_message,
)

_UINT64_MASK = (1 << 64) - 1


class MtRpcError(Exception):
    """MtService could not deliver the command to MetaTrader, see status for the reason."""

    def __init__(self, status, message):
        super().__init__(message)
        self.status = status


class _ErrorResponse:
    # Response with status other than Ok: payload is error message from MtService
    def __init__(self, status, message):
        self.status = status
        self.message = message


class CommandTask:
    def __init__(self):
        self.waiter = Condition()
        self.response = None
        self.is_set = False

    def wait_response(self, timeout):
        with self.waiter:
            self.waiter.wait_for(lambda: self.is_set, timeout)
            return self.response

    def set_response(self, response):
        with self.waiter:
            self.response = response
            self.is_set = True
            self.waiter.notify_all()


class MtRpcClient:
    def __init__(self, callback=None):
        self.__logger = logging.getLogger(__name__)
        self.__callback = callback
        self.__tasks = dict()
        self.__next_command_id = 0
        self.__lock = Lock()

    def connect(self, url):
        self.__logger.debug(f"connecting to {url}")
        self.__ws = ws_connect(url)
        self.__receive_thread = Thread(target=self.__receive_messages_thread)
        self.__receive_thread.start()

    def disconnect(self):
        self.__ws.close()
        self.__receive_thread.join()
        self.__logger.debug("disconnected")

    def request_expert_list(self, timeout=10):
        request_id = self.__get_next_command_id()
        self.__logger.debug(f"request_expert_list: request_id = {request_id}")
        message = MtRequestExpertList(request_id).encode()
        return self.__send(message, request_id, timeout)

    def send_command(self, expert_handle, command_type, payload=None, timeout=10):
        """Send command to MetaTrader and wait for its response.

        Returns None if the response was not received within timeout.
        Raises MtRpcError if MtService could not deliver the command (no expert, expert not found or removed).
        """
        command_id = self.__get_next_command_id()
        self.__logger.debug(
            f"send_command: expert_handle = {expert_handle}, command_id = {command_id}, "
            f"command_type = {command_type}, payload = {payload}"
        )
        message = MtCommand(expert_handle, command_id, command_type, payload or "").encode()
        return self.__send(message, command_id, timeout)

    # Private methods

    def __get_next_command_id(self):
        with self.__lock:
            while True:
                command_id = self.__next_command_id
                self.__next_command_id = (self.__next_command_id + 1) & _UINT64_MASK
                if command_id not in self.__tasks:
                    # reserve id so concurrent callers do not get the same one
                    self.__tasks[command_id] = CommandTask()
                    return command_id

    def __send(self, message, command_id, timeout):
        with self.__lock:
            task = self.__tasks[command_id]
        try:
            self.__ws.send(message)
            response = task.wait_response(timeout)
        finally:
            with self.__lock:
                self.__tasks.pop(command_id, None)
        if isinstance(response, _ErrorResponse):
            raise MtRpcError(response.status, response.message)
        return response

    def __set_task_response(self, command_id, response):
        with self.__lock:
            task = self.__tasks.get(command_id)
        if task is not None:
            task.set_response(response)

    def __process_message(self, message):
        if not isinstance(message, (bytes, bytearray)):
            self.__logger.warning(f"process_message: unexpected text message: {message}")
            return
        self.__logger.debug(f"process_message: {len(message)} bytes")

        msg = decode_message(message)
        if isinstance(msg, MtResponse):
            self.__process_response(msg)
        elif isinstance(msg, MtEvent):
            self.__process_event(msg)
        elif isinstance(msg, MtExpertList):
            self.__process_expert_list(msg)
        elif isinstance(msg, MtExpertAdded):
            self.__process_expert_added(msg)
        elif isinstance(msg, MtExpertRemoved):
            self.__process_expert_removed(msg)
        else:
            self.__logger.warning(f"received unknown message: {msg}")

    def __process_response(self, msg):
        self.__logger.debug(
            f"process_response: {msg.expert_handle}, {msg.command_id}, {msg.status!r}, [{msg.string_payload}]"
        )
        if msg.status == ResponseStatus.Ok:
            response = msg.string_payload
        else:
            response = _ErrorResponse(msg.status, msg.string_payload)
        self.__set_task_response(msg.command_id, response)

    def __process_event(self, msg):
        self.__logger.debug(f"process_event: {msg.expert_handle}, {msg.event_type}, [{msg.string_payload}]")
        if self.__callback is not None:
            self.__callback.mt_rpc_on_event(msg.expert_handle, msg.event_type, msg.string_payload)

    def __process_expert_list(self, msg):
        self.__logger.debug(f"process_expert_list: request_id = {msg.request_id}, experts = {msg.expert_list}")
        # keep order, drop duplicates (C# client uses HashSet)
        experts = list(dict.fromkeys(msg.expert_list))
        self.__set_task_response(msg.request_id, experts)

    def __process_expert_added(self, msg):
        self.__logger.debug(f"process_expert_added: {msg.expert_handle}")
        if self.__callback is not None:
            self.__callback.mt_rpc_on_expert_added(msg.expert_handle)

    def __process_expert_removed(self, msg):
        self.__logger.debug(f"process_expert_removed: {msg.expert_handle}")
        if self.__callback is not None:
            self.__callback.mt_rpc_on_expert_removed(msg.expert_handle)

    def __receive_messages_thread(self):
        self.__logger.debug("started receive_messages thread")
        while True:
            try:
                message = self.__ws.recv()
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

            # A malformed message or an exception raised by a subscriber's callback
            # must not break the receive loop: log it and drop only this message.
            try:
                self.__process_message(message)
            except Exception:
                size = len(message) if isinstance(message, (bytes, bytearray, str)) else "?"
                self.__logger.exception(f"receive_messages: failed to process received message ({size} bytes)")
        self.__logger.debug("function receive_messages finished")
