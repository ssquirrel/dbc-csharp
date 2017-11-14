using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;
using DbcLib.Excel.Parser;
using NPOI.SS.UserModel;
using System.Collections;

namespace DbcLib.Excel.Sheet
{
    class ReadingRow : IEnumerable<ReadingCell>
    {
        private ReadingCell[] cells = new ReadingCell[24];

        public ReadingRow(IRow raw)
        {
            NRow = raw;

            foreach (var cell in raw)
            {
                if (cell.ColumnIndex >= cells.Length)
                    break;

                cells[cell.ColumnIndex] = new ReadingCell(cell);
            }

            for (int i = 0; i < cells.Length; ++i)
            {
                if (cells[i] == null)
                    cells[i] = new ReadingCell(raw.RowNum, i);
            }
        }

        public IRow NRow { get; }

        public int Row => NRow.RowNum;

        public ReadingCell Transmitter => cells[0];
        public ReadingCell MsgID => cells[1];
        public ReadingCell MsgName => cells[2];
        public ReadingCell MsgCycleTime => cells[3];
        public ReadingCell Event => cells[4];
        public ReadingCell PeriodicEvent => cells[5];
        public ReadingCell MsgSize => cells[6];
        public ReadingCell SignalName => cells[7];
        public ReadingCell SizeInBits => cells[8];
        public ReadingCell StartBit => cells[9];
        public ReadingCell ByteOrder => cells[10];
        public ReadingCell ValueType => cells[11];
        public ReadingCell SigComment => cells[12];
        public ReadingCell ValueDescs => cells[13];
        public ReadingCell Unit => cells[14];
        public ReadingCell Factor => cells[15];
        public ReadingCell Offset => cells[16];
        public ReadingCell LogicalMin => cells[17];
        public ReadingCell PhysicalMin => cells[18];
        public ReadingCell LogicalMax => cells[19];
        public ReadingCell PhysicalMax => cells[20];
        public ReadingCell SigStartValue => cells[21];
        public ReadingCell Receiver => cells[22];
        public ReadingCell MsgComment => cells[23];

        public IEnumerator<ReadingCell> GetEnumerator()
        {
            return cells.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
