using System;
using System.Collections.Generic;

#if FW35
using System.Xml.Linq;
#else
using System.Xml.XPath;
#endif

namespace D900Cdr.Schema
{
    using DefinitionsDictionary = Dictionary<string, ID900CdrElementDefinition>;

    [Serializable]
    public class D900CdrElementDefinition : ID900CdrElementDefinition
    {
        private string _path;
        private string _name;
        private string _parselet;
        private string _valueType;

        public D900CdrElementDefinition(string path, string name, string parselet, string valueType)
        {
            _path = path;
            _name = name;
            _parselet = parselet;
            _valueType = valueType;
        }

        public string Path { get { return _path; } }
        public string Name { get { return _name; } }
        public string Parselet { get { return _parselet; } }
        public string ValueType { get { return _valueType; } }
    }


    public class D900CdrDefinitionSchema
    {
        private string _schemaName;
        private DefinitionsDictionary _definitions;

        public D900CdrDefinitionSchema(string schemaName)
        {
            _schemaName = schemaName;
            _definitions = new Dictionary<string, ID900CdrElementDefinition>();
        }

        public string Name { get { return _schemaName; } }
        public DefinitionsDictionary Definitions { get { return _definitions; } }
    }

    public class D900CdrDefinitionProvider
    {
        private static D900CdrDefinitionProvider _provInstance;
        private static string _xmlSource = "D900Cdr.Elements.xml";
        private string _xmlVersion;
        private List<D900CdrDefinitionSchema> _schemaList;
        private int _currentSchema;
        private int _defaultSchema;

        private D900CdrDefinitionProvider(string xmlURI)
        {
            _schemaList = new List<D900CdrDefinitionSchema>();
            _defaultSchema = -1;
            _xmlVersion = String.Empty;

            bool def;

#if FW35
            XElement cdrXML = XDocument
                .Load(xmlURI, LoadOptions.None)
                .Element("D900Cdr");

            __xmlVersion = cdrXML.Attribute("Version").Value;

            foreach (XElement schXML in cdrXML.Elements("Schema"))
            {
                D900CdrDefinitionSchema sch = new D900CdrDefinitionSchema(schXML.Attribute("Name").Value);
                ReadDefinitions(sch.Definitions, schXML, String.Empty);
                _schemaList.Add(sch);
                Boolean.TryParse(schXML.Attribute("Default").Value, out def);
                if (def) _defaultSchema = _schemaList.Count - 1;
            }
#else
            XPathNavigator cdrXML = new XPathDocument(xmlURI)
                .CreateNavigator()
                .SelectSingleNode("D900Cdr");

            _xmlVersion = cdrXML.GetAttribute("Version", String.Empty);

            foreach (XPathNavigator schXML in cdrXML.Select("Schema"))
            {
                D900CdrDefinitionSchema sch = new D900CdrDefinitionSchema(schXML.GetAttribute("Name", String.Empty));
                ReadDefinitions(sch.Definitions, schXML, String.Empty);
                _schemaList.Add(sch);
                if (Boolean.TryParse(schXML.GetAttribute("Default", String.Empty), out def) && def)
                {
                    _defaultSchema = _schemaList.Count - 1;
                }
            }
#endif
            if ((_defaultSchema < 0) && (_schemaList.Count > 0))
                _defaultSchema = 0;
            _currentSchema = _defaultSchema;
        }

#if FW35
        private static void ReadDefinitions(DefinitionsDictionary definitions, XElement parent, string parentPath)
        {
            int pos;
            string path;
            string parseletValue;
            string parselet;
            string valueType;

            foreach (XElement _child in parent.Elements("Element"))
            {
                path = (parentPath == String.Empty) ? _child.Attribute("Tag").Value : String.Concat(parentPath, ".", _child.Attribute("Tag").Value);
                parseletValue = _child.Attribute("Parselet").Value;
                pos = parseletValue.IndexOf('.');
                if (pos >= 0)
                {
                    parselet = parseletValue.Substring(0, pos);
                    valueType = parseletValue.Substring(pos + 1);
                }
                else
                {
                    parselet = parseletValue;
                    valueType = String.Empty;
                }
                definitions.Add(path, new D900CdrElementDefinition(path, _child.Attribute("Name").Value, parselet, valueType));
                ReadDefinitions(definitions, _child, path);
            }
        }
#else
        private static void ReadDefinitions(DefinitionsDictionary definitions, XPathNavigator parent, string parentPath)
        {
            int pos;
            string path;
            string parseletValue;
            string parselet;
            string valueType;

            foreach (XPathNavigator _child in parent.SelectChildren("Element", String.Empty))
            {
                path = String.IsNullOrEmpty(parentPath) ? _child.GetAttribute("Tag", String.Empty) : String.Concat(parentPath, ".", _child.GetAttribute("Tag", String.Empty));
                parseletValue = _child.GetAttribute("Parselet", String.Empty);
                pos = parseletValue.IndexOf('.');
                if (pos >= 0)
                {
                    parselet = parseletValue.Substring(0, pos);
                    valueType = parseletValue.Substring(pos + 1);
                }
                else
                {
                    parselet = parseletValue;
                    valueType = String.Empty;
                }
                definitions.Add(path, new D900CdrElementDefinition(path, _child.GetAttribute("Name", String.Empty), parselet, valueType));
                ReadDefinitions(definitions, _child, path);
            }
        }
#endif

        public static D900CdrDefinitionProvider Instance
        {
            get
            {
                if (_provInstance == null)
                {
                    _provInstance = new D900CdrDefinitionProvider(_xmlSource);
                }
                return _provInstance;
            }
        }

        public string[] AvailableSchemas()
        {
            string[] sch = new string[_schemaList.Count];
            for (int idx = 0; idx < sch.Length; idx++)
            {
                sch[idx] = _schemaList[idx].Name;
            }
            return sch;
        }

        public D900CdrDefinitionSchema GetSchema(string name)
        {
            foreach (D900CdrDefinitionSchema schema in _schemaList)
                if (String.Equals(schema.Name, name, StringComparison.CurrentCultureIgnoreCase))
                    return schema;
            return null;
        }

        public string DefaultSchema
        {
            get
            {
                return (_defaultSchema >= 0) ? _schemaList[_defaultSchema].Name : String.Empty;
            }
        }

        public string CurrentSchema
        {
            get
            {
                return (_currentSchema >= 0) ? _schemaList[_currentSchema].Name : String.Empty;
            }

            set
            {
                foreach (D900CdrDefinitionSchema sch in _schemaList)
                {
                    if (String.Compare(sch.Name, value, true) == 0)
                    {
                        _currentSchema = _schemaList.IndexOf(sch);
                        break;
                    }
                }
            }
        }

        public ID900CdrElementDefinition FindDefinition(string path)
        {
            return _schemaList[_currentSchema].Definitions.ContainsKey(path) ? _schemaList[_currentSchema].Definitions[path] : null;
        }

        public string XmlVersion
        {
            get { return _xmlVersion; }
        }
    }
}
