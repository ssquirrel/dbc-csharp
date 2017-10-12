using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel
{
    enum CellType
    {
        Empty,
        Number,
        String
    }

    class DbcCell
    {
        public DbcCell(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public DbcCell(ICell cell)
        {
            Row = cell.RowIndex;
            Col = cell.ColumnIndex;

            if (cell.CellType == NPOI.SS.UserModel.CellType.String)
            {
                string trimed = cell.StringCellValue.Trim();

                if (trimed.Length != 0)
                {
                    Set(cell.StringCellValue.Trim());
                }
            }
            else if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
            {
                try
                {
                    Set(cell.NumericCellValue);
                }
                catch (FormatException) { }
            }

        }

        public string Val { get; private set; } = "";
        public double Num { get; private set; }

        public int Row { get; }
        public int Col { get; }

        public CellType Type { get; private set; }

        public void Set(string val)
        {
            Val = val;
            Type = CellType.String;
        }

        public void Set(double num)
        {
            Num = num;
            Type = CellType.Number;
        }

        public bool IsEmpty()
        {
            return Type == CellType.Empty;
        }
    }
}
