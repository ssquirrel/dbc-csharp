using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;

using DbcLib.DBC.Parser;
using DbcLib.Model;
using DbcLib.DBC.Lex;
using DbcLib.DBC.Writer;
using DbcLib.Excel.Reader;
using DbcLib.Excel.Parser;



namespace dbc_test
{
    class Program
    {



        static void Main(string[] args)
        {

            DbcWorkbook workbook = new DbcWorkbook("sample.xlsx");

            DbcSheet sheet = workbook.Take(1).Single();

            ExcelParser parser = new ExcelParser();
            DBC dbc = parser.Parse(sheet);

            
            using (DbcWriter writer = new DbcWriter(new StreamWriter("out.dbc", false, Encoding.Default)))
            {
                //DBC dbc = DbcParser.Parse("sample.dbc");

                writer.Write(dbc);

            }


            dbc = DbcParser.Parse("out.dbc");

            using (DbcWriter writer = new DbcWriter(new StreamWriter("out1.dbc", false, Encoding.Default)))
            {
                writer.Write(dbc);
            }
        }




        static void ExcelReader()
        {
            DBC dbc = DbcParser.Parse("sample.dbc");

            using (DbcWriter writer = new DbcWriter(new StreamWriter("out.dbc", false, Encoding.Default)))
            {
                writer.Write(dbc);
            }
        }

        static void DbcParser1()
        {
            try
            {









            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }


            Console.ReadKey();
        }


    }
}
