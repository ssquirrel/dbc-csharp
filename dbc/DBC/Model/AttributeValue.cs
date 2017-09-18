using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class AttributeValue
    {
        public enum ActiveField
        {
            None,
            Numerical,
            String
        }

        private string val = "";
        private double num;

        public ActiveField Active { get; private set; } = ActiveField.None;

        public String Val
        {
            get
            {
                return val;
            }

            set
            {
                val = value;
                Active = ActiveField.String;
            }
        }

        public double Num
        {
            get
            {
                return num;
            }

            set
            {
                num = value;
                Active = ActiveField.Numerical;
            }
        }

        public override string ToString()
        {
            if (Active == ActiveField.Numerical)
                return num.ToString();

            if (Active == ActiveField.String)
                return "\"" + val + "\"";

            return "";
        }
    }
}
