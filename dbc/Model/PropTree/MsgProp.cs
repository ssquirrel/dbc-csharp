using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    class MsgProp : IMsgProp, IQueryById
    {
        private List<SignalProp> list = new List<SignalProp>();

        public MsgProp(long id, DefaultAttributes def)
        {
            ID = id;
            Attributes = new Attributes(def);
        }

        public long ID { get; }

        public string Comment
        {
            get
            {
                if (CM == null)
                    return "";

                return CM.Val;
            }
        }

        public Comment CM { get; set; }

        public Attributes Attributes { get; }

        public IEnumerable<SignalProp> SignalProps => list;

        IMsgProp IQueryById.MsgProp => this;

        IAttributes IMsgProp.Attributes => Attributes;

        public ISignalProp Name(string name)
        {
            return Internal_Name(name) ?? PropTree.EmptySignalProp;
        }

        public ISignalProp Insert(string name)
        {
            return Internal_Insert(name);
        }

        public SignalProp Internal_Name(string name)
        {
            return list.Find(p => p.Name == name);
        }

        public SignalProp Internal_Insert(string name)
        {
            var prop = Internal_Name(name);

            if (prop != null)
                return prop;

            prop = new SignalProp(ID, name, Attributes.Defaults);
            list.Add(prop);

            return prop;
        }

    }
}
