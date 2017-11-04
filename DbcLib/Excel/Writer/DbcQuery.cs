using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;

namespace DbcLib.Excel.Writer
{
    class MsgProp
    {
        public Comment CM { get; set; }

        public IList<ObjAttributeValue> AV { get; } =
            new List<ObjAttributeValue>();

        public ObjAttributeValue GetAttribute(string name)
        {
            return AV.FirstOrDefault(a => a.AttributeName == name);
        }
    }

    class SignalProp
    {
        public SignalProp(long msgID, string name)
        {
            MsgID = msgID;
            Name = name;
        }

        public long MsgID { get; }
        public string Name { get; }
        public Comment CM { get; set; }
        public SignalValueDescription VD { get; set; }
    }

    class MsgPropStore
    {
        private IDictionary<long, MsgProp> msgStore =
           new Dictionary<long, MsgProp>();

        public MsgProp GetOrCreate(long id)
        {
            if (!msgStore.TryGetValue(id, out MsgProp prop))
            {
                prop = new MsgProp();
                msgStore[id] = prop;
            }

            return prop;
        }

        public MsgProp Get(long id)
        {
            if (msgStore.TryGetValue(id, out MsgProp prop))
            {
                return prop;
            }

            return null;
        }
    }


    class SignalPropStore
    {
        private IDictionary<string, List<SignalProp>> primary =
            new Dictionary<string, List<SignalProp>>();

        public SignalProp GetOrCreate(long msgID, string name)
        {
            if (!primary.TryGetValue(name, out var conflicts))
            {
                conflicts = new List<SignalProp>();
                primary[name] = conflicts;
            }

            var prop = conflicts.Find(p => p.MsgID == msgID);
            if (prop == null)
            {
                prop = new SignalProp(msgID, name);
                conflicts.Add(prop);
            }

            return prop;
        }

        public SignalProp Get(long msgID, string name)
        {
            if (primary.TryGetValue(name, out var conflicts))
            {
                return conflicts.Find(p => p.MsgID == msgID);
            }

            return null;
        }


    }

    class DbcQuery
    {
        private MsgPropStore msgStore = new MsgPropStore();

        private SignalPropStore signalStore = new SignalPropStore();

        private IDictionary<string, AttributeDefault> defaultStore =
            new Dictionary<string, AttributeDefault>();

        public DbcQuery(Model.DBC dbc)
        {
            foreach (var cm in dbc.Comments)
            {
                if (cm.Type == Keyword.MESSAGES)
                {
                    msgStore.GetOrCreate(cm.MsgID).CM = cm;
                }
                else if (cm.Type == Keyword.SIGNAL)
                {
                    signalStore.GetOrCreate(cm.MsgID, cm.Name).CM = cm;
                }
            }


            foreach (ObjAttributeValue av in dbc.AttributeValues)
            {
                if (av.Type == Keyword.MESSAGES)
                {
                    msgStore.GetOrCreate(av.MsgID).AV.Add(av);
                }
            }

            foreach (SignalValueDescription vd in dbc.ValueDescriptions)
            {
                signalStore.GetOrCreate(vd.MsgID, vd.Name).VD = vd;
            }

            foreach (AttributeDefault def in dbc.AttributeDefaults)
            {
                defaultStore[def.AttributeName] = def;
            }
        }

        public MsgProp GetMsgProp(long msgID)
        {
            return msgStore.Get(msgID);
        }

        public SignalProp GetSignalProp(long msgID, string name)
        {
            return signalStore.Get(msgID, name);
        }

        public AttributeDefault GetAttributeDefault(string name)
        {
            if (defaultStore.TryGetValue(name, out AttributeDefault def))
            {
                return def;
            }

            return null;
        }
    }

}
