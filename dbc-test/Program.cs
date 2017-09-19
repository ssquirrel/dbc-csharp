using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;

using DbcLib.DBC.Parser;
using DbcLib.DBC.Model;
using DbcLib.DBC.Lex;
using DbcLib.DBC.Writer;
using DbcLib.Excel.Reader;
using DbcLib.Excel;



namespace dbc_test
{

    class Program
    {

        static void Main(string[] args)
        {
            //ExcelParser book = new ExcelParser("sample.xlsx");


            DbcParser parser = new DbcParser("sample.dbc");
            DBC dbc = parser.Parse();

            using (DbcWriter writer = new DbcWriter(new StreamWriter("out.dbc", false, Encoding.Default)))
            {
                writer.Write(dbc);
            }
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

                Lexer lex = new Lexer("");

                foreach (Token t in lex.Lex())
                {
                    Console.WriteLine(t.Val + " " + t.Type.ToString());
                }



            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }


            System.Console.ReadKey();
        }

    }
}
