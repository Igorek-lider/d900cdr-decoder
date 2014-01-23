using System;

namespace D900Cdr.Decoder.Parselets
{
    class BcdTimeParselet : GenericParselet
    {
        public BcdTimeParselet()
            : base()
        {
            RegisterMethod("Time", ValueAsTime);
            DefaultValueType = "Time";
        }

        public static string ValueAsTime(byte[] value)
        {
            DateTime time = DateTime.ParseExact(BitConverter.ToString(value).Replace("-", String.Empty), "HHmmss", null);
            return time.ToString("HH:mm:ss");
        }
    }
}
