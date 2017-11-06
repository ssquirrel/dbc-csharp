using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    public interface IAttributeValue
    {
        AttrValType Type { get; }

        double Num { get; }

        string Val { get; }
    }

    public interface IAttributes
    {
        IAttributeValue this[string name] { get; }
    }

    public interface IQueryById
    {
        IMsgProp MsgProp { get; }
        ISignalProp Name(string name);
    }

    public interface IMsgProp
    {
        long ID { get; }
        string Comment { get; }
        Comment CM { get; }
        IAttributes Attributes { get; }
    }

    public interface ISignalProp
    {
        long ID { get; }
        string Name { get; }

        string Comment { get; }
        IReadOnlyCollection<ValueDesc> Descs { get; }

        Comment CM { get; }
        SignalValueDescription VD { get; }
    }
}
