using Org.SbeTool.Sbe.Dll;
using MtService;
using Xunit;

namespace MtApi5.Tests
{
    // Covers the remaining messages of specs/mtprotocol_sbe.xml (MtCommand is in SbeCodecTests).
    // Ids and block lengths are asserted explicitly: they are part of the wire format shared
    // with MtService (C++) and PyMtApi5/mtprotocol_sbe.py, so any drift must fail a test.
    public class SbeSchemaTests
    {
        [Theory]
        [InlineData(100, 16, MtCommand.TemplateId, MtCommand.BlockLength, MtCommand.SchemaId, MtCommand.SchemaVersion)]
        [InlineData(101, 16, MtResponse.TemplateId, MtResponse.BlockLength, MtResponse.SchemaId, MtResponse.SchemaVersion)]
        [InlineData(102, 8, MtEvent.TemplateId, MtEvent.BlockLength, MtEvent.SchemaId, MtEvent.SchemaVersion)]
        [InlineData(103, 8, MtExpertList.TemplateId, MtExpertList.BlockLength, MtExpertList.SchemaId, MtExpertList.SchemaVersion)]
        [InlineData(104, 4, MtExpertAdded.TemplateId, MtExpertAdded.BlockLength, MtExpertAdded.SchemaId, MtExpertAdded.SchemaVersion)]
        [InlineData(105, 4, MtExpertRemoved.TemplateId, MtExpertRemoved.BlockLength, MtExpertRemoved.SchemaId, MtExpertRemoved.SchemaVersion)]
        [InlineData(106, 8, MtRequestExpertList.TemplateId, MtRequestExpertList.BlockLength, MtRequestExpertList.SchemaId, MtRequestExpertList.SchemaVersion)]
        public void Message_Ids_And_BlockLengths_MatchSpec(int id, int blockLength,
            int actualId, int actualBlockLength, int schemaId, int schemaVersion)
        {
            Assert.Equal(id, actualId);
            Assert.Equal(blockLength, actualBlockLength);
            Assert.Equal(1, schemaId);
            Assert.Equal(1, schemaVersion);
        }

        [Fact]
        public void MessageHeader_Size_Is8()
        {
            Assert.Equal(8, MessageHeader.Size);
        }

        [Fact]
        public void ResponseStatus_Values_MatchSpec()
        {
            Assert.Equal(0, (int)ResponseStatus.Ok);
            Assert.Equal(1, (int)ResponseStatus.NoExperts);
            Assert.Equal(2, (int)ResponseStatus.ExpertNotFound);
            Assert.Equal(3, (int)ResponseStatus.ExpertRemoved);
        }

        [Theory]
        [InlineData(ResponseStatus.Ok, "{\"ErrorCode\":0}")]
        [InlineData(ResponseStatus.NoExperts, "No experts")]
        [InlineData(ResponseStatus.ExpertNotFound, "")]
        [InlineData(ResponseStatus.ExpertRemoved, "удалён")]
        public void MtResponse_RoundTrip(ResponseStatus status, string payload)
        {
            var buffer = new DirectBuffer(new byte[256]);
            var enc = new MtResponse().WrapForEncodeAndApplyHeader(buffer, 0, new MessageHeader());
            enc.Expert_handle = -5;
            enc.Command_id = ulong.MaxValue - 1;
            enc.Status = status;
            enc.SetStringPayload(payload);

            var h = new MessageHeader();
            h.Wrap(buffer, 0, MtResponse.SchemaVersion);
            Assert.Equal(MtResponse.TemplateId, h.TemplateId);
            Assert.Equal(MtResponse.BlockLength, h.BlockLength);

            var dec = new MtResponse().WrapForDecode(buffer, MessageHeader.Size, h.BlockLength, h.Version);
            Assert.Equal(-5, dec.Expert_handle);
            Assert.Equal(ulong.MaxValue - 1, dec.Command_id);
            Assert.Equal(status, dec.Status);
            Assert.Equal(payload, dec.GetStringPayload());
        }

        [Fact]
        public void MtResponse_WireLayout_IsLittleEndian_FixedOffsets()
        {
            var bytes = new byte[64];
            var buffer = new DirectBuffer(bytes);
            var enc = new MtResponse().WrapForEncodeAndApplyHeader(buffer, 0, new MessageHeader());
            enc.Expert_handle = 0x01020304;
            enc.Command_id = 0x1112131415161718UL;
            enc.Status = ResponseStatus.ExpertRemoved;
            enc.SetStringPayload("ab");

            int o = MessageHeader.Size;
            Assert.Equal(new byte[] { 0x04, 0x03, 0x02, 0x01 }, bytes[o..(o + 4)]);
            Assert.Equal(new byte[] { 0x18, 0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11 }, bytes[(o + 4)..(o + 12)]);
            Assert.Equal(new byte[] { 3, 0, 0, 0 }, bytes[(o + 12)..(o + 16)]);
            // var data: uint32 length followed by UTF-8 bytes
            Assert.Equal(new byte[] { 2, 0, 0, 0, (byte)'a', (byte)'b' }, bytes[(o + 16)..(o + 22)]);
        }

        [Theory]
        [InlineData(1, "{\"Symbol\":\"EURUSD\"}")]
        [InlineData(int.MaxValue, "")]
        public void MtEvent_RoundTrip(int eventType, string payload)
        {
            var buffer = new DirectBuffer(new byte[128]);
            var enc = new MtEvent().WrapForEncodeAndApplyHeader(buffer, 0, new MessageHeader());
            enc.Expert_handle = 9;
            enc.Event_type = eventType;
            enc.SetStringPayload(payload);

            var dec = new MtEvent().WrapForDecode(buffer, MessageHeader.Size, MtEvent.BlockLength, MtEvent.SchemaVersion);
            Assert.Equal(9, dec.Expert_handle);
            Assert.Equal(eventType, dec.Event_type);
            Assert.Equal(payload, dec.GetStringPayload());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void MtExpertList_RoundTrip(int count)
        {
            var buffer = new DirectBuffer(new byte[256]);
            var enc = new MtExpertList().WrapForEncodeAndApplyHeader(buffer, 0, new MessageHeader());
            enc.Request_id = 0xABCDEF0123456789UL;
            var group = enc.Expert_listCount(count);
            for (int i = 0; i < count; i++)
                group.Next().Expert_handle = 100 + i;

            var dec = new MtExpertList().WrapForDecode(buffer, MessageHeader.Size, MtExpertList.BlockLength, MtExpertList.SchemaVersion);
            Assert.Equal(0xABCDEF0123456789UL, dec.Request_id);
            var list = dec.Expert_list;
            Assert.Equal(count, list.Count);
            var handles = new List<int>();
            while (list.HasNext)
                handles.Add(list.Next().Expert_handle);
            Assert.Equal(Enumerable.Range(100, count), handles);
        }

        [Fact]
        public void MtExpertList_Group_BlockLengthIs4()
        {
            Assert.Equal(4, MtExpertList.Expert_listGroup.SbeBlockLength);
        }

        [Fact]
        public void MtExpertAdded_RoundTrip()
        {
            var buffer = new DirectBuffer(new byte[32]);
            var enc = new MtExpertAdded().WrapForEncodeAndApplyHeader(buffer, 0, new MessageHeader());
            enc.Expert_handle = 123;
            var dec = new MtExpertAdded().WrapForDecode(buffer, MessageHeader.Size, MtExpertAdded.BlockLength, MtExpertAdded.SchemaVersion);
            Assert.Equal(123, dec.Expert_handle);
        }

        [Fact]
        public void MtExpertRemoved_RoundTrip()
        {
            var buffer = new DirectBuffer(new byte[32]);
            var enc = new MtExpertRemoved().WrapForEncodeAndApplyHeader(buffer, 0, new MessageHeader());
            enc.Expert_handle = -1;
            var dec = new MtExpertRemoved().WrapForDecode(buffer, MessageHeader.Size, MtExpertRemoved.BlockLength, MtExpertRemoved.SchemaVersion);
            Assert.Equal(-1, dec.Expert_handle);
        }

        [Fact]
        public void MtRequestExpertList_RoundTrip()
        {
            var buffer = new DirectBuffer(new byte[32]);
            var enc = new MtRequestExpertList().WrapForEncodeAndApplyHeader(buffer, 0, new MessageHeader());
            enc.Request_id = ulong.MaxValue - 1;
            var dec = new MtRequestExpertList().WrapForDecode(buffer, MessageHeader.Size, MtRequestExpertList.BlockLength, MtRequestExpertList.SchemaVersion);
            Assert.Equal(ulong.MaxValue - 1, dec.Request_id);
        }
    }
}
