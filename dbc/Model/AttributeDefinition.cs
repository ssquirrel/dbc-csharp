using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class AttributeDefinition
    {
        public class ValueType
        {
            public const string INT = "INT";
            public const string HEX = "HEX";
            public const string FLOAT = "FLOAT";
            public const string STRING = "STRING";
            public const string ENUM = "ENUM";
        }

        public string objectType;
        public string attributeName;
        public string valueType;
        public List<String> values;
    }
}
