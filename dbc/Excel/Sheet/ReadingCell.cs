﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel.Sheet
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

    class ReadingCell
    {
        private readonly CellType type = CellType.Blank;
        private readonly string val = "";
        private readonly double num;

        public ReadingCell(ICell cell)
        {
            Row = cell.RowIndex;
            Col = cell.ColumnIndex;

            if (cell.CellType == NPOI.SS.UserModel.CellType.String)
            {
                string trimed = cell.StringCellValue.Trim();

                if (trimed.Length != 0)
                {
                    val = trimed;
                    type = CellType.String;
                }
            }
            else if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
            {
                try
                {
                    num = cell.NumericCellValue;
                    type = CellType.Number;
                }
                catch (FormatException) { }
            }
        }

        public ReadingCell(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public int Row { get; }
        public int Col { get; }

        public int State { get; private set; }

        public bool IsEmpty => type == CellType.Blank;

        public int Int()
        {
            bool p = TryGetDouble(out var num);

            if (p && num == (int)num)
                return PARSE_SUCCESS((int)num);

            return PARSE_FAILURE(0);
        }

        public long Long()
        {
            bool p = TryGetDouble(out var num);

            if (p && num == (long)num)
                return PARSE_SUCCESS((long)num);

            return PARSE_FAILURE(0);
        }

        public double Double()
        {
            if (TryGetDouble(out var num))
                return PARSE_SUCCESS(num);

            return PARSE_FAILURE(0);
        }

        public string Identifier()
        {
            if (type == CellType.String && IsIdentifier(val))
                return PARSE_SUCCESS(val);

            return PARSE_FAILURE(val);
        }

        public string CharString()
        {
            if (type == CellType.Blank)
                return PARSE_SUCCESS("");

            if (type == CellType.Number)
                return PARSE_SUCCESS(num.ToString());

            if (type == CellType.String && val.All(ch => ch != '"'))
                return PARSE_SUCCESS(val);

            return PARSE_FAILURE(val);
        }

        public long Hex()
        {
            if (type == CellType.String && HexTryParse(val, out long num))
                return PARSE_SUCCESS(num);

            return PARSE_FAILURE(0);
        }

        //"0" Motorola, big endian; "1" Intel, little endian
        public int ByteOrder()
        {
            if (type == CellType.Blank)
                return PARSE_SUCCESS(1);

            switch (val.ToLower())
            {
                case "intel":
                case "little":
                    return PARSE_SUCCESS(1);

                case "motorola":
                case "big":
                    return PARSE_SUCCESS(0);
            }

            return PARSE_FAILURE(-1);
        }

        //"-" signed "+" unsigned
        public string ValueType()
        {
            if (type == CellType.Blank)
                return PARSE_SUCCESS("-");

            switch (val.ToLower())
            {
                case "signed":
                    return PARSE_SUCCESS("-");

                case "unsigned":
                    return PARSE_SUCCESS("+");
            }

            return PARSE_FAILURE("");
        }

        public IList<ValueDesc> ValueDescs()
        {
            IList<ValueDesc> result = new List<ValueDesc>();

            if (type == CellType.Blank)
                return PARSE_SUCCESS(result);

            if (type == CellType.Number)
                return PARSE_FAILURE(result);

            string[] lines = val.Split('\n');
            foreach (string pair in lines)
            {
                int colon = pair.IndexOf(':');

                if (colon == -1)
                    return PARSE_FAILURE(result);

                string hex = pair.Substring(0, colon).Trim();
                string val = pair.Substring(colon + 1).Trim();

                if (!HexTryParse(hex.ToLower(), out int num) ||
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

        public IList<string> Receivers()
        {
            if (type == CellType.Blank)
                return PARSE_SUCCESS(new List<string> { "Vector__XXX" });

            if (type == CellType.String)
            {
                string[] receivers = val.Split(null);
                if (receivers.All(r => IsIdentifier(r)))
                    return PARSE_SUCCESS(receivers.ToList());
            }

            return PARSE_FAILURE(new List<string>());
        }

        private bool TryGetDouble(out double nn)
        {
            switch (type)
            {
                case CellType.Number:
                    nn = num;
                    return true;
                case CellType.String:
                    return double.TryParse(val, out nn);
                default:
                    nn = 0;
                    return false;
            }
        }

        private T PARSE_SUCCESS<T>(T a)
        {
            State = 0;
            return a;
        }

        private T PARSE_FAILURE<T>(T a)
        {
            State = 1;
            return a;
        }

        private static bool HexTryParse(string str, out int hex)
        {
            string hexstr = GetHexString(str);

            return int.TryParse(hexstr,
                NumberStyles.AllowHexSpecifier,
                null,
                out hex);
        }

        private static bool HexTryParse(string str, out long hex)
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

    /*
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

        //"0" Motorola, big endian; "1" Intel, little endian
        public int GetByteOrder()
        {
            if (Type == CellType.Blank)
                return PARSE_SUCCESS(1);

            switch (Val.ToLower())
            {
                case "intel":
                case "little":
                    return PARSE_SUCCESS(1);

                case "motorola":
                case "big":
                    return PARSE_SUCCESS(0);
            }

            return PARSE_FAILURE(-1);
        }

        //"-" signed "+" unsigned
        public string GetValueType()
        {
            if (Type == CellType.Blank)
                return PARSE_SUCCESS("-");

            switch (Val.ToLower())
            {
                case "signed":
                    return PARSE_SUCCESS("-");

                case "unsigned":
                    return PARSE_SUCCESS("+");
            }

            return PARSE_FAILURE("");
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

        //"0" Motorola, big endian; "1" Intel, little endian
        public void SetByteOrder(int order)
        {
            switch (order)
            {
                case 0:
                    Set("Motorola");
                    break;
                case 1:
                    Set("Intel");
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        //"-" signed "+" unsigned
        public void SetValueType(string vt)
        {
            switch (vt)
            {
                case "-":
                    Set("Signed");
                    break;
                case "+":
                    Set("Unsigned");
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public void SetValueDescs(IEnumerable<ValueDesc> descs)
        {
            StringBuilder builder = new StringBuilder();

            var first = descs.FirstOrDefault();

            if (first != null)
            {
                builder.AppendFormat("0x{0}:{1}",
                    ((int)first.Num).ToString("X"),
                    first.Val);

                foreach (var desc in descs.Skip(1))
                {
                    builder.AppendFormat("\n0x{0}:{1}",
                        ((int)desc.Num).ToString("X"),
                        desc.Val);
                }
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


    }
        */


}