using Org.SbeTool.Sbe.Dll;
using MtService;
using Xunit;

namespace MtApi5.Tests
{
    // Round-trips through the SBE-generated codecs, which guard against
    // field order / type mismatches with specs/mtprotocol_sbe.xml.
    public class SbeCodecTests
    {
        [Fact]
        public void MtCommand_RoundTrip()
        {
            var bytes = new byte[256];
            using var buffer = new DirectBuffer(bytes);
            var header = new MessageHeader();

            var enc = new MtCommand().WrapForEncodeAndApplyHeader(buffer, 0, header);
            enc.Expert_handle = 42;
            enc.Command_id = 0x1_0000_0001UL;
            enc.Command_type = 77;
            enc.SetStringPayload("héllo");

            var decHeader = new MessageHeader();
            decHeader.Wrap(buffer, 0, MtCommand.SchemaVersion);
            Assert.Equal(MtCommand.TemplateId, decHeader.TemplateId);
            Assert.Equal(MtCommand.SchemaId, decHeader.SchemaId);

            var dec = new MtCommand().WrapForDecode(buffer, MessageHeader.Size,
                decHeader.BlockLength, decHeader.Version);
            Assert.Equal(42, dec.Expert_handle);
            Assert.Equal(0x1_0000_0001UL, dec.Command_id);
            Assert.Equal(77, dec.Command_type);
            Assert.Equal("héllo", dec.GetStringPayload());
        }

        [Fact]
        public void MtCommand_EmptyPayload_RoundTrip()
        {
            var bytes = new byte[64];
            using var buffer = new DirectBuffer(bytes);
            var enc = new MtCommand().WrapForEncodeAndApplyHeader(buffer, 0, new MessageHeader());
            enc.Expert_handle = 1;
            enc.Command_id = 2;
            enc.Command_type = 3;
            enc.SetStringPayload("");

            var dec = new MtCommand().WrapForDecode(buffer, MessageHeader.Size, MtCommand.BlockLength, MtCommand.SchemaVersion);
            Assert.Equal("", dec.GetStringPayload());
        }
    }
}
