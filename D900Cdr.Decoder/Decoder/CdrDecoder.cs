using System.IO;
using D900Cdr.Decoder.BER;
using D900Cdr.Decoder.Parselets;
using D900Cdr.Schema;

namespace D900Cdr.Decoder
{
    public partial class CdrDecoder : BerDecoder
    {
        private D900CdrTlvParser _tlvParser;

        public CdrDecoder()
        {
            _tlvParser = new D900CdrTlvParser(D900CdrDefinitionProvider.Instance, D900ParseletProvider.Instance);
        }

        public D900CdrDefinitionProvider ElementDefinitionProvider { get { return _tlvParser.DefinitionProvider; } }

        public D900CdrElement DecodeRecord(Stream asnStream, bool skipFillers)
        {
            TlvObject tlv;
            long offset = asnStream.Position;

            // Verify that the next byte - Billing Record Tag (0xE1)
            if (!skipFillers)
            {
                int b = asnStream.ReadByte();
                if (b != 0xE1)
                {
                    return null;
                }
                asnStream.Seek(-1, SeekOrigin.Current);
            }

            BerDecoderResult pr = DecodeTlv(asnStream, out tlv, ref offset, 0, byte.MaxValue);

            D900CdrElement record = null;

            if (pr == BerDecoderResult.Finished)
            {

                record = _tlvParser.ParseTlvObject(tlv); ;
            }

            return record;
        }
    }
}
