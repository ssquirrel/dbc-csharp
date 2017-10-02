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

}
