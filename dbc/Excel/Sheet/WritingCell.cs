using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;
using NPOI.SS.UserModel;

namespace DbcLib.Excel.Sheet
{
    class WritingCell
    {
        private Lazy<ICell> lazy;

        public WritingCell(IRow row, int col)
        {
            lazy = new Lazy<ICell>(() =>
            {
                return row.GetCell(col, MissingCellPolicy.CREATE_NULL_AS_BLANK);
            }, false);
        }

        public ICell NCell => lazy.Value;

        public ICellStyle Style
        {
            get => NCell.CellStyle;

            set => NCell.CellStyle = value;
        }

        public void Set(string val)
        {
            NCell.SetCellValue(val);
        }

        public void Set(double num)
        {
            NCell.SetCellValue(num);
        }

        public void SetHex(long hex)
        {
            Set("0x" + hex.ToString("X3"));
        }

        public void SetStartBit(int bit, int len, int order)
        {
            if (order == Signal.Intel)
            {
                Set(bit);
            }
            else
            {
                Set(StartBitConverter.LSB(bit, len));
            }
        }

        public void SetByteOrder(int order)
        {
            switch (order)
            {
                case Signal.Motorola:
                    Set("Motorola");
                    break;
                case Signal.Intel:
                    Set("Intel");
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        //"-" signed "+" unsigned
        public void SetValueType(string type)
        {
            switch (type)
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
            if (!descs.Any())
                return;

            descs = descs.OrderBy(desc => desc.Num);

            var first = descs.FirstOrDefault();

            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("0x{0}:{1}",
                ((int)first.Num).ToString("X"),
                first.Val);

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
    }
}
