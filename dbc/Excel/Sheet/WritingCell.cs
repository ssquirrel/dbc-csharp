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
    }
}
