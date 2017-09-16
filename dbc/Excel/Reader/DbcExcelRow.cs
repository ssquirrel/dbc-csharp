using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Excel.Reader
{
    [Flags]
    enum CellType
    {
        None = 0,
        UNSIGNED = 0x1,
        SIGNED = 0x2 | UNSIGNED,
        DOUBLE = 0X4 | SIGNED,
        STRING = 0x8 | DOUBLE
    }

    class Cell
    {
        public Cell()
        {
        }

        public Cell(int r, int c, string value, CellType type)
        {
            Row = r;
            Col = c;
            Value = value;
            Type = type;
        }

        public Cell(int r, int c, double num, CellType type)
        {
            Row = r;
            Col = c;
            Value = num.ToString();
            Num = num;
            Type = type;
        }

        public bool Assert(CellType t)
        {
            return (Type & t) != Type;
        }

        public int Row { get; private set; } = 0;
        public int Col { get; private set; } = 0;
        public string Value { get; private set; } = "";
        public double Num { get; private set; } = 0;
        public CellType Type { get; private set; } = CellType.None;
    }

    static class DbcExcelRow
    {
        /*
        Transmitter,
        MsgID,
        MsgName,
        FixedPeriodic,
        Event,
        PeriodicEvent,
        MsgSize,
        SignalName,
        SignalSize,
        BitPos,
        SignalComment,
        SignalValDef,
        Unit,
        Factor,
        Offset,
        LogicalMin,
        PhysicalMin,
        LogicalMax,
        PhysicalMax,
        DefaultVal,
        DefaultTimeout,
        Storage,
        Receiver,
        MsgComment
        */

        public const int Transmitter = 0;
        public const int MsgID = 1;
        public const int MsgName = 2;
        public const int FixedPeriodic = 3;
        public const int Event = 4;
        public const int PeriodicEvent = 5;
        public const int MsgSize = 6;
        public const int SignalName = 7;
        public const int SignalSize = 8;
        public const int BitPos = 9;
        public const int SignalComment = 10;
        public const int SignalValDef = 11;
        public const int Unit = 12;
        public const int Factor = 13;
        public const int Offset = 14;
        public const int LogicalMin = 15;
        public const int PhysicalMin = 16;
        public const int LogicalMax = 17;
        public const int PhysicalMax = 18;
        public const int DefaultVal = 19;
        public const int DefaultTimeout = 20;
        public const int Storage = 21;
        public const int Receiver = 22;
        public const int MsgComment = 23;

        public static Cell EmptyCell = new Cell();

        public static Cell[] CreateRow()
        {
            Cell[] row = new Cell[24];
            for (int i = 0; i < row.Length; ++i)
                row[i] = EmptyCell;

            return row;
        }
    }
}
