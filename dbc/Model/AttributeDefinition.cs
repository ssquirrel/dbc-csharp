using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public class AttributeDefinition
    {
        public string ObjectType { get; set; }
        public string AttributeName { get; set; }
        public string ValueType { get; set; }
        public double Num1 { get; set; }
        public double Num2 { get; set; }
        public IList<String> Values { get; set; }
    }
}
