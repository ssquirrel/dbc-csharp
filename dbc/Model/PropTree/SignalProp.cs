using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    class SignalProp : ISignalProp
    {
        public SignalProp(long id, string name, DefaultAttributes def)
        {
            ID = id;
            Name = name;
            Attributes = new Attributes(def);
        }

        public long ID { get; }
        public string Name { get; }

        public string Comment
        {
            get
            {
                if (CM == null)
                    return "";

                return CM.Val;
            }
        }

        public IReadOnlyCollection<ValueDesc> Descs
        {
            get
            {
                if (VD == null)
                    return Array.Empty<ValueDesc>();

                return new ReadOnlyCollection<ValueDesc>(VD.Descs);
            }
        }

        public Comment CM { get; set; }
        public Attributes Attributes { get; }
        public SignalValueDescription VD { get; set; }

        IAttributes ISignalProp.Attributes => Attributes;
    }

}
