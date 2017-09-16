using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Excel
{
    [Flags]
    public enum CellType
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

        public Cell(string value, CellType type)
        {
            Value = value;
            Type = type;
        }

        public string Value { get; private set; } = "";
        public CellType Type { get; private set; } = CellType.None;
    }

    class DbcExcelRow
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

        public static Cell EmptyCell = new Cell();

        public Cell[] row = new Cell[24];

        public DbcExcelRow()
        {
            for (int i = 0; i < row.Length; ++i)
                row[i] = EmptyCell;
        }

        public Cell Transmitter
        {
            get
            {
                return row[0];
            }
        }
        public Cell MsgID
        {
            get
            {
                return row[1];
            }
        }
        public Cell MsgName
        {
            get
            {
                return row[2];
            }
        }
        public Cell FixedPeriodic
        {
            get
            {
                return row[3];
            }
        }
        public Cell Event
        {
            get
            {
                return row[4];
            }
        }
        public Cell PeriodicEvent
        {
            get
            {
                return row[5];
            }
        }
        public Cell MsgSize
        {
            get
            {
                return row[6];
            }
        }
        public Cell SignalName
        {
            get
            {
                return row[7];
            }
        }
        public Cell SignalSize
        {
            get
            {
                return row[8];
            }
        }
        public Cell BitPos
        {
            get
            {
                return row[9];
            }
        }
        public Cell SignalComment
        {
            get
            {
                return row[10];
            }
        }
        public Cell SignalValDef
        {
            get
            {
                return row[11];
            }
        }
        public Cell Unit
        {
            get
            {
                return row[12];
            }
        }
        public Cell Factor
        {
            get
            {
                return row[13];
            }
        }
        public Cell Offset
        {
            get
            {
                return row[14];
            }
        }
        public Cell LogicalMin
        {
            get
            {
                return row[15];
            }
        }
        public Cell PhysicalMin
        {
            get
            {
                return row[16];
            }
        }
        public Cell LogicalMax
        {
            get
            {
                return row[17];
            }
        }
        public Cell PhysicalMax
        {
            get
            {
                return row[18];
            }
        }
        public Cell DefaultVal
        {
            get
            {
                return row[19];
            }
        }
        public Cell DefaultTimeout
        {
            get
            {
                return row[20];
            }
        }
        public Cell Storage
        {
            get
            {
                return row[21];
            }
        }
        public Cell Receiver
        {
            get
            {
                return row[22];
            }
        }
        public Cell MsgComment
        {
            get
            {
                return row[23];
            }
        }
    }
}
