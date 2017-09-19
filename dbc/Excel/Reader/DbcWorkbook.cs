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
    class DbcWorkbook : IDisposable, IEnumerable<DbcSheet>
    {
        private IWorkbook workbook;

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
        }

        public void Dispose()
        {
            workbook.Close();
        }

        public IEnumerator<DbcSheet> GetEnumerator()
        {
            foreach (ISheet sheet in workbook)
            {
                if (!sheet.SheetName.StartsWith("Message_Detail"))
                    continue;

                DbcSheet dbcSheet = new DbcSheet(sheet);


                //if (first != null && sec != null)
                yield return dbcSheet;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
