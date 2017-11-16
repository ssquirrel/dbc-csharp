using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    public interface ISignalProp
    {
        string Comment { get; }
        IList<ValueDesc> Descs { get; }

        Comment CM { get; }
        SignalValueDescription VD { get; }

        AttributeValue Attribute(string name, AttributeValue def);
    }

    class SignalProp : ISignalProp
    {
        class EmptySigProp : ISignalProp
        {
            public string Comment => "";

            public IList<ValueDesc> Descs => Array.Empty<ValueDesc>();

            public Comment CM => null;

            public SignalValueDescription VD => null;

            public AttributeValue Attribute(string name, AttributeValue def)
            {
                return def;
            }
        }

        public static ISignalProp EmptyProp { get; } = new EmptySigProp();

        public Attributes Attributes { get; } = new Attributes();

        public string Comment => CM?.Val ?? "";

        public IList<ValueDesc> Descs => VD?.Descs ?? Array.Empty<ValueDesc>();

        public Comment CM { get; set; }
        public SignalValueDescription VD { get; set; }

        public AttributeValue Attribute(string name, AttributeValue def)
        {
            return Attributes.Get(name, def);
        }

    }

}
