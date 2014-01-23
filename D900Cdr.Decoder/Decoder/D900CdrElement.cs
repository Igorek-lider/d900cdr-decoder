using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using D900Cdr.Decoder.BER;
using D900Cdr.Decoder.Parselets;

namespace D900Cdr.Decoder
{
    public class D900CdrElement
    {
        private long _offset;
        private string _path = String.Empty;
        private string _name = String.Empty;
        private bool _isConstructed;
        private object _value;
        private ID900Parselet _parselet;
        private string _defValueType = String.Empty;

        public long Offset { get { return _offset; } }
        public string Path { get { return _path; } }
        public string Name { get { return _name; } set { _name = value; } }
        public bool IsConstructed { get { return _isConstructed; } }
        public object Value { get { return _value; } }

        public ID900Parselet Parselet
        {
            get { return _parselet; }
            set
            {
                _parselet = value;
                _defValueType = String.Empty;
            }
        }

        public string DefaultValueType
        {
            get
            {
                if ((_defValueType != null) && (_defValueType.Length > 0))
                {
                    return _defValueType;
                }
                else
                {
                    return (_parselet != null) ? _parselet.DefaultValueType : String.Empty;
                }
            }
            set { _defValueType = value; }
        }

        public D900CdrElement()
        {
        }

        public static D900CdrElement CreateFromTlv(TlvObject tlv)
        {
            D900CdrElement element = new D900CdrElement();
            element._offset = tlv.Offset;
            element._path = tlv.Path;
            element._name = tlv.Path;
            element._isConstructed = tlv.IsConstructed;
            if (!tlv.IsEmpty)
            {
                if (tlv.IsConstructed)
                {
                    List<D900CdrElement> val = new List<D900CdrElement>((tlv.Value as List<TlvObject>).Count);
                    foreach (TlvObject ch in (tlv.Value as List<TlvObject>))
                    {
                        val.Add(D900CdrElement.CreateFromTlv(ch));
                    };
                    element._value = val;
                }
                else
                {
                    byte[] src = (tlv.Value as byte[]);
                    byte[] dst = new byte[src.Length];
                    Array.Copy(src, dst, src.Length);
                    element._value = dst;
                }
            }

            return element;
        }

        public bool IsEmpty
        {
            get
            {
                if (_value == null)
                {
                    return true;
                }
                else if (_isConstructed)
                {
                    return (_value as List<D900CdrElement>).Count > 0 ? false : true;
                }
                else
                {
                    return ((_value as byte[]).Length == 0);
                }
            }
        }

        public IList<D900CdrElement> GetAllChilds()
        {
            if (!this.IsConstructed)
                return null;

            List<D900CdrElement> allChilds = new List<D900CdrElement>(1);
            IList<D900CdrElement> td;

            foreach (D900CdrElement child in (this.Value as List<D900CdrElement>))
            {
                if (child.IsConstructed && !child.IsEmpty)
                {
                    td = child.GetAllChilds();
                    if (td != null)
                    {
                        allChilds.AddRange(td);
                    }
                }
                else
                {
                    allChilds.Add(child);
                }
            }
            return (allChilds.Count > 0) ? allChilds.AsReadOnly() : null;
        }

        public D900CdrElement[] FindChild(string path)
        {
            IList<D900CdrElement> childs = this.GetAllChilds();
            if (childs == null)
            {
                return null;
            }
            else
            {
                List<D900CdrElement> result = new List<D900CdrElement>();
                foreach (D900CdrElement element in childs)
                    if (String.Equals(element.Path, path))
                        result.Add(element);
                return (result.Count > 0) ? result.ToArray() : null;
            }
        }

        public override string ToString()
        {
            StringBuilder record = new StringBuilder(String.Format("{0}=", this.Name));
            if (this.IsConstructed)
            {
                record.Append('[');
                if (!this.IsEmpty)
                {
                    for (int i = 0; i < (this.Value as List<D900CdrElement>).Count; i++)
                    {
                        if (i > 0) record.Append(' ');
                        record.Append((this.Value as List<D900CdrElement>)[i].ToString());
                    }
                }
                record.Append(']');
            }
            else
            {
                record.AppendFormat("\"{0}\"", this.Parselet.DefaultValue(this));
            }

            return record.ToString();
        }

        public void DumpToXml(TextWriter dumpWriter, int ident)
        {
            for (int i = ident; i > 0; i--) { dumpWriter.Write("\t"); }
            dumpWriter.Write("<Element=\"{0} ({1})\" Offset=\"{2}\""
                , this.Path
                , this.Name
                , this.Offset);
            if (this.IsConstructed)
            {
                dumpWriter.Write(">");
            }
            else
            {
                dumpWriter.Write(" Parselet=\"{0}\">", this.Parselet.ParseletName);
            }
            if (this.IsEmpty)
            {
                dumpWriter.WriteLine("</Element>");
            }
            else if (this.IsConstructed)
            {
                dumpWriter.WriteLine();
                foreach (D900CdrElement element in (this.Value as List<D900CdrElement>)) { element.DumpToXml(dumpWriter, ident + 1); };
                for (int i = ident; i > 0; i--) { dumpWriter.Write("\t"); }
                dumpWriter.WriteLine("</Element>");
            }
            else
            {
                dumpWriter.Write("Default=\"{0}\"", this.DefaultValueType);
                foreach (string key in this.Parselet.GetValueTypes())
                {
                    dumpWriter.Write(" {0}=\"{1}\"", key, this.Parselet.Value(key, this));
                }
                dumpWriter.WriteLine("</Element>");
            }
        }

        public void DumpToTxt(TextWriter dumpWriter, string leftHeader)
        {
            dumpWriter.Write("{0} {1}", leftHeader, this.ToString());
        }
    }
}
