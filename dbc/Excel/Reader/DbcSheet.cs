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
    class DbcSheet
    {
        private IEnumerator rowIter;

        public DbcSheet(ISheet sheet)
        {
            rowIter = sheet.GetEnumerator();

            EndOfStream = !rowIter.MoveNext();

            Consume();
        }

        public DbcExcelRow Curr { get; private set; }
        public bool EndOfStream { get; private set; }

        public DbcExcelRow Consume()
        {
            if (EndOfStream)
                return null;

            DbcExcelRow row = new DbcExcelRow((IRow)rowIter.Current);

            DbcExcelRow old = Curr;

            Curr = row;

            EndOfStream = !rowIter.MoveNext();

            return old;
        }

    }


}
