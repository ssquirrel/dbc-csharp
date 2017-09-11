using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using DbcLib.DBC.Parser;
using DbcLib.DBC.Model;

namespace dbc_test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (StreamReader reader = new StreamReader("sample.dbc"))
                {
                    /*
                    Lexer lexer = new Lexer(reader);

                    foreach (Token t in lexer.Lex())
                    {
                        System.Console.WriteLine(t.Val + " " + t.Type.ToString());
                    }
                    */

                    DbcParser parser = new DbcParser(reader);
                    DBC dbc = parser.Parse();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }


            System.Console.ReadKey();
        }
    }
}
