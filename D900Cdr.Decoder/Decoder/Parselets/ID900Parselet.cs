using System;

namespace D900Cdr.Decoder.Parselets
{
    public interface ID900Parselet
    {
        string[] GetValueTypes();
        string DefaultValue(D900CdrElement element);
        string Value(string valueType, D900CdrElement element);
        string ParseletName { get; }
        string DefaultValueType { get; }
    }
}
