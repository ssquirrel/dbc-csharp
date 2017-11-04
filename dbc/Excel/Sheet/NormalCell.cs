using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel
{

    class NormalCell : DbcCell
    {
        public NormalCell(ICell cell)
        {
            NCell = cell;

            if (cell.CellType == NPOI.SS.UserModel.CellType.String)
            {
                string trimed = cell.StringCellValue.Trim();

                if (trimed.Length != 0)
                {
                    Val = trimed;
                    Type = CellType.String;
                }
            }
            else if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
            {
                try
                {
                    Num = cell.NumericCellValue;
                    Type = CellType.Number;
                }
                catch (FormatException) { }
            }
        }

        public override ICell NCell { get; }

        public override int Row => NCell.RowIndex;

        public override int Col => NCell.ColumnIndex;
    }
}
