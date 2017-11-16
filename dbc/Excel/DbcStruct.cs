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
