using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using DbcLib.DBC.Parser;
using DbcLib.DBC.Model;
using DbcLib.DBC.Lex;
using DbcLib.Excel;

namespace dbc_test
{
    class Program
    {
        [Flags]
        public enum TokenType
        {
            None = 0x0,
            TOKEN = 0x1,
            UNSIGNED = 0x2,
            SIGNED = 0x4 | UNSIGNED,
            DOUBLE = 0X8 | SIGNED,
            STRING = 0x10,
            IDENTIFIER = 0x20,
            KEYWORD = 0x40,
        }

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






            DbcParser parser = new DbcParser("sample.dbc");
            DBC dbc = parser.Parse();

        }

        static bool Assert(TokenType t, TokenType e)
        {
            return (t & e) == t;
        }

        static void ExcelReader()
        {
            using (ExcelReader reader = new ExcelReader("sample.xls"))
            {
                List<DbcExcelRow> rows = reader.Read();
            }
        }

        static void DbcParser()
        {
            try
            {

                /*
                           Lexer lexer = new Lexer("sample.dbc");

            foreach (Token t in lexer.Lex())
            {
                Console.WriteLine(t.Val + " " + t.Type.ToString());
            }
                */

                DbcParser parser = new DbcParser("sample.dbc");
                DBC dbc = parser.Parse();

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
