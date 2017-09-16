using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using DbcLib.DBC.Parser;
using DbcLib.DBC.Model;
using DbcLib.DBC.Lex;
using DbcLib.DBC.Writer;
using DbcLib.Excel.Reader;

namespace dbc_test
{
    class Program
    {

        static void Main(string[] args)
        {
            /*
            Console.WriteLine(Assert(TokenType.UNSIGNED, TokenType.DOUBLE));
            Console.WriteLine(Assert(TokenType.SIGNED, TokenType.DOUBLE));
            Console.WriteLine(Assert(TokenType.UNSIGNED, TokenType.SIGNED));
            Console.WriteLine(Assert(TokenType.UNSIGNED, TokenType.UNSIGNED));
            Console.WriteLine(Assert(TokenType.DOUBLE, TokenType.DOUBLE));

            Console.WriteLine(Assert(TokenType.DOUBLE, TokenType.SIGNED));
            Console.WriteLine(Assert(TokenType.DOUBLE, TokenType.UNSIGNED));
            Console.WriteLine(Assert(TokenType.DOUBLE, TokenType.DOUBLE | TokenType.STRING));
            */

            DbcWorkbook book = new DbcWorkbook("sample.xlsx");
        }

        static bool Assert(TokenType t, TokenType e)
        {
            return (t & e) == t;
        }

        static void ExcelReader()
        {

        }

        static void DbcParser()
        {
            try
            {

                /*
                          
                */

                DbcParser parser = new DbcParser("sample.dbc");
                DBC dbc = parser.Parse();

                using (DbcWriter writer =
                    new DbcWriter(new StreamWriter(File.Open("out.dbc", FileMode.Create), Encoding.Default)))
                {

                    writer.Write(dbc);
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
