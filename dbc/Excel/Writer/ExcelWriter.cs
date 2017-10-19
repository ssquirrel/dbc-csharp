using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;
using DbcLib.Model;

namespace DbcLib.Excel.Writer
{
    class ExcelTemplate
    {
        public static IWorkbook LoadTemplate(string fn)
        {
            return WorkbookFactory.Create(fn);
        }
    }

    class ExcelWriter : IDisposable
    {
        private IWorkbook workbook;

        public ExcelWriter()
        {
            workbook = ExcelTemplate.LoadTemplate("template.xlsx");
        }

        public void Add(string sn, Model.DBC dbc)
        {
            ISheet sheet = AllocateSheet(sn);

            IRow row = sheet.CreateRow(2);

            Message msg = new Message();

            DbcRow r = new DbcRow(row);
            r.MsgID.SetHex(msg.MsgID);
            r.Transmitter.Set(msg.Transmitter);
        }

        public void Dispose()
        {
            workbook.Close();
        }

        private ISheet AllocateSheet(string name)
        {
            workbook.SetSheetName(0, name);
            return workbook.GetSheetAt(0);
        }
    }
}
