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

    abstract class BasicCell
    {
        public abstract ICell NCell { get; }
        public abstract int Row { get; }
        public abstract int Col { get; }

        public CellType Type { get; protected set; }
        public string Val { get; protected set; } = "";
        public double Num { get; protected set; }

        public void Set(string val)
        {
            Val = val;
            Type = CellType.String;

            NCell.SetCellValue(val);
        }

        public void Set(double num)
        {
            Num = num;
            Type = CellType.Number;

            NCell.SetCellValue(num);
        }

    }

    abstract class DbcCell : BasicCell
    {
        public int State { get; private set; }

        public int GetInt()
        {
            bool p = TryGetDouble(this, out var num);

            if (p && num == (int)num)
                return PARSE_SUCCESS((int)num);

            return PARSE_FAILURE(0);
        }

        public long GetLong()
        {
            bool p = TryGetDouble(this, out var num);

            if (p && num == (long)num)
                return PARSE_SUCCESS((long)num);

            return PARSE_FAILURE(0);
        }

        public double GetDouble()
        {
            if (TryGetDouble(this, out var num))
                return PARSE_SUCCESS(num);

            return PARSE_FAILURE(0);
        }

        public string GetIdentifier()
        {
            if (Type == CellType.String && IsIdentifier(Val))
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
            if (Type == CellType.String && Hex.TryParse(Val, out long num))
                return PARSE_SUCCESS(num);

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

        public IList<string> GetReceiver()
        {
            if (Type == CellType.Blank)
                return PARSE_SUCCESS(new List<string> { "Vector__XXX" });

            string[] receivers = Val.Trim().Split(null);
            if (receivers.All(r => IsIdentifier(r)))
                return PARSE_SUCCESS(receivers.ToList());

            return PARSE_FAILURE(new List<string>());
        }

        public void SetHex(long hex)
        {
            Set("0x" + hex.ToString("X3"));
        }

        public void SetValueDescs(IEnumerable<ValueDesc> descs)
        {
            if (!descs.Any())
                return;

            StringBuilder builder = new StringBuilder();

            {
                var desc = descs.First();

                builder.AppendFormat("0x{0}:{1}",
                    ((int)desc.Num).ToString("X"),
                    desc.Val);
            }

            foreach (var desc in descs.Skip(1))
            {
                builder.AppendFormat("\n0x{0}:{1}",
                    ((int)desc.Num).ToString("X"),
                    desc.Val);
            }

            Set(builder.ToString());
        }

        public void SetReceiver(IEnumerable<string> receivers)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(receivers.First());

            foreach (var receiver in receivers.Skip(1))
                builder.Append("\n" + receiver);

            Set(builder.ToString());
        }

        private static bool TryGetDouble(DbcCell cell, out double num)
        {
            if (cell.Type == CellType.Number)
            {
                num = cell.Num;
                return true;
            }

            if (double.TryParse(cell.Val, out num))
                return true;

            num = 0;
            return false;
        }

        private T PARSE_SUCCESS<T>(T a)
        {
            return a;
        }

        private T PARSE_FAILURE<T>(T a)
        {
            State = 1;
            return a;
        }
    }

    static class Hex
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
