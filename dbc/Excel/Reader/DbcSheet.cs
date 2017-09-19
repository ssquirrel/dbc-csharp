using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DbcLib.Excel.Reader
{
    class DbcSheet : IEnumerable<DbcExcelRow>
    {
        private ISheet sheet;

        public DbcSheet(ISheet sheet)
        {
            this.sheet = sheet;
        }

        public IEnumerator<DbcExcelRow> GetEnumerator()
        {
            foreach (IRow row in sheet)
                yield return new DbcExcelRow(row);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


}
