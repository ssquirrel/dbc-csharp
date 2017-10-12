using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

using DbcLib.Model;

namespace DbcLib.Excel.Parser
{
    using static DBC.Lex.Common;

    public class ParseError
    {
        private DbcCell cell;

        internal ParseError(string s, DbcCell cell)
        {
            this.cell = cell;
        }
    }

    class CellParser
    {
        private List<ParseError> errors = new List<ParseError>();

        public IReadOnlyList<ParseError> Errors => errors;

        public int Unsigned(DbcCell cell)
        {
            if (TryGetNum(cell, out double num))
            {
                int u = (int)num;

                if (u == num)
                    return u;
            }

            errors.Add(new ParseError("", cell));

            return 0;
        }

        public double Double(DbcCell cell)
        {
            if (TryGetNum(cell, out double num))
                return num;

            errors.Add(new ParseError("", cell));

            return 0;
        }

        public string Identifier(DbcCell cell)
        {
            if (cell.Type == CellType.String &&
                IsIdentifier(cell.Val))
                return cell.Val;

            errors.Add(new ParseError("", cell));

            return "";
        }

        public string String(DbcCell cell)
        {
            if (cell == DbcCell.EmptyCell)
                return "";

            if (cell.Type == CellType.Number)
                return cell.Val.ToString();

            if (cell.Type == CellType.String &&
                cell.Val.All((ch) => ch != '"'))
                return cell.Val;

            errors.Add(new ParseError("", cell));

            return "";
        }

        public int Hex(DbcCell cell)
        {
            if (cell.Type == CellType.String &&
                HexTryParse(cell.Val, out int num))
                return num;

            errors.Add(new ParseError("", cell));

            return 0;
        }

        public int MsgSendType(DbcRow row)
        {
            bool cyclic = row.FixedPeriodic != DbcCell.EmptyCell;
            bool ifActive = row.Event != DbcCell.EmptyCell;
            bool TBD = row.PeriodicEvent != DbcCell.EmptyCell;

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
            if (cell.Type == CellType.Number)
                return (int)cell.Num;

            if (cell.Type == CellType.String)
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
            if (cell == DbcCell.EmptyCell)
                return new List<ValueDesc>();

            IList<ValueDesc> result = new List<ValueDesc>();

            string[] pairs = cell.Val.Split('\n');

            if (pairs.Length == 0)
            {
                errors.Add(new ParseError("", cell));
                return new List<ValueDesc>();
            }

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

                if (!HexTryParse(hex.ToLower(), out int num) ||
                    val.Any((ch) => ch == '"'))
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
            if (cell == DbcCell.EmptyCell)
            {
                return new List<string> { "Vector__XXX" };
            }

            string[] receivers = cell.Val.Trim().Split(null);

            if (receivers.All((r) => IsIdentifier(r)))
                return new List<string>(receivers);

            errors.Add(new ParseError("", cell));

            return new List<string>();
        }

        private static bool TryGetNum(DbcCell cell, out double num)
        {
            if (cell.Type == CellType.Number)
            {
                num = cell.Num;
                return true;
            }

            return double.TryParse(cell.Val, out num);
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
