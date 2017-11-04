using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    public interface IByIdLeaf
    {
        SignalProp Name(string name);
        MsgProp MsgProp { get; }

        SignalProp Insert(string name);
    }

    class EmptyLeaf : IByIdLeaf
    {
        public MsgProp MsgProp => null;

        public SignalProp Insert(string name)
        {
            return null;
        }

        public SignalProp Name(string name)
        {
            return null;
        }
    }

    public class PropTree
    {
        private IDictionary<long, IByIdLeaf> byID =
            new Dictionary<long, IByIdLeaf>();

        public PropTree()
        {

        }

        public PropTree(DBC dbc)
        {
            Def = new DefaultAttributes(dbc.AttributeDefaults);
        }

        public DefaultAttributes Def { get; set; }

        public DBC ToDBC()
        {
            return null;
        }

        public IByIdLeaf ID(long id)
        {
            return null;
        }

        public IByIdLeaf Insert(long id)
        {
            if (!byID.TryGetValue(id, out var leaf))
            {
                leaf = new MsgProp(id, Def);
                byID.Add(id, leaf);
            }

            return leaf;
        }
    }
}
