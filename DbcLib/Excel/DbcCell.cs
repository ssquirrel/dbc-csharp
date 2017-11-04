using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel
{
    using DbcLib.Model;
    using System.Globalization;
    using static DBC.Lex.Common;

    enum CellType
    {
        Blank,
        Number,
        String
    }

    class BasicCell
    {
        public BasicCell(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public BasicCell(ICell cell)
        {
            Raw = cell;

            Row = cell.RowIndex;
            Col = cell.ColumnIndex;

            if (cell.CellType == NPOI.SS.UserModel.CellType.String)
            {
                string trimed = cell.StringCellValue.Trim();

                if (trimed.Length != 0)
                {
                    Set(cell.StringCellValue.Trim());
                }
            }
            else if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
            {
                try
                {
                    Set(cell.NumericCellValue);
                }
                catch (FormatException) { }
            }
        }

        public ICell Raw { get; set; }

        public int Row { get; }
        public int Col { get; }

        public string Val { get; private set; } = "";
        public double Num { get; private set; }

        public CellType Type { get; private set; }

        public bool State { get; private set; } = true;

        public void Set(string val)
        {
            Val = val;
            Type = CellType.String;
        }

        public void Set(double num)
        {
            Num = num;
            Type = CellType.Number;
        }

        protected T PARSE_SUCCESS<T>(T a)
        {
            State = true;
            return a;
        }

        protected T PARSE_FAILURE<T>(T a)
        {
            State = false;
            return a;
        }
    }

    class DbcCell : BasicCell
    {
        public DbcCell(int row, int col) : base(row, col)
        {
        }

        public DbcCell(ICell cell) : base(cell)
        {
        }

        public int GetUnsigned()
        {
            double num = GetDouble();

            if (State && num == (int)num)
                return (int)num;

            return PARSE_FAILURE(0);
        }

        public double GetDouble()
        {
            if (Type == CellType.Number)
                return PARSE_SUCCESS(Num);

            if (double.TryParse(Val, out double num))
                return PARSE_SUCCESS(num);

            return PARSE_FAILURE(0);
        }

        public string GetIdentifier()
        {
            if (Val.Length > 0 && IsIdentifier(Val))
                return PARSE_SUCCESS(Val);

            return PARSE_FAILURE(Val);
        }

        public string GetCharString()
        {
            if (Type == CellType.Blank)
                return PARSE_SUCCESS("");

            if (Type == CellType.Number)
                return PARSE_SUCCESS(Num.ToString());

            if (Type == CellType.String && Val.All(ch => ch != '"'))
                return PARSE_SUCCESS(Val);

            return PARSE_FAILURE(Val);
        }

        public long GetHex()
        {
            if (Hex.TryParse(Val, out long num))
                return PARSE_SUCCESS(num);

            return PARSE_FAILURE(0);
        }

        public void SetHex(long hex)
        {
            Set("0x" + hex.ToString("X3"));
        }

        public int GetStartBit()
        {
            if (Type == CellType.Number)
                return (int)PARSE_SUCCESS(Num);

            if (Type == CellType.String)
            {
                StringBuilder builder = new StringBuilder();

                foreach (char ch in Val)
                {
                    if (!IsDigit(ch))
                        break;

                    builder.Append(ch);
                }

                if (int.TryParse(builder.ToString(), out int num))
                    return PARSE_SUCCESS(num);
            }

            return PARSE_FAILURE(0);
        }

        public IList<ValueDesc> GetValueDescs()
        {
            IList<ValueDesc> result = new List<ValueDesc>();

            if (Type == CellType.Blank)
                return PARSE_SUCCESS(result);

            string[] lines = Val.Split('\n');
            foreach (string pair in lines)
            {
                int colon = pair.IndexOf(':');

                if (colon == -1)
                    return PARSE_FAILURE(result);

                string hex = pair.Substring(0, colon).Trim();
                string val = pair.Substring(colon + 1).Trim();

                if (!Hex.TryParse(hex.ToLower(), out int num) ||
                    val.Any(ch => ch == '"'))
                {
                    return PARSE_FAILURE(result);
                }

                result.Add(new ValueDesc
                {
                    Num = num,
                    Val = val
                });
            }

            return PARSE_SUCCESS(result);
        }

        public void SetValueDescs(IList<ValueDesc> descs)
        {
            var desc = descs[0];

            string result = "0x" + ((int)desc.Num).ToString("X") + ":" + desc.Val;

            for (int i = 1; i < descs.Count; ++i)
            {
                desc = descs[i];
                result += "\n0x" + ((int)desc.Num).ToString("X") + ":" + desc.Val;
            }

            Set(result);
        }

        public IList<string> GetReceiver()
        {
            if (Type == CellType.Blank)
                return PARSE_SUCCESS(new List<string> { "Vector__XXX" });

            string[] receivers = Val.Trim().Split(null);
            if (receivers.All(r => IsIdentifier(r)))
                return PARSE_SUCCESS(receivers.ToList());

            return PARSE_FAILURE(new List<string>());
        }

        public void SetReceiver(IList<string> receivers)
        {
            string result = receivers[0];

            for (int i = 1; i < receivers.Count; ++i)
                result += "\n" + receivers[i];

            Set(result);
        }
    }

    class Hex
    {
        public static bool TryParse(string str, out int hex)
        {
            string hexstr = GetHexString(str);

            return int.TryParse(hexstr,
                NumberStyles.AllowHexSpecifier,
                null,
                out hex);
        }

        public static bool TryParse(string str, out long hex)
        {
            string hexstr = GetHexString(str);

            return long.TryParse(hexstr,
                NumberStyles.AllowHexSpecifier,
                null,
                out hex);
        }

        private static string GetHexString(string str)
        {
            if (str.StartsWith("0x"))
            {
                return str.Substring(2, str.Length - 2);
            }

            return null;
        }
    }
}
