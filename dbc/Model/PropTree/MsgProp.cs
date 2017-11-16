using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    public interface IMsgProp
    {
        long ID { get; }
        string Comment { get; }
        Comment CM { get; }

        AttributeValue Attribute(string name, AttributeValue def);
    }

    class MsgProp : IMsgProp
    {
        class EmptyMsgProp : IMsgProp
        {
            public long ID => 0;

            public string Comment => "";

            public Comment CM => null;

            public AttributeValue Attribute(string name, AttributeValue def)
            {
                return def;
            }
        }

        public MsgProp(long id)
        {
            ID = id;
        }

        public static IMsgProp EmptyProp { get; } = new EmptyMsgProp();

        public long ID { get; }

        public Attributes Attributes { get; } = new Attributes();

        public string Comment => CM?.Val ?? "";

        public Comment CM { get; set; }

        public AttributeValue Attribute(string name, AttributeValue def)
        {
            return Attributes.Get(name, def);
        }
    }
}
