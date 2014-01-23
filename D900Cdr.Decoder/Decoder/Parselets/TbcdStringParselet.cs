using System;

namespace D900Cdr.Decoder.Parselets
{
    /*
     * TBCD-string: Technical Binary Coded Digits.
     * The octetstring contains the digits 0 to F, two digits per octet. 
     * Bit 4 to 1 of octet n encoding digit 2(n-1)+1, Bit 5 to 8 of octet n encoding digit 2n,
     * bit 8 being most significant bit.
     * e.g. number = 12345 stored in following order : 2143F5FFFF... ( FF... only for fixed length)
     */

    class TbcdStringParselet : GenericParselet
    {
        public TbcdStringParselet()
            : base()
        {
            RegisterMethod("TBCD", ValueAsTBCDString);
            DefaultValueType = "TBCD";
        }

        public static string ValueAsTBCDString(byte[] value)
        {
            string res = String.Empty;

            byte msb;
            byte lsb;

            foreach (byte x in value)
            {
                msb = (byte)((x & 0xF0) >> 4);
                lsb = (byte)(x & 0xF);

                if (lsb != 0xF)
                {
                    if (lsb > 9)
                    {
                    }
                    else
                    {
                        res += lsb.ToString();
                    }

                    if (msb != 0xF)
                    {
                        if (msb > 9)
                        {
                        }
                        else
                        {
                            res += msb.ToString();
                        }
                    }
                }

                if ((lsb == 0xF) || (msb == 0xF))
                    break;
            }

            return res;
        }
    }
}
