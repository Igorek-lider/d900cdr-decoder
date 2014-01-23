using System;
using System.Collections.Generic;
using System.Reflection;

namespace D900Cdr.Decoder.Parselets
{
    public class D900ParseletProvider
    {
        private static D900ParseletProvider _provInstance;
        private Dictionary<string, ID900Parselet> _parseletTable;

        private D900ParseletProvider()
        {
            _parseletTable = new Dictionary<string, ID900Parselet>();
            this.RegisterAssemblyParselets(Assembly.GetExecutingAssembly());
        }

        public static D900ParseletProvider Instance
        {
            get
            {
                if (_provInstance == null)
                {
                    _provInstance = new D900ParseletProvider();
                }
                return _provInstance;
            }
        }

        protected void RegisterAssemblyParselets(Assembly asm)
        {
            ID900Parselet parselet;

            foreach (Type t in asm.GetTypes())
            {
                if (t.GetInterface(typeof(ID900Parselet).Name) != null)
                {
                    parselet = (asm.CreateInstance(t.FullName, true) as ID900Parselet);
                    this.RegisterParselet(parselet);
                }
            }
        }

        public void RegisterParselet(ID900Parselet parselet)
        {
            if (!_parseletTable.ContainsKey(parselet.ParseletName))
            {
                _parseletTable.Add(parselet.ParseletName, parselet);
            }
        }

        public ID900Parselet FindParselet(string parseletName)
        {
            return _parseletTable.ContainsKey(parseletName) ? _parseletTable[parseletName] : null;
        }
    }
}
