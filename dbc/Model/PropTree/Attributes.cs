using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    class Attributes
    {
        private List<ObjAttributeValue> list = new List<ObjAttributeValue>();

        public Attributes()
        {
        }

        public void Add(ObjAttributeValue attribute)
        {
            list.Add(attribute);
        }

        public AttributeValue Get(string name)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                var av = list[i];

                if (av.AttributeName == name)
                    return av.Value;
            }

            return null;
        }

        public AttributeValue Get(string name, AttributeValue def)
        {
            var value = Get(name);

            return value ?? def;
        }

    }

}
