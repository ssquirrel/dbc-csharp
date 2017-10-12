using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel
{
    enum RowType
    {
        Unknown,
        Msg,
        Signal
    }

    class DbcRow
    {
        private DbcCell[] cells = new DbcCell[24];

        public DbcRow(IRow raw)
        {
            Row = raw.RowNum;

            foreach (var cell in raw)
            {
                if (cell.ColumnIndex >= cells.Length)
                    return;

                cells[cell.ColumnIndex] = new DbcCell(cell);
            }
        }

        public RowType Type
        {
            get
            {
                if (!MsgName.IsEmpty())
                    return RowType.Msg;

                if (!SignalName.IsEmpty())
                    return RowType.Signal;

                return RowType.Unknown;
            }
        }

        public int Row { get; }

        public DbcCell Transmitter => GetCell(0);
        public DbcCell MsgID => GetCell(1);
        public DbcCell MsgName => GetCell(2);
        public DbcCell FixedPeriodic => GetCell(3);
        public DbcCell Event => GetCell(4);
        public DbcCell PeriodicEvent => GetCell(5);
        public DbcCell MsgSize => GetCell(6);
        public DbcCell SignalName => GetCell(7);
        public DbcCell SignalSize => GetCell(8);
        public DbcCell BitPos => GetCell(9);
        public DbcCell DetailedMeaning => GetCell(10);
        public DbcCell State => GetCell(11);
        public DbcCell Unit => GetCell(12);
        public DbcCell Factor => GetCell(13);
        public DbcCell Offset => GetCell(14);
        public DbcCell LogicalMin => GetCell(15);
        public DbcCell PhysicalMin => GetCell(16);
        public DbcCell LogicalMax => GetCell(17);
        public DbcCell PhysicalMax => GetCell(18);
        public DbcCell DefaultVal => GetCell(19);
        public DbcCell DefaultTimeout => GetCell(20);
        public DbcCell Storage => GetCell(21);
        public DbcCell Receiver => GetCell(22);
        public DbcCell MsgComment => GetCell(23);

        public void Commmit(IRow row)
        {
            foreach (DbcCell cell in cells)
            {
                if (cell == null)
                    continue;

                if (cell.Type == CellType.Number)
                {
                    ICell raw = GetOrCreateCell(row, cell.Col);
                    raw.SetCellValue(cell.Num);
                }
                else if (cell.Type == CellType.String)
                {
                    ICell raw = GetOrCreateCell(row, cell.Col);
                    raw.SetCellValue(cell.Val);
                }

            }
        }

        private DbcCell GetCell(int idx)
        {
            DbcCell cell = cells[idx];

            if (cell != null)
                return cell;

            cells[idx] = new DbcCell(Row, idx);

            return cells[idx];
        }

        private static ICell GetOrCreateCell(IRow row, int col)
        {
            ICell cell = row.GetCell(col);

            if (cell != null)
                return cell;

            return row.CreateCell(col);
        }
    }

}
