using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public enum AttrValType
    {
        Number,
        String
    }

    public class AttributeValue
    {
        private double num;
        private string val;

        public AttrValType Type { get; private set; }

        public double Num
        {
            get
            {
                return num;
            }
            set
            {
                num = value;

                Type = AttrValType.Number;
            }
        }

        public string Val
        {
            get
            {
                return val;
            }
            set
            {
                val = value;

                Type = AttrValType.String;
            }
        }
    }
}
