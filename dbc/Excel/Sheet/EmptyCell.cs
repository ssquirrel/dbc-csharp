using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel
{
    class EmptyCell : DbcCell
    {
        private Lazy<ICell> lazy;

        public EmptyCell(IRow row, int col)
        {
            lazy = new Lazy<ICell>(() =>
            {
                return row.GetCell(col, MissingCellPolicy.CREATE_NULL_AS_BLANK);
            }, false);

            Row = row.RowNum;
            Col = col;
        }

        public override ICell NCell => lazy.Value;

        public override int Row { get; }

        public override int Col { get; }
    }
}
