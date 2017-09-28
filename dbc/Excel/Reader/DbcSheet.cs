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
    public class DbcSheet : IEnumerable<DbcRow>
    {
        private ISheet sheet;

        public DbcSheet(ISheet s)
        {
            sheet = s;
        }

        public IEnumerator<DbcRow> GetEnumerator()
        {
            int i = 0;

            foreach (IRow raw in sheet)
            {
                if (i < 2)
                {
                    ++i;
                    continue;
                }

                DbcRow row = new DbcRow(raw);

                if (row.RowType == RowType.Unknown)
                    continue;

                yield return row;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


}
