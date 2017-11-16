using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    class SignalStore
    {
        private class Key : IEquatable<Key>
        {
            public long id;
            public string name;

            public Key(long id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public bool Equals(Key other)
            {
                return id == other.id && name == other.name;
            }

            public override bool Equals(object obj)
            {
                return obj is Key && Equals(obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = (int)2166136261;
                    hash = hash * 16777619 + id.GetHashCode();
                    hash = hash * 16777619 + name.GetHashCode();

                    return hash;
                }
            }
        }

        private Dictionary<Key, SignalProp> dict =
            new Dictionary<Key, SignalProp>();

        public SignalProp Get(long id, string name)
        {
            dict.TryGetValue(new Key(id, name), out var prop);

            return prop;
        }

        public SignalProp GetOrCreate(long id, string name)
        {
            var key = new Key(id, name);

            if (!dict.TryGetValue(key, out var prop))
            {
                prop = dict[key] = new SignalProp();
            }

            return prop;
        }
    }
}
