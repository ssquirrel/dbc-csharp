using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    public class Tree
    {
        private IDictionary<long, MsgProp> byID =
            new Dictionary<long, MsgProp>();

        private SignalStore sigStore = new SignalStore();

        public Tree(DBC dbc)
        {
            DBC = dbc;

            Def = dbc.AttributeDefaults.ToDictionary(ad => ad.AttributeName);

            foreach (Comment cm in dbc.Comments)
            {
                switch (cm.Type)
                {
                    case Keyword.MESSAGES:
                        GetOrCreate(cm.MsgID).CM = cm;
                        break;

                    case Keyword.SIGNAL:
                        sigStore.GetOrCreate(cm.MsgID, cm.Name).CM = cm;
                        break;
                }
            }

            List<SignalProp> list = new List<PropTree.SignalProp>();

            foreach (ObjAttributeValue av in dbc.AttributeValues)
            {
                switch (av.ObjType)
                {
                    case Keyword.MESSAGES:
                        GetOrCreate(av.MsgID).Attributes.Add(av);
                        break;
                    case Keyword.SIGNAL:
                        sigStore.GetOrCreate(av.MsgID, av.Name)
                            .Attributes.Add(av);
                        break;
                }
            }

            foreach (SignalValueDescription vd in dbc.ValueDescriptions)
            {
                sigStore.GetOrCreate(vd.MsgID, vd.Name).VD = vd;
            }
        }

        public DBC DBC { get; }

        public IReadOnlyDictionary<string, AttributeDefault> Def { get; }

        public IMsgProp MsgProp(long id)
        {
            return Get(id) ?? PropTree.MsgProp.EmptyProp;
        }

        public ISignalProp SignalProp(long id, string name)
        {
            return sigStore.Get(id, name) ?? PropTree.SignalProp.EmptyProp;
        }

        private MsgProp Get(long id)
        {
            if (byID.TryGetValue(id, out var leaf))
            {
                return leaf;
            }

            return null;
        }

        private MsgProp GetOrCreate(long id)
        {
            if (!byID.TryGetValue(id, out var prop))
            {
                prop = byID[id] = new MsgProp(id);
            }

            return prop;
        }

    }
}
