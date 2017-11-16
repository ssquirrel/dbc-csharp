using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model.PropTree;
using System.Diagnostics;
using DbcLib.Model;
using System.Collections;

namespace dbc_test
{
    static class CompositeKeyTest
    {
        class SignalProp
        {
            public string Name;
            public int Num;

            public SignalProp()
            {
            }

            public SignalProp(string name)
            {
                Name = name;
            }
        }

        class SigPropStore
        {
            interface IStore : IEnumerable<SignalProp>
            {
                int Count { get; }

                SignalProp Get(string name);
                SignalProp Add(string name);
            }

            class ListBased : IStore
            {
                private List<SignalProp> list = new List<SignalProp>();

                public int Count => list.Count;

                public SignalProp Get(string name)
                {
                    for (int i = 0; i < list.Count; ++i)
                    {
                        var prop = list[i];

                        if (prop.Name == name)
                            return prop;
                    }

                    return null;
                }

                public SignalProp Add(string name)
                {
                    var prop = new SignalProp(name);

                    list.Add(prop);

                    return prop;
                }

                public IEnumerator<SignalProp> GetEnumerator()
                {
                    return list.GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            class DictBased : IStore
            {
                private Dictionary<string, SignalProp> dict;

                public DictBased(IEnumerable<SignalProp> props)
                {
                    dict = props.ToDictionary(prop => prop.Name);
                }

                public int Count => dict.Count;

                public SignalProp Get(string name)
                {
                    dict.TryGetValue(name, out var prop);

                    return prop;
                }

                public SignalProp Add(string name)
                {
                    return dict[name] = new SignalProp();
                }

                public IEnumerator<SignalProp> GetEnumerator()
                {
                    return dict.Values.GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            private bool linear = true;
            private IStore store = new ListBased();

            public SignalProp Get(string name)
            {
                return store.Get(name);
            }

            public SignalProp GetOrCreate(string name)
            {
                var prop = store.Get(name);

                if (prop != null)
                    return prop;

                if (linear && store.Count >= 10)
                {
                    linear = false;
                    store = new DictBased(store);
                }

                return store.Add(name);
            }

        }

        class Key : IEquatable<Key>
        {
            public int id;
            public string name;

            public Key(int id, string name)
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

        public static void Start(int N)
        {
            var src = Sample(N);

            var mixed = Mixed(src);
            var composite = CompositeKey(src);

            var access = NLookup.RandomRepeat(src, N * 10);

            var sw1 = new Stopwatch();
            sw1.Start();
            foreach (var key in access)
            {
                var store = mixed[key.id];
                store.GetOrCreate(key.name);//.CM.MsgID++;
            }
            sw1.Stop();

            var sw2 = new Stopwatch();
            sw2.Start();
            foreach (var key in access)
            {
                var a = composite[key];//.CM.MsgID++;
            }
            sw2.Stop();

            Console.WriteLine("N: " + N);
            Console.WriteLine(sw1.ElapsedTicks);
            Console.WriteLine(sw2.ElapsedTicks);
            Console.WriteLine("------------------------------");
        }

        private static List<Key> Sample(int N)
        {
            List<Key> sample = new List<Key>();

            var rand = new Random();

            for (int i = 0; i < N; ++i)
            {
                var id = rand.Next(0, int.MaxValue);

                int large = rand.Next(100);

                if (large >= 75)
                {
                    int count = rand.Next(11, 60);
                    for (int k = 11; k < count; ++k)
                        sample.Add(new Key(id, NLookup.RandomString()));
                }
                else
                {
                    int count = rand.Next(10);
                    for (int k = 0; k < count; ++k)
                        sample.Add(new Key(id, NLookup.RandomString()));
                }
            }

            return sample;
        }

        private static Dictionary<int, SigPropStore> Mixed(List<Key> list)
        {
            Dictionary<int, SigPropStore> dict = new Dictionary<int, SigPropStore>();

            foreach (var sample in list)
            {
                if (!dict.TryGetValue(sample.id, out var store))
                {
                    store = dict[sample.id] = new SigPropStore();
                }
            }

            return dict;
        }

        private static Dictionary<Key, SignalProp> CompositeKey(List<Key> list)
        {
            var dict = new Dictionary<Key, SignalProp>();

            foreach (var sample in list)
            {
                var prop = new SignalProp();
                dict[sample] = prop;
            }

            return dict;
        }
    }
}
