using System;

namespace D900Cdr.Decoder.Parselets
{
    // internal structure : YY, MM, DD
    class BcdDateParselet : GenericParselet
    {
        public BcdDateParselet()
            : base()
        {
            RegisterMethod("Date", ValueAsDate);
            DefaultValueType = "Date";
        }

        public static string ValueAsDate(byte[] value)
        {
            DateTime date = DateTime.ParseExact(BitConverter.ToString(value).Replace("-", String.Empty), "yyMMdd", null);
            return date.ToString("dd.MM.yyyy");
        }
    }
}
