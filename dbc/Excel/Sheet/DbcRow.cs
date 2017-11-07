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
            NRow = raw;

            foreach (var cell in raw)
            {
                if (cell.ColumnIndex >= cells.Length)
                    break;

                cells[cell.ColumnIndex] = new NormalCell(cell);
            }

            for (int i = 0; i < cells.Length; ++i)
            {
                if (cells[i] == null)
                    cells[i] = new EmptyCell(NRow, i);
            }
        }

        public IRow NRow { get; }

        public int Row => NRow.RowNum;

        public DbcCell Transmitter => cells[0];
        public DbcCell MsgID => cells[1];
        public DbcCell MsgName => cells[2];
        public DbcCell MsgCycleTime => cells[3];
        public DbcCell Event => cells[4];
        public DbcCell PeriodicEvent => cells[5];
        public DbcCell MsgSize => cells[6];
        public DbcCell SignalName => cells[7];
        public DbcCell SizeInBits => cells[8];
        public DbcCell StartBit => cells[9];
        public DbcCell ByteOrder => cells[10];
        public DbcCell ValueType => cells[11];
        public DbcCell SigComment => cells[12];
        public DbcCell ValueDescs => cells[13];
        public DbcCell Unit => cells[14];
        public DbcCell Factor => cells[15];
        public DbcCell Offset => cells[16];
        public DbcCell LogicalMin => cells[17];
        public DbcCell PhysicalMin => cells[18];
        public DbcCell LogicalMax => cells[19];
        public DbcCell PhysicalMax => cells[20];
        public DbcCell SigStartValue => cells[21];
        public DbcCell Receiver => cells[22];
        public DbcCell MsgComment => cells[23];

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
