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

        public Cell[] Curr { get; private set; }
        public bool EndOfStream { get; private set; }

        public Cell[] Consume()
        {
            if (EndOfStream)
                return null;

            IRow row = (IRow)rowIter.Current;

            Cell[] raw = DbcExcelRow.CreateRow();

            for (int i = 0; i < raw.Length; ++i)
            {
                ICell cell = row.GetCell(i);

                if (cell == null)
                    continue;

                switch (cell.CellType)
                {
                    case NPOI.SS.UserModel.CellType.Numeric:
                        double num = cell.NumericCellValue;

                        if ((int)num != num)
                        {
                            raw[i] = new Cell(cell.RowIndex,
                                cell.ColumnIndex,
                                num,
                                CellType.DOUBLE);
                        }
                        else if ((int)num < 0)
                        {
                            raw[i] = new Cell(cell.RowIndex,
                                cell.ColumnIndex,
                                num,
                                CellType.SIGNED);
                        }
                        else
                        {
                            raw[i] = new Cell(cell.RowIndex,
                                cell.ColumnIndex,
                                num,
                                CellType.UNSIGNED);
                        }

                        break;
                    case NPOI.SS.UserModel.CellType.String:

                        raw[i] = new Cell(cell.RowIndex,
                                cell.ColumnIndex, 
                                cell.StringCellValue,
                                CellType.STRING);

                        break;
                }
            }

            EndOfStream = !rowIter.MoveNext();

            Cell[] old = Curr;

            Curr = raw;

            return old;
        }

    }


}
