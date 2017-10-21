using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;

using DbcLib.DBC.Parser;
using DbcLib.Excel.Parser;
using DbcLib.DBC.Writer;
using DbcLib.Model;


namespace dbc_test
{
    class Program
    {
        static void Main(string[] args)
        {

            DbcParserTests.Start(@"..\..\ParserTestFiles\");


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
            

            using (ExcelDBC d = ExcelParser.Parse("sample.xlsx", "Message_Detail"))
            using (DbcWriter writer = new DbcWriter(new StreamWriter("out.dbc", false, Encoding.Default)))
            {
                if (d.DBC != null)
                    writer.Write(d.DBC);
            }

            DBC dbc = DbcParser.Parse("M16_PHEV_HS-CAN3_Message_list_C-Sample_V1.4.dbc");

            using (DbcWriter writer = new DbcWriter(new StreamWriter("out.dbc", false, Encoding.Default)))
            {
                writer.Write(dbc);
            }
            */

            //Console.Write(double.Parse("-.1E+100"));
            Console.ReadKey();
        }



    }


}
