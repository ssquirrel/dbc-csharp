using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    public class PropTree
    {
        private static readonly DefaultAttributes Empty_Def =
            new DefaultAttributes(Enumerable.Empty<AttributeDefault>());

        private static readonly MsgProp SENTINEL_MSG = new MsgProp(0, Empty_Def);
        private static readonly SignalProp SENTINEL_SIG = new SignalProp(0, "");

        private IDictionary<long, MsgProp> byID =
            new Dictionary<long, MsgProp>();

        public PropTree(DBC dbc)
        {
            Def = new DefaultAttributes(dbc.AttributeDefaults);

            foreach (Comment cm in dbc.Comments)
            {
                switch (cm.Type)
                {
                    case Keyword.MESSAGES:
                        Internal_Insert(cm.MsgID).CM = cm;
                        break;

                    case Keyword.SIGNAL:
                        Internal_Insert(cm.MsgID).Internal_Insert(cm.Name).CM = cm;
                        break;
                }
            }

            foreach (ObjAttributeValue av in dbc.AttributeValues)
            {
                switch (av.ObjType)
                {
                    case Keyword.MESSAGES:
                        Internal_Insert(av.MsgID).Attributes.Insert(av);
                        break;
                }
            }

            foreach (SignalValueDescription vd in dbc.ValueDescriptions)
            {
                Internal_Insert(vd.MsgID).Internal_Insert(vd.Name).VD = vd;
            }
        }

        public static IAttributeValue EmptyAttributeValue { get; } = new AttributeValue();
        public static IQueryById EmptyQuery { get; } = SENTINEL_MSG;
        public static IMsgProp EmptyMsgProp { get; } = SENTINEL_MSG;
        public static ISignalProp EmptySignalProp { get; } = SENTINEL_SIG;

        public DefaultAttributes Def { get; set; }

        public IQueryById ID(long id)
        {
            if (byID.TryGetValue(id, out var leaf))
            {
                return leaf;
            }

            return leaf;
        }

        public IQueryById Insert(long id)
        {
            return Internal_Insert(id);
        }

        private MsgProp Internal_Insert(long id)
        {
            if (!byID.TryGetValue(id, out var prop))
            {
                prop = byID[id] = new MsgProp(id, Def);
            }

            return prop;
        }

    }
}
