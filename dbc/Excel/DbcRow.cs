using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel
{
    public enum CellType
    {
        None,
        Number,
        String
    }

    class DbcCell
    {
        private DbcCell()
        {

        }

        private DbcCell(string val, int row, int col)
        {
            Val = val;
            Type = CellType.String;


            Row = row;
            Col = col;
        }

        private DbcCell(double num, int row, int col)
        {
            Num = num;
            Type = CellType.Number;

            Row = row;
            Col = col;
        }

        public static DbcCell NewDbcCell(ICell cell)
        {
            if (cell == null)
                return EmptyCell;

            if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
            {
                try
                {
                    return new DbcCell(cell.NumericCellValue,
                        cell.RowIndex,
                        cell.ColumnIndex);
                }
                catch (FormatException)
                {
                    //let it fall through
                }
            }
            else if (cell.CellType == NPOI.SS.UserModel.CellType.String)
            {
                string trimed = cell.StringCellValue.Trim();

                if (trimed.Length != 0)
                {
                    return new DbcCell(trimed,
                        cell.RowIndex,
                        cell.ColumnIndex);
                }
            }

            return EmptyCell;
        }

        public static readonly DbcCell EmptyCell = new DbcCell();

        public string Val { get; } = "";
        public double Num { get; }

        public int Row { get; }
        public int Col { get; }

        public CellType Type { get; }
    }

    enum RowType
    {
        Unknown,
        Msg,
        Signal
    }

    class DbcRow
    {
        private DbcCell[] cells = new DbcCell[24];

        public DbcRow(IRow row)
        {
            Raw = row;

            for (int i = 0; i < cells.Length; ++i)
                cells[i] = DbcCell.EmptyCell;

            foreach (var cell in row)
            {
                if (cell.ColumnIndex >= cells.Length)
                    return;

                cells[cell.ColumnIndex] = DbcCell.NewDbcCell(cell);
            }
        }

        public IRow Raw { get; }

        public RowType Type
        {
            get
            {
                if (MsgName != DbcCell.EmptyCell)
                    return RowType.Msg;

                if (SignalName != DbcCell.EmptyCell)
                    return RowType.Signal;

                return RowType.Unknown;
            }
        }

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
    }

}
