using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Collections;

using DbcLib.Model;
using DbcLib.DBC.Parser;
using DbcLib.DBC.Writer;

namespace dbc_test
{
    class DbcParserTests
    {
        public static void Start(string dir)
        {
            DbcParserTests test = new DbcParserTests(dir);

            foreach (var fn in test.files)
            {
                if (test.Eval(fn.FullName))
                    Console.WriteLine(fn.Name + " Pass");
                else
                    Console.WriteLine(fn.Name + " FaiLLLLLLLLLLLLLLLLLLL");
            }
        }

        private FileInfo[] files;

        public DbcParserTests(string dir)
        {
            var info = new DirectoryInfo(dir);
            var files = info.GetFiles("*.dbc");

            if (!files.Any())
                throw new ArgumentException();

            this.files = files;
        }

        private bool Eval(string fn)
        {
            DBC dbc = DbcParser.Parse(fn);

            using (FileStream fs = new FileStream(Path.GetRandomFileName(),
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.Read | FileShare.ReadWrite,
                    4096,
                    FileOptions.DeleteOnClose))
            using (StreamReader src = new StreamReader(fn, Encoding.Default))
            {
                DbcWriter writer = new DbcWriter(new StreamWriter(fs, Encoding.Default));
                writer.Write(dbc);

                fs.Position = 0;

                var d1 = ReadNoneEmptyLines(src);
                var d2 = ReadNoneEmptyLines(new StreamReader(fs, Encoding.Default));

                if (d1.Count == 0)
                    return false;

                return Enumerable.SequenceEqual(d1, d2);
            }
        }

        private static List<string> ReadNoneEmptyLines(StreamReader reader)
        {
            List<string> list = new List<string>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line != "")
                    list.Add(line);
            }

            return list;
        }
    }
}
