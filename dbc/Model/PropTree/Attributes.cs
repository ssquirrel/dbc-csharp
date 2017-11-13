using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    class Attributes : IAttributes
    {
        private List<ObjAttributeValue> list = new List<ObjAttributeValue>();

        public Attributes(DefaultAttributes def)
        {
            Defaults = def;
        }

        public DefaultAttributes Defaults { get; set; }

        public IAttributeValue this[string name]
        {
            get => Get(name);
        }

        public ObjAttributeValue Insert(ObjAttributeValue attribute)
        {
            list.Add(attribute);

            return attribute;
        }

        private IAttributeValue Get(string name)
        {
            var attr = list.Find(a => a.AttributeName == name);

            return attr != null ? attr.Value : Defaults[name];
        }
    }

}
