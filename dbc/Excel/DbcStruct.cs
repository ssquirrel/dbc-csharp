using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.DBC.Parser;
using DbcLib.Model;
using DbcLib.Model.PropTree;

namespace DbcLib.Excel
{
    enum MsgSendTypeEnum
    {
        Cyclic,
        IfActive,
        CyclicEvent,
        NoMsgSendType
    }

    static class StartBitConverter
    {
        public static int MSB(int lsb, int len)
        {
            int b = lsb / 8;
            int bM = b * 8 + 7;

            int sub1 = bM - lsb + 1;

            if (sub1 >= len)
            {
                return lsb + len - 1;
            }

            int f = (len - sub1) / 8;
            int sub2 = len - f * 8 - sub1;

            int rest = sub2 == 0 ? 7 : sub2 - 9;

            int startBit = (b - f) * 8 + rest;

            return startBit;
        }

        public static int LSB(int msb, int len)
        {
            int b = msb / 8;
            int bl = b * 8;

            int sub1 = msb - bl + 1;

            if (sub1 >= len)
            {
                return msb - len + 1;
            }

            int f = (len - sub1) / 8;
            int sub2 = len - f * 8 - sub1;

            int rest = sub2 == 0 ? 0 : 16 - sub2;

            int startBit = (f + b) * 8 + rest;

            return startBit;
        }
    }

    static class MsgSendType
    {
        public const string AttributeName = "GenMsgSendType";

        public static MsgSendTypeEnum ToEnum(string type)
        {
            switch (type.ToLower())
            {
                case "fixedperiodic":
                case "cyclic":
                    return MsgSendTypeEnum.Cyclic;
                case "ifactive":
                case "event":
                    return MsgSendTypeEnum.IfActive;
                case "cyclicevent":
                    return MsgSendTypeEnum.CyclicEvent;
                default:
                    return MsgSendTypeEnum.NoMsgSendType;
            }
        }

        public static AttributeDefinition Definition()
        {
            return new AttributeDefinition
            {
                AttributeName = AttributeName,
                ObjectType = Keyword.MESSAGES,
                ValueType = "ENUM",
                Values = new List<string>
                {
                    "Cyclic","IfActive","CyclicEvent","NoMsgSendType"
                }
            };
        }

        public static AttributeDefault Default()
        {
            return new AttributeDefault
            {
                AttributeName = AttributeName,
                Value = new AttributeValue
                {
                    Val = "NoMsgSendType"
                }
            };
        }

    }

    static class MsgCycleTime
    {
        public const string AttributeName = "GenMsgCycleTime";

        public static AttributeDefinition Definition()
        {
            return new AttributeDefinition
            {
                AttributeName = AttributeName,
                ObjectType = Keyword.MESSAGES,
                ValueType = "INT",
                Num1 = 0,
                Num2 = 100000
            };
        }

        public static AttributeDefault Default()
        {
            return new AttributeDefault
            {
                AttributeName = AttributeName,
                Value = new AttributeValue
                {
                    Num = 0
                }
            };
        }

    }

    static class SigStartValue
    {
        public const string AttributeName = "GenSigStartValue";

        public static AttributeDefinition Definition()
        {
            return new AttributeDefinition
            {
                AttributeName = AttributeName,
                ObjectType = Keyword.SIGNAL,
                ValueType = "INT",
                Num1 = 0,
                Num2 = 100000
            };
        }

        public static AttributeDefault Default()
        {
            return new AttributeDefault
            {
                AttributeName = AttributeName,
                Value = new AttributeValue
                {
                    Num = 0
                }
            };
        }
    }
}
