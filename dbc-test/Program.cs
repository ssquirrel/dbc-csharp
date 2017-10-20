using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;

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
            Console.ReadKey();

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
            }*/
        }



    }


}
