using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    public interface IAttribute
    {
        string AttributeName { get; set; }

        AttrValType Type { get; }

        double Num { get; }
        string Val { get; }
    }

    public interface IAttributes
    {
        IAttribute this[string name] { get; }
    }

    public class DefaultAttributes : IAttributes
    {
        private IDictionary<string, IAttribute> dict =
            new Dictionary<string, IAttribute>();

        public DefaultAttributes(IEnumerable<IAttribute> defaults)
        {
            dict = defaults.ToDictionary(def => def.AttributeName);
        }

        public IAttribute this[string name]
        {
            get
            {
                dict.TryGetValue(name, out var def);

                return def;
            }
        }

        public bool TryInsert(IAttribute ia)
        {
            bool pred = dict.ContainsKey(ia.AttributeName);

            if (!pred)
                dict.Add(ia.AttributeName, ia);

            return pred;
        }
    }

    public class Attributes : IAttributes
    {
        private IAttributes defs;
        private List<IAttribute> list = new List<IAttribute>();

        public Attributes(IAttributes defs)
        {
            this.defs = defs;
        }

        public IAttribute this[string name]
        {
            get => Get(name);
        }

        public void Insert(IAttribute attribute)
        {
            list.Add(attribute);
        }

        private IAttribute Get(string name)
        {
            var attr = list.Find(a => a.AttributeName == name);

            return attr ?? defs[name];
        }
    }

    public class MsgAttributes : Attributes
    {
        private long id;

        public MsgAttributes(long id, IAttributes defs) : base(defs)
        {
            this.id = id;
        }

        public void Insert(string name, double num)
        {
            Insert(new ObjAttributeValue
            {
                AttributeName = name,
                ObjType = Keyword.MESSAGES,
                MsgID = id,
                Num = num
            });
        }

        public void Insert(string name, string val)
        {
            Insert(new ObjAttributeValue
            {
                AttributeName = name,
                ObjType = Keyword.MESSAGES,
                MsgID = id,
                Val = val
            });
        }
    }

}
