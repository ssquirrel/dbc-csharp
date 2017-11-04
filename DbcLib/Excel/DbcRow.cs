using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;
using DbcLib.Excel.Parser;
using NPOI.SS.UserModel;
using System.Collections;

namespace DbcLib.Excel
{
    class DbcRow : IEnumerable<DbcCell>
    {
        private DbcCell[] cells = new DbcCell[24];

        public DbcRow(IRow raw)
        {
            Raw = raw;

            foreach (var cell in raw)
            {
                if (cell.ColumnIndex >= cells.Length)
                    break;

                cells[cell.ColumnIndex] = new DbcCell(cell);
            }

            for (int i = 0; i < cells.Length; ++i)
            {
                if (cells[i] == null)
                    cells[i] = new DbcCell(raw.RowNum, i);
            }
        }

        public IRow Raw { get; }
        public int Row => Raw.RowNum;

        public DbcCell Transmitter => cells[0];
        public DbcCell MsgID => cells[1];
        public DbcCell MsgName => cells[2];
        public DbcCell FixedPeriodic => cells[3];
        public DbcCell Event => cells[4];
        public DbcCell PeriodicEvent => cells[5];
        public DbcCell MsgSize => cells[6];
        public DbcCell SignalName => cells[7];
        public DbcCell SignalSize => cells[8];
        public DbcCell BitPos => cells[9];
        public DbcCell DetailedMeaning => cells[10];
        public DbcCell State => cells[11];
        public DbcCell Unit => cells[12];
        public DbcCell Factor => cells[13];
        public DbcCell Offset => cells[14];
        public DbcCell LogicalMin => cells[15];
        public DbcCell PhysicalMin => cells[16];
        public DbcCell LogicalMax => cells[17];
        public DbcCell PhysicalMax => cells[18];
        public DbcCell DefaultVal => cells[19];
        public DbcCell DefaultTimeout => cells[20];
        public DbcCell Storage => cells[21];
        public DbcCell Receiver => cells[22];
        public DbcCell MsgComment => cells[23];

        public void Commit()
        {
            foreach (var cell in this)
            {
                if (cell.Type == CellType.Blank)
                    continue;

                ICell rawCell = Raw.GetCell(cell.Col);
                if (rawCell == null)
                {
                    rawCell = Raw.CreateCell(cell.Col);
                }

                cell.Raw = rawCell;

                if (cell.Type == CellType.Number)
                {
                    rawCell.SetCellValue(cell.Num);
                }
                else if (cell.Type == CellType.String)
                {
                    rawCell.SetCellValue(cell.Val);
                }
            }
        }

        public IEnumerator<DbcCell> GetEnumerator()
        {
            return cells.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
