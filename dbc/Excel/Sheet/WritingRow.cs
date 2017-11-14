using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel.Sheet
{
    class WritingRow : IEnumerable<WritingCell>
    {
        private WritingCell[] cells = new WritingCell[24];

        public WritingRow(IRow raw)
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                cells[i] = new WritingCell(raw, i);
            }

            NRow = raw;
        }

        public IRow NRow { get; }

        public WritingCell Transmitter => cells[0];
        public WritingCell MsgID => cells[1];
        public WritingCell MsgName => cells[2];
        public WritingCell MsgCycleTime => cells[3];
        public WritingCell Event => cells[4];
        public WritingCell PeriodicEvent => cells[5];
        public WritingCell MsgSize => cells[6];
        public WritingCell SignalName => cells[7];
        public WritingCell SizeInBits => cells[8];
        public WritingCell StartBit => cells[9];
        public WritingCell ByteOrder => cells[10];
        public WritingCell ValueType => cells[11];
        public WritingCell SigComment => cells[12];
        public WritingCell ValueDescs => cells[13];
        public WritingCell Unit => cells[14];
        public WritingCell Factor => cells[15];
        public WritingCell Offset => cells[16];
        public WritingCell LogicalMin => cells[17];
        public WritingCell PhysicalMin => cells[18];
        public WritingCell LogicalMax => cells[19];
        public WritingCell PhysicalMax => cells[20];
        public WritingCell SigStartValue => cells[21];
        public WritingCell Receiver => cells[22];
        public WritingCell MsgComment => cells[23];

        public IEnumerator<WritingCell> GetEnumerator()
        {
            return cells.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
