using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;

using DbcLib.DBC.Parser;
using DbcLib.Excel.Writer;
using DbcLib.Excel.Parser;
using DbcLib.DBC.Writer;
using DbcLib.Model;

namespace dbc_test
{
    class Program
    {
        static void Main(string[] args)
        {

            //DbcParserTests.Start(@"..\..\ParserTestFiles\");


            /*
            DbcWorkbook workbook = new DbcWorkbook("sample.xlsx");
            var sheet = workbook.FirstOrDefault();

            ExcelParser parser = new ExcelParser();

            DBC dbc = parser.Parse(sheet);

            if (parser.Errors.Any())
                throw new Exception();
   
            using (DbcWriter writer = new DbcWriter(new StreamWriter("out.dbc", false, Encoding.Default)))
            {
                writer.Write(dbc);
            }

            using (DbcWriter writer = new DbcWriter(new StreamWriter("out.dbc", false, Encoding.Default)))
            {
                writer.Write(dbc);
            }

              using (ExcelDBC d = ExcelParser.Parse("out.xlsx", "Message_Detail"))
            using (DbcWriter writer = new DbcWriter(new StreamWriter("out.dbc", false, Encoding.Default)))
            {
                if (d.DBC != null)
                    writer.Write(d.DBC);

            

           

            }
           


            */



            using (ExcelWriter writer = new ExcelWriter(@"out1.xlsx"))
            {
                DBC dbc = DbcParser.Parse("sample.dbc");
                writer.Add("Message_Detail", dbc);
                writer.Write();
            }

            using (ExcelDBC d = ExcelParser.Parse("out1.xlsx", "Message_Detail"))
            using (DbcWriter writer = new DbcWriter(new StreamWriter("out1.dbc", false, Encoding.Default)))
            {
                if (d.DBC != null)
                    writer.Write(d.DBC);
            }

            using (ExcelWriter writer = new ExcelWriter(@"out2.xlsx"))
            {
                DBC dbc = DbcParser.Parse("out1.dbc");
                writer.Add("Message_Detail", dbc);
                writer.Write();
            }

            using (ExcelDBC d = ExcelParser.Parse("out2.xlsx", "Message_Detail"))
            using (DbcWriter writer = new DbcWriter(new StreamWriter("out2.dbc", false, Encoding.Default)))
            {
                if (d.DBC != null)
                    writer.Write(d.DBC);
            }


            Console.ReadKey();
        }



    }


}
