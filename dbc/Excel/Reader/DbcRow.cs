using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel.Reader
{
    [Flags]
    enum CellType
    {
        None = 0,
        TEXT = 0x1,
        DOUBLE = 0x2,
        SIGNED = 0x4 | DOUBLE,
        UNSIGNED = 0X8 | SIGNED,
    }

    class DbcCell
    {
        public DbcCell() { }

        public DbcCell(string val)
        {
            Val = val;
            Type = CellType.TEXT;
        }

        public DbcCell(double num)
        {
            Num = num;

            if ((int)num != num)
                Type = CellType.DOUBLE;
            else if ((int)num < 0)
                Type = CellType.SIGNED;
            else
                Type = CellType.UNSIGNED;
        }

        public bool Assert(CellType t)
        {
            if (t == CellType.None)
                return Type == t;

            return Type.HasFlag(t);
        }

        public string Val { get; private set; } = "";
        public double Num { get; private set; } = 0;

        private CellType Type { get; set; } = CellType.None;
    }

    class DbcRowInfo
    {
        public DbcRowInfo(int row)
        {
            RowNum = row;
        }

        public int RowNum { get; }
        public int Transmitter => 0;
        public int MsgID => 1;
        public int MsgName => 2;
        public int FixedPeriodic => 3;
        public int Event => 4;
        public int PeriodicEvent => 5;
        public int MsgSize => 6;
        public int SignalName => 7;
        public int SignalSize => 8;
        public int BitPos => 9;
        public int SignalComment => 10;
        public int State => 11;
        public int Unit => 12;
        public int Factor => 13;
        public int Offset => 14;
        public int LogicalMin => 15;
        public int PhysicalMin => 16;
        public int LogicalMax => 17;
        public int PhysicalMax => 18;
        public int DefaultVal => 19;
        public int DefaultTimeout => 20;
        public int Storage => 21;
        public int Receiver => 22;
        public int MsgComment => 23;
    }

    enum RowType
    {
        Unknown,
        Msg,
        Signal
    }

    public class DbcRow
    {
        internal static readonly DbcCell EmptyCell = new DbcCell();

        internal DbcRow(IRow row)
        {
            Raw = row;

            RowInfo = new DbcRowInfo(row.RowNum);

            if (MsgName != EmptyCell)
            {
                RowType = RowType.Msg;
            }
            else if (SignalName != EmptyCell)
            {
                RowType = RowType.Signal;
            }
            else
            {
                RowType = RowType.Unknown;
            }
        }

        internal IRow Raw { get; }

        internal DbcRowInfo RowInfo { get; }

        internal RowType RowType { get; }

        internal DbcCell Transmitter => GetCell(RowInfo.Transmitter);
        internal DbcCell MsgID => GetCell(RowInfo.MsgID);
        internal DbcCell MsgName => GetCell(RowInfo.MsgName);
        internal DbcCell FixedPeriodic => GetCell(RowInfo.FixedPeriodic);
        internal DbcCell Event => GetCell(RowInfo.Event);
        internal DbcCell PeriodicEvent => GetCell(RowInfo.PeriodicEvent);
        internal DbcCell MsgSize => GetCell(RowInfo.MsgSize);
        internal DbcCell SignalName => GetCell(RowInfo.SignalName);
        internal DbcCell SignalSize => GetCell(RowInfo.SignalSize);
        internal DbcCell BitPos => GetCell(RowInfo.BitPos);
        internal DbcCell SignalComment => GetCell(RowInfo.SignalComment);
        internal DbcCell State => GetCell(RowInfo.State);
        internal DbcCell Unit => GetCell(RowInfo.Unit);
        internal DbcCell Factor => GetCell(RowInfo.Factor);
        internal DbcCell Offset => GetCell(RowInfo.Offset);
        internal DbcCell PhysicalMin => GetCell(RowInfo.PhysicalMin);
        internal DbcCell PhysicalMax => GetCell(RowInfo.PhysicalMax);
        internal DbcCell DefaultVal => GetCell(RowInfo.DefaultVal);
        internal DbcCell Receiver => GetCell(RowInfo.Receiver);
        internal DbcCell MsgComment => GetCell(RowInfo.MsgComment);

        private DbcCell GetCell(int idx)
        {
            ICell cell = Raw.GetCell(idx);

            if (cell == null)
                return EmptyCell;

            if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
            {
                try
                {
                    return new DbcCell(cell.NumericCellValue);
                }
                catch (FormatException)
                {
                    //let it fall through
                }
            }
            else if (cell.CellType == NPOI.SS.UserModel.CellType.String)
            {
                string trimed = cell.StringCellValue.Trim();

                if (trimed.Length == 0)
                    return EmptyCell;

                return new DbcCell(trimed);
            }

            return EmptyCell;
        }
    }

}
