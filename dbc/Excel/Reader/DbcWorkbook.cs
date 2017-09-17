using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DbcLib.Excel.Reader
{
    class DbcWorkbook : IDisposable
    {
        private IWorkbook workbook;
        private IEnumerator sheetIter;

        public DbcWorkbook(string filename)
        {
            if (!filename.EndsWith(".xlsx") &&
                !filename.EndsWith(".xls"))
                throw new ArgumentException("The file ext must be either .xlsx or xls");

            FileStream stream = File.Open(filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            if (filename.EndsWith(".xlsx"))
                workbook = new XSSFWorkbook(stream);
            else
                workbook = new HSSFWorkbook(stream);

            sheetIter = workbook.GetEnumerator();

            Curr = FindNext();

            EndOfStream = Curr == null;
        }

        public void Dispose()
        {
            workbook.Close();
        }


        public bool EndOfStream { get; private set; }
        public DbcSheet Curr { get; private set; }

        public DbcSheet Consume()
        {
            DbcSheet old = Curr;
            Curr = FindNext();

            EndOfStream = Curr == null;

            return old;
        }

        private DbcSheet FindNext()
        {
            while (sheetIter.MoveNext())
            {
                ISheet sheet = (ISheet)sheetIter.Current;

                if (!sheet.SheetName.StartsWith("Message_Detail"))
                    continue;

                DbcSheet dbcSheet = new DbcSheet(sheet);

                DbcExcelRow first = dbcSheet.Consume();
                DbcExcelRow sec = dbcSheet.Consume();

                if (first != null && sec != null)
                    return dbcSheet;
            }

            return null;
        }

    }
}
