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
    public class DbcWorkbook : IDisposable, IEnumerable<DbcSheet>
    {
        private IWorkbook workbook;

        public DbcWorkbook(string filename)
        {
            if (!filename.EndsWith(".xlsx") &&
                !filename.EndsWith(".xls"))
                throw new ArgumentException("The file ext must be either .xlsx or .xls");

            FileStream stream = File.Open(filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            workbook = WorkbookFactory.Create(stream);
        }

        public void Dispose()
        {
            workbook.Close();
        }

        public IEnumerator<DbcSheet> GetEnumerator()
        {
            foreach (ISheet raw in workbook)
            {
                if (!raw.SheetName.StartsWith("Message_Detail"))
                    continue;

                yield return new DbcSheet(raw);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
