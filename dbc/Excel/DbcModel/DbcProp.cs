using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;

namespace DbcLib.Excel
{
    interface IDict<TKey, TValue>
    {
        TValue Get(TKey key);
        TValue GetOrCreate(TKey key);
    }

    class Attribute
    {
        public string AttributeName { get; }
    }

    class Attributes
    {
        private List<Attribute> attrs =
            new List<Attribute>();

        public Attribute this[string name]
        {
            get
            {
                return null;
            }

            set
            {

            }
        }

        private int At(string name)
        {
            return attrs.FindIndex(a => a.AttributeName == name);
        }
    }

    class MsgProp
    {
        public Comment CM { get; set; }

        public Attributes Attribute { get; } = new Attributes();
    }

}
