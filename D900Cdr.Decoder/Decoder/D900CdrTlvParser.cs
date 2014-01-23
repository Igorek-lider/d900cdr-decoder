using System.Collections.Generic;
using D900Cdr.Decoder.BER;
using D900Cdr.Decoder.Parselets;
using D900Cdr.Schema;

namespace D900Cdr.Decoder
{
    class D900CdrTlvParser
    {
        private D900CdrDefinitionProvider _definitionProvider;
        private D900ParseletProvider _parseletProv;

        public D900CdrTlvParser(D900CdrDefinitionProvider definitionProvider, D900ParseletProvider parseletProv)
        {
            _definitionProvider = definitionProvider;
            _parseletProv = parseletProv;
        }

        public D900CdrDefinitionProvider DefinitionProvider { get { return _definitionProvider; } }
        public D900ParseletProvider ParseletProvider { get { return _parseletProv; } }

        private void ParseElement(D900CdrElement element)
        {
            ID900CdrElementDefinition elementDef = DefinitionProvider.FindDefinition(element.Path);
            if (elementDef != null)
            {
                if (elementDef.Name.Length != 0)
                {
                    element.Name = elementDef.Name;
                }

                if (elementDef.Parselet.Length > 0)
                {
                    ID900Parselet parselet = ParseletProvider.FindParselet(elementDef.Parselet);
                    if (parselet != null)
                    {
                        element.Parselet = parselet;
                        element.DefaultValueType = elementDef.ValueType;
                    }
                }
            }

            if (element.Parselet == null && !element.IsConstructed)
            {
                element.Parselet = D900ParseletProvider.Instance.FindParselet("GenericParselet");
            }

            if ((element.IsConstructed) && (!element.IsEmpty))
            {
                foreach (D900CdrElement e in (element.Value as List<D900CdrElement>))
                {
                    this.ParseElement(e);
                }
            }
        }

        public D900CdrElement ParseTlvObject(TlvObject tlv)
        {
            D900CdrElement element = D900CdrElement.CreateFromTlv(tlv);

            this.ParseElement(element);

            return element;
        }
    }
}
