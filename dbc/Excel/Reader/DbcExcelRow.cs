using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

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

        public Cell(string value, int r, int c)
        {
            Row = r;
            Col = c;
            Value = value;
            Type = CellType.STRING;
        }

        public Cell(double num, int r, int c)
        {
            Row = r;
            Col = c;
            Value = num.ToString();
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
            return (Type & t) != Type;
        }

        public int Row { get; private set; } = 0;
        public int Col { get; private set; } = 0;
        public string Value { get; private set; } = "";
        public double Num { get; private set; } = 0;
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

        private Cell[] raw = new Cell[24];

        public DbcExcelRow(IRow row)
        {
            for (int i = 0; i < raw.Length; ++i)
                raw[i] = EmptyCell;


            foreach (ICell cell in row)
            {
                if (cell.ColumnIndex >= raw.Length)
                    break;

                switch (cell.CellType)
                {
                    case NPOI.SS.UserModel.CellType.Numeric:

                        raw[cell.ColumnIndex] = new Cell(cell.NumericCellValue,
                                cell.RowIndex,
                                cell.ColumnIndex);
                        break;
                    case NPOI.SS.UserModel.CellType.String:
                        raw[cell.ColumnIndex] = new Cell(cell.StringCellValue,
                                cell.RowIndex,
                                cell.ColumnIndex);

                        break;
                }
            }
        }

        public Cell Transmitter
        {
            get
            {
                return raw[0];
            }
        }
        public Cell MsgID
        {
            get
            {
                return raw[1];
            }
        }
        public Cell MsgName
        {
            get
            {
                return raw[2];
            }
        }
        public Cell FixedPeriodic
        {
            get
            {
                return raw[3];
            }
        }
        public Cell Event
        {
            get
            {
                return raw[4];
            }
        }
        public Cell PeriodicEvent
        {
            get
            {
                return raw[5];
            }
        }
        public Cell MsgSize
        {
            get
            {
                return raw[6];
            }
        }
        public Cell SignalName
        {
            get
            {
                return raw[7];
            }
        }
        public Cell SignalSize
        {
            get
            {
                return raw[8];
            }
        }
        public Cell BitPos
        {
            get
            {
                return raw[9];
            }
        }
        public Cell SignalComment
        {
            get
            {
                return raw[10];
            }
        }
        public Cell SignalValDef
        {
            get
            {
                return raw[11];
            }
        }
        public Cell Unit
        {
            get
            {
                return raw[12];
            }
        }
        public Cell Factor
        {
            get
            {
                return raw[13];
            }
        }
        public Cell Offset
        {
            get
            {
                return raw[14];
            }
        }
        public Cell LogicalMin
        {
            get
            {
                return raw[15];
            }
        }
        public Cell PhysicalMin
        {
            get
            {
                return raw[16];
            }
        }
        public Cell LogicalMax
        {
            get
            {
                return raw[17];
            }
        }
        public Cell PhysicalMax
        {
            get
            {
                return raw[18];
            }
        }
        public Cell DefaultVal
        {
            get
            {
                return raw[19];
            }
        }
        public Cell DefaultTimeout
        {
            get
            {
                return raw[20];
            }
        }
        public Cell Storage
        {
            get
            {
                return raw[21];
            }
        }
        public Cell Receiver
        {
            get
            {
                return raw[22];
            }
        }
        public Cell MsgComment
        {
            get
            {
                return raw[23];
            }
        }
    }


}
