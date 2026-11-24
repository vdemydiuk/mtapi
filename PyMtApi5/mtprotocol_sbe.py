"""SBE (Simple Binary Encoding) codec for the MtService protocol.

Hand-written implementation of specs/mtprotocol_sbe.xml
(package="MtService", schema id=1, version=1, byteOrder=littleEndian).
Keep it in sync with the spec and the generated C# (MtClient/*.g.cs) / C++ (MtService/*.h) codecs.
"""

import struct
from dataclasses import dataclass, field
from enum import IntEnum

SCHEMA_ID = 1
SCHEMA_VERSION = 1

# messageHeader: blockLength(u16) templateId(u16) schemaId(u16) version(u16)
_HEADER = struct.Struct("<HHHH")
HEADER_SIZE = _HEADER.size  # 8

# varDataEncoding: length(u32) followed by UTF-8 bytes
_VAR_DATA_LENGTH = struct.Struct("<I")

# groupSizeEncoding: blockLength(u16) numInGroup(u16)
_GROUP_SIZE = struct.Struct("<HH")


class SbeDecodeError(Exception):
    pass


class ResponseStatus(IntEnum):
    """enum ResponseStatus (int32): result of command delivery by MtService."""

    Ok = 0  # command was executed by MetaTrader, payload contains its response
    NoExperts = 1  # there is no expert attached to MtService to execute the command
    ExpertNotFound = 2  # expert with requested handle is not found
    ExpertRemoved = 3  # expert was removed before the command was executed


def _decode_response_status(value):
    # Values unknown to this version are kept as plain int (like the generated C# codec does)
    try:
        return ResponseStatus(value)
    except ValueError:
        return value


@dataclass
class MessageHeader:
    block_length: int
    template_id: int
    schema_id: int
    version: int

    def encode(self):
        return _HEADER.pack(self.block_length, self.template_id, self.schema_id, self.version)

    @staticmethod
    def decode(buffer, offset=0):
        if len(buffer) < offset + HEADER_SIZE:
            raise SbeDecodeError("Buffer underflow: complete header not received")
        return MessageHeader(*_HEADER.unpack_from(buffer, offset))


def _encode_var_data(value):
    data = b"" if value is None else value.encode("utf-8")
    return _VAR_DATA_LENGTH.pack(len(data)) + data


def _decode_var_data(buffer, offset):
    if len(buffer) < offset + _VAR_DATA_LENGTH.size:
        raise SbeDecodeError("Buffer underflow: var data length")
    (length,) = _VAR_DATA_LENGTH.unpack_from(buffer, offset)
    start = offset + _VAR_DATA_LENGTH.size
    end = start + length
    if len(buffer) < end:
        raise SbeDecodeError("Buffer underflow: var data exceeds buffer")
    return bytes(buffer[start:end]).decode("utf-8"), end


def _check_block(buffer, offset, block_length, min_length, name):
    if block_length < min_length:
        raise SbeDecodeError(f"{name}: block length {block_length} is less than {min_length}")
    if len(buffer) < offset + block_length:
        raise SbeDecodeError(f"{name}: buffer underflow in root block")


def _header_for(cls):
    return MessageHeader(cls.BLOCK_LENGTH, cls.TEMPLATE_ID, SCHEMA_ID, SCHEMA_VERSION).encode()


# ---------------------------------------------------------------------------
# Messages
# ---------------------------------------------------------------------------


@dataclass
class MtCommand:
    """id=100: Carry command payload to MetaTrader."""

    TEMPLATE_ID = 100
    _BLOCK = struct.Struct("<iQi")  # expert_handle, command_id, command_type
    BLOCK_LENGTH = _BLOCK.size  # 16

    expert_handle: int
    command_id: int
    command_type: int
    string_payload: str = ""

    def encode(self):
        return (
            _header_for(MtCommand)
            + self._BLOCK.pack(self.expert_handle, self.command_id, self.command_type)
            + _encode_var_data(self.string_payload)
        )

    @staticmethod
    def decode(buffer, offset, block_length):
        _check_block(buffer, offset, block_length, MtCommand.BLOCK_LENGTH, "MtCommand")
        expert_handle, command_id, command_type = MtCommand._BLOCK.unpack_from(buffer, offset)
        payload, _ = _decode_var_data(buffer, offset + block_length)
        return MtCommand(expert_handle, command_id, command_type, payload)


@dataclass
class MtResponse:
    """id=101: Carry response payload from MetaTrader.

    string_payload is MetaTrader response if status is Ok, otherwise error message from MtService.
    """

    TEMPLATE_ID = 101
    _BLOCK = struct.Struct("<iQi")  # expert_handle, command_id, status
    BLOCK_LENGTH = _BLOCK.size  # 16

    expert_handle: int
    command_id: int
    status: ResponseStatus = ResponseStatus.Ok
    string_payload: str = ""

    def encode(self):
        return (
            _header_for(MtResponse)
            + self._BLOCK.pack(self.expert_handle, self.command_id, int(self.status))
            + _encode_var_data(self.string_payload)
        )

    @staticmethod
    def decode(buffer, offset, block_length):
        _check_block(buffer, offset, block_length, MtResponse.BLOCK_LENGTH, "MtResponse")
        expert_handle, command_id, status = MtResponse._BLOCK.unpack_from(buffer, offset)
        payload, _ = _decode_var_data(buffer, offset + block_length)
        return MtResponse(expert_handle, command_id, _decode_response_status(status), payload)


@dataclass
class MtEvent:
    """id=102: Carry event payload from MetaTrader."""

    TEMPLATE_ID = 102
    _BLOCK = struct.Struct("<ii")  # expert_handle, event_type
    BLOCK_LENGTH = _BLOCK.size  # 8

    expert_handle: int
    event_type: int
    string_payload: str = ""

    def encode(self):
        return (
            _header_for(MtEvent)
            + self._BLOCK.pack(self.expert_handle, self.event_type)
            + _encode_var_data(self.string_payload)
        )

    @staticmethod
    def decode(buffer, offset, block_length):
        _check_block(buffer, offset, block_length, MtEvent.BLOCK_LENGTH, "MtEvent")
        expert_handle, event_type = MtEvent._BLOCK.unpack_from(buffer, offset)
        payload, _ = _decode_var_data(buffer, offset + block_length)
        return MtEvent(expert_handle, event_type, payload)


@dataclass
class MtExpertList:
    """id=103: Provide expert list linked to MetaTrader."""

    TEMPLATE_ID = 103
    _BLOCK = struct.Struct("<Q")  # request_id
    BLOCK_LENGTH = _BLOCK.size  # 8
    _GROUP_ENTRY = struct.Struct("<i")  # expert_handle
    GROUP_BLOCK_LENGTH = _GROUP_ENTRY.size  # 4

    request_id: int
    expert_list: list = field(default_factory=list)

    def encode(self):
        parts = [
            _header_for(MtExpertList),
            self._BLOCK.pack(self.request_id),
            _GROUP_SIZE.pack(self.GROUP_BLOCK_LENGTH, len(self.expert_list)),
        ]
        parts.extend(self._GROUP_ENTRY.pack(h) for h in self.expert_list)
        return b"".join(parts)

    @staticmethod
    def decode(buffer, offset, block_length):
        _check_block(buffer, offset, block_length, MtExpertList.BLOCK_LENGTH, "MtExpertList")
        (request_id,) = MtExpertList._BLOCK.unpack_from(buffer, offset)
        pos = offset + block_length
        if len(buffer) < pos + _GROUP_SIZE.size:
            raise SbeDecodeError("MtExpertList: buffer underflow in group header")
        entry_length, count = _GROUP_SIZE.unpack_from(buffer, pos)
        pos += _GROUP_SIZE.size
        if entry_length < MtExpertList.GROUP_BLOCK_LENGTH:
            raise SbeDecodeError(f"MtExpertList: group block length {entry_length} is too small")
        if len(buffer) < pos + entry_length * count:
            raise SbeDecodeError("MtExpertList: buffer underflow in group entries")
        experts = []
        for _ in range(count):
            (handle,) = MtExpertList._GROUP_ENTRY.unpack_from(buffer, pos)
            experts.append(handle)
            pos += entry_length
        return MtExpertList(request_id, experts)


@dataclass
class MtExpertAdded:
    """id=104: Event about adding expert into MetaTrader."""

    TEMPLATE_ID = 104
    _BLOCK = struct.Struct("<i")  # expert_handle
    BLOCK_LENGTH = _BLOCK.size  # 4

    expert_handle: int

    def encode(self):
        return _header_for(MtExpertAdded) + self._BLOCK.pack(self.expert_handle)

    @staticmethod
    def decode(buffer, offset, block_length):
        _check_block(buffer, offset, block_length, MtExpertAdded.BLOCK_LENGTH, "MtExpertAdded")
        return MtExpertAdded(*MtExpertAdded._BLOCK.unpack_from(buffer, offset))


@dataclass
class MtExpertRemoved:
    """id=105: Event about removing expert from MetaTrader."""

    TEMPLATE_ID = 105
    _BLOCK = struct.Struct("<i")  # expert_handle
    BLOCK_LENGTH = _BLOCK.size  # 4

    expert_handle: int

    def encode(self):
        return _header_for(MtExpertRemoved) + self._BLOCK.pack(self.expert_handle)

    @staticmethod
    def decode(buffer, offset, block_length):
        _check_block(buffer, offset, block_length, MtExpertRemoved.BLOCK_LENGTH, "MtExpertRemoved")
        return MtExpertRemoved(*MtExpertRemoved._BLOCK.unpack_from(buffer, offset))


@dataclass
class MtRequestExpertList:
    """id=106: Internal request of experts to the MtApiService."""

    TEMPLATE_ID = 106
    _BLOCK = struct.Struct("<Q")  # request_id
    BLOCK_LENGTH = _BLOCK.size  # 8

    request_id: int

    def encode(self):
        return _header_for(MtRequestExpertList) + self._BLOCK.pack(self.request_id)

    @staticmethod
    def decode(buffer, offset, block_length):
        _check_block(buffer, offset, block_length, MtRequestExpertList.BLOCK_LENGTH, "MtRequestExpertList")
        return MtRequestExpertList(*MtRequestExpertList._BLOCK.unpack_from(buffer, offset))


_MESSAGES = {
    cls.TEMPLATE_ID: cls
    for cls in (
        MtCommand,
        MtResponse,
        MtEvent,
        MtExpertList,
        MtExpertAdded,
        MtExpertRemoved,
        MtRequestExpertList,
    )
}


def decode_message(buffer):
    """Decode a complete SBE message (header + body).

    Returns the decoded message object, or None if the template id is unknown.
    Raises SbeDecodeError on malformed input or a schema mismatch.
    """
    header = MessageHeader.decode(buffer)
    if header.schema_id != SCHEMA_ID:
        raise SbeDecodeError(f"Unexpected SBE schema ID: {header.schema_id}")
    cls = _MESSAGES.get(header.template_id)
    if cls is None:
        return None
    return cls.decode(buffer, HEADER_SIZE, header.block_length)
