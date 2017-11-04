using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    class Lazy<T>
    {
        private Func<T> ctor;
        private T val;

        public Lazy(Func<T> ctor)
        {
            this.ctor = ctor;
        }

        public T Value
        {
            get
            {
                if (val == null)
                    val = ctor();

                return val;
            }

            set => val = value;
        }
    }

    public class SignalProp
    {
        private Lazy<Comment> cm;
        private Lazy<SignalValueDescription> vd;

        public SignalProp(long id, string name)
        {
            ID = id;
            Name = name;

            cm = new Lazy<Comment>(() =>
            {
                return new Comment
                {
                    MsgID = id,
                    Name = name
                };
            });

            vd = new Lazy<SignalValueDescription>(() =>
            {
                return new SignalValueDescription
                {
                    MsgID = id,
                    Name = name
                };
            });
        }

        public long ID { get; }
        public string Name { get; }

        public Comment CM
        {
            get => cm.Value;
            set => cm.Value = value;
        }

        public SignalValueDescription VD
        {
            get => vd.Value;
            set => vd.Value = value;
        }
    }

    public class MsgProp : IByIdLeaf
    {
        private Lazy<Comment> cm;

        public MsgProp(long id, IAttributes def)
        {
            ID = id;
            Attribute = new MsgAttributes(id, def);

            cm = new Lazy<Comment>(() =>
            {
                return new Comment { MsgID = ID };
            });
        }

        public long ID { get; }
        public Comment CM { get => cm.Value; set => cm.Value = value; }
        public MsgAttributes Attribute { get; }

        MsgProp IByIdLeaf.MsgProp => this;

        public SignalProp Insert(string name)
        {
            throw new NotImplementedException();
        }

        public SignalProp Name(string name)
        {
            throw new NotImplementedException();
        }
    }

}
