using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class AttributeDefinition
    {
        public string objectType;
        public string attributeName;
        public string valueType;
        public List<String> values = new List<string>();
    }
}
