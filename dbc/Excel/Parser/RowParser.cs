using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

using DbcLib.Model;
using DbcLib.Excel.Reader;

namespace DbcLib.Excel.Parser
{
    using static DBC.Lex.Common;

    public class ParseError
    {
        internal ParseError(string s, DbcCell cell)
        {

        }
    }

    class RowParser
    {
        private List<ParseError> errors = new List<ParseError>();

        public IReadOnlyCollection<ParseError> Errors => errors;

        public ParsedRow Parse(DbcRow row)
        {
            if (row.RowType == RowType.Msg)
                return new MsgRow(this, row);

            if (row.RowType == RowType.Signal)
                return new SignalRow(this, row);

            throw new ArgumentException();
        }

        public int Unsigned(DbcCell cell)
        {
            if (cell.Assert(CellType.UNSIGNED))
                return (int)cell.Num;

            if (cell.Assert(CellType.TEXT) &&
                int.TryParse(cell.Val, out int num) &&
                num >= 0)
                return num;

            errors.Add(new ParseError("", cell));

            return 0;
        }

        public double Double(DbcCell cell)
        {
            if (cell.Assert(CellType.DOUBLE))
                return cell.Num;

            if (cell.Assert(CellType.TEXT) &&
                double.TryParse(cell.Val, out double num))
                return num;

            errors.Add(new ParseError("", cell));

            return 0;
        }

        public string Identifier(DbcCell cell)
        {
            if (cell.Assert(CellType.TEXT) &&
                IsIdentifier(cell.Val))
                return cell.Val;

            errors.Add(new ParseError("", cell));

            return "";
        }

        public string String(DbcCell cell)
        {
            if (cell.Assert(CellType.None))
                return "";

            if (cell.Assert(CellType.DOUBLE))
                return cell.Val.ToString();

            if (cell.Assert(CellType.TEXT) &&
                cell.Val.All((ch) => ch != '"'))
                return cell.Val;

            errors.Add(new ParseError("", cell));

            return "";
        }

        public int Hex(DbcCell cell)
        {
            if (cell.Assert(CellType.TEXT) &&
                HexTryParse(cell.Val, out int num))
                return num;

            errors.Add(new ParseError("", cell));

            return 0;
        }

        public int MsgSendType(DbcRow row)
        {
            bool cyclic = row.FixedPeriodic != DbcRow.EmptyCell;
            bool ifActive = row.Event != DbcRow.EmptyCell;
            bool TBD = row.PeriodicEvent != DbcRow.EmptyCell;

            if (cyclic && !ifActive && !TBD)
                return DbcTemplate.MsgSendType_Cyclic;

            if (ifActive && !cyclic && !TBD)
                return DbcTemplate.MsgSendType_IfActive;

            if (TBD && !cyclic && !ifActive)
                return 2; //?????

            errors.Add(new ParseError("", row.FixedPeriodic));

            return 0;
        }

        public int StartBit(DbcCell cell)
        {
            if (cell.Assert(CellType.UNSIGNED))
                return (int)cell.Num;

            if (cell.Assert(CellType.TEXT))
            {
                StringBuilder builder = new StringBuilder();

                foreach (char ch in cell.Val)
                {
                    if (!IsDigit(ch))
                        break;

                    builder.Append(ch);
                }

                if (int.TryParse(builder.ToString(), out int num))
                    return num;
            }

            errors.Add(new ParseError("", cell));

            return 0;
        }

        public IList<ValueDesc> SignalValDescs(DbcCell cell)
        {
            if (!cell.Assert(CellType.TEXT))
            {
                return new List<ValueDesc>();
            }

            IList<ValueDesc> result = new List<ValueDesc>();

            string[] pairs = cell.Val.Split('\n');
            foreach (string pairstr in pairs)
            {
                int colon = pairstr.IndexOf(':');

                if (colon == -1)
                {
                    errors.Add(new ParseError("", cell));

                    break;
                }

                string hex = pairstr.Substring(0, colon).Trim();
                string val = pairstr.Substring(colon + 1).Trim();

                if (!HexTryParse(hex.ToLower(), out int num))
                {
                    errors.Add(new ParseError("", cell));

                    break;
                }

                if (val.Any((ch) => ch == '"'))
                {
                    errors.Add(new ParseError("", cell));

                    break;
                }

                result.Add(new ValueDesc
                {
                    Num = num,
                    Val = val
                });
            }

            return result;
        }

        public IList<string> Receiver(DbcCell cell)
        {
            if (cell == DbcRow.EmptyCell)
            {
                return new List<string> { "Vector__XXX" };
            }


            string[] receivers = cell.Val.Trim().Split(null);

            if (receivers.All((r) => IsIdentifier(r)))
                return new List<string>(receivers);

            errors.Add(new ParseError("", cell));

            return new List<string>();
        }

        private static bool HexTryParse(string hexstring, out int hex)
        {
            if (!hexstring.StartsWith("0x"))
            {
                hex = 0;
                return false;
            }

            return int.TryParse(hexstring.Substring(2, hexstring.Length - 2),
                NumberStyles.AllowHexSpecifier,
                null,
                out hex);
        }
    }

    abstract class ParsedRow
    {
        private RowParser ct;
        private DbcRow row;

        public ParsedRow(RowParser ct, DbcRow row)
        {
            this.ct = ct;
            this.row = row;
        }

        public abstract string Transmitter { get; }
        public abstract int MsgID { get; }
        public abstract string MsgName { get; }
        public abstract int MsgSendType { get; }
        public abstract int MsgCycleTime { get; }
        public abstract int MsgSize { get; }
        public abstract string SignalName { get; }
        public abstract int SignalSize { get; }
        public abstract int StartBit { get; }
        public abstract string ByteOrder { get; }
        public abstract string ValueType { get; }
        public abstract string SignalComment { get; }
        public abstract IList<ValueDesc> SignalValDescs { get; }
        public abstract string Unit { get; }
        public abstract double Factor { get; }
        public abstract double Offset { get; }
        public abstract double Min { get; }
        public abstract double Max { get; }
        //DefaultVal;
        //DefaultTimeout;
        public abstract IList<string> Receiver { get; }
        public abstract string MsgComment { get; }
    }

    class MsgRow : ParsedRow
    {
        public MsgRow(RowParser ct, DbcRow row) : base(ct, row)
        {
            Transmitter = ct.Identifier(row.Transmitter);
            MsgID = ct.Hex(row.MsgID);
            MsgName = ct.Identifier(row.MsgName);
            MsgSendType = ct.MsgSendType(row);
            MsgSize = ct.Unsigned(row.MsgSize);
            MsgComment = ct.String(row.MsgComment);

            if (MsgSendType == DbcTemplate.MsgSendType_Cyclic)
                MsgCycleTime = ct.Unsigned(row.FixedPeriodic);
            else
                MsgCycleTime = -1;
        }

        public override string Transmitter { get; }
        public override int MsgID { get; }
        public override string MsgName { get; }
        public override int MsgSendType { get; }
        public override int MsgCycleTime { get; }
        public override int MsgSize { get; }
        public override string MsgComment { get; }

        public override string SignalName => throw new NotImplementedException();
        public override int SignalSize => throw new NotImplementedException();
        public override int StartBit => throw new NotImplementedException();
        public override string SignalComment => throw new NotImplementedException();
        public override IList<ValueDesc> SignalValDescs => throw new NotImplementedException();
        public override string Unit => throw new NotImplementedException();
        public override double Factor => throw new NotImplementedException();
        public override double Offset => throw new NotImplementedException();
        public override double Min => throw new NotImplementedException();
        public override double Max => throw new NotImplementedException();
        public override string ByteOrder => throw new NotImplementedException();
        public override string ValueType => throw new NotImplementedException();

        public override IList<string> Receiver => throw new NotImplementedException();
    }

    class SignalRow : ParsedRow
    {
        public SignalRow(RowParser ct, DbcRow row) : base(ct, row)
        {
            SignalName = ct.Identifier(row.SignalName);
            SignalSize = ct.Unsigned(row.SignalSize);
            StartBit = ct.StartBit(row.BitPos);
            SignalComment = ct.String(row.SignalComment);
            SignalValDescs = ct.SignalValDescs(row.State);
            Unit = ct.String(row.Unit);
            Factor = ct.Double(row.Factor);
            Offset = ct.Double(row.Offset);
            Min = ct.Double(row.PhysicalMin);
            Max = ct.Double(row.PhysicalMax);
            Receiver = ct.Receiver(row.Receiver);

            ByteOrder = "0";
            ValueType = "+";
        }

        public override string SignalName { get; }
        public override int SignalSize { get; }
        public override int StartBit { get; }
        public override string ByteOrder { get; }
        public override string ValueType { get; }
        public override string SignalComment { get; }
        public override IList<ValueDesc> SignalValDescs { get; }
        public override string Unit { get; }
        public override double Factor { get; }
        public override double Offset { get; }
        public override double Min { get; }
        public override double Max { get; }
        public override IList<string> Receiver { get; }

        public override string Transmitter => throw new NotImplementedException();
        public override int MsgID => throw new NotImplementedException();
        public override string MsgName => throw new NotImplementedException();
        public override int MsgSendType => throw new NotImplementedException();
        public override int MsgCycleTime => throw new NotImplementedException();
        public override int MsgSize => throw new NotImplementedException();
        public override string MsgComment => throw new NotImplementedException();
    }

}
