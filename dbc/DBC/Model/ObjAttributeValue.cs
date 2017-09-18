using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class ObjAttributeValue
    {
        public string AttributeName { get; set; }
        public AttributeValue AttributeValue { get; set; }
        public string Type { get; set; } = "";
        public string Name { get; set; }
        public string MsgID { get; set; }
    }
}
