using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbc_test
{
    class Pair
    {
        public string key;
        public int count;

        public Pair(string key, int count)
        {
            this.key = key;
            this.count = count;
        }
    }

    class NLookup
    {
        public static void Start(int N)
        {
            var sample = Sample(N);
            var access = RandomRepeat(sample, 100000);

            var dict = DictionaryBased(sample);
            var list = ListBased(sample);

            var sw1 = new Stopwatch();
            sw1.Start();
            foreach (var key in access)
            {
                var pair = dict[key];
                pair.count += 1;
            }
            sw1.Stop();

            var sw2 = new Stopwatch();
            sw2.Start();
            foreach (var key in access)
            {
                foreach (var pair in list)
                {
                    if (pair.key == key)
                    {
                        pair.count += 1;
                        break;
                    }
                }

            }
            sw2.Stop();

            Console.WriteLine("N: " + N);
            Console.WriteLine(sw1.ElapsedTicks);
            Console.WriteLine(sw2.ElapsedTicks);
            Console.WriteLine("------------------------------");
        }

        private static string[] RandomRepeat(string[] arr, int N)
        {
            var rr = new string[N];
            var random = new Random();

            for (int i = 0; i < N; ++i)
            {
                rr[i] = arr[random.Next(arr.Length)];
            }

            return rr;
        }

        private static List<Pair> ListBased(string[] sample)
        {
            var list = new List<Pair>();
            foreach (var str in sample)
            {
                list.Add(new Pair(str, 0));
            }

            return list;
        }

        private static Dictionary<string, Pair> DictionaryBased(string[] sample)
        {
            var dict = new Dictionary<string, Pair>();
            foreach (var str in sample)
            {
                dict[str] = new Pair(str, 0);
            }

            return dict;
        }

        private static string[] Sample(int N)
        {
            string[] sample = new string[N];

            for (int i = 0; i < N; ++i)
            {
                sample[i] = RandomString();
            }

            return sample;
        }

        private static string RandomString()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random(Guid.NewGuid().GetHashCode());
            var stringChars = new char[random.Next(5, 20)];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
    }
}
