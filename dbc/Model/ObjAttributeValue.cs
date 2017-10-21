using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public class ObjAttributeValue
    {
        public string AttributeName { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public double MsgID { get; set; }

        public AttributeValue Value { get; set; }
    }
}
