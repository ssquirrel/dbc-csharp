using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    abstract class Attributes : IAttributes, IEnumerable<ObjAttributeValue>
    {
        private DefaultAttributes def;
        private List<ObjAttributeValue> list = new List<ObjAttributeValue>();

        public Attributes(DefaultAttributes def)
        {
            this.def = def;
        }

        public IAttributeValue this[string name]
        {
            get => Get(name);
        }

        public IEnumerator<ObjAttributeValue> GetEnumerator()
        {
            foreach (var val in def)
            {
                var av = Construct();
                av.AttributeName = val.AttributeName;
                av.Value = av.Value;

                yield return av;
            }

            foreach (var av in list)
                yield return av;
        }

        public ObjAttributeValue Insert(ObjAttributeValue attribute)
        {
            list.Add(attribute);

            return attribute;
        }

        protected abstract ObjAttributeValue Construct();

        private IAttributeValue Get(string name)
        {
            var attr = list.Find(a => a.AttributeName == name);

            return attr != null ? attr.Value : def[name];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class MsgAttributes : Attributes
    {
        private long id;

        public MsgAttributes(long id, DefaultAttributes defs) : base(defs)
        {
            this.id = id;
        }

        protected override ObjAttributeValue Construct()
        {
            return new ObjAttributeValue
            {
                ObjType = Keyword.MESSAGES,
                MsgID = id
            };
        }
    }

}
