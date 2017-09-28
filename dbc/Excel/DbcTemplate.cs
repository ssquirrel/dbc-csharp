using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;

namespace DbcLib.Excel
{
    static class DbcTemplate
    {
        public static readonly string
        SendType_Cyclic = "Cyclic";

        public static readonly string
        SendType_IfActive = "IfActive";

        public static readonly AttributeDefinition
        MsgSendType = new AttributeDefinition
        {
            AttributeName = "GenMsgSendType",
            ObjectType = Keyword.MESSAGES,
            ValueType = "ENUM",
            Values = new List<string> {
                    SendType_Cyclic,
                    SendType_IfActive,
                    "NoMsgSendType"
                }
        };

        public static readonly AttributeDefinition
        MsgCycleTime = new AttributeDefinition
        {
            AttributeName = "GenMsgCycleTime",
            ObjectType = Keyword.MESSAGES,
            ValueType = "INT",
            Num1 = 0,
            Num2 = 10000
        };

        public static readonly AttributeDefinition
        SigStartValue = new AttributeDefinition
        {
            AttributeName = "GenSigStartValue",
            ObjectType = Keyword.SIGNAL,
            ValueType = "INT",
            Num1 = 0,
            Num2 = 7
        };

        public static readonly AttributeDefault
        MsgSendTypeDefault = new AttributeDefault
        {
            AttributeName = MsgSendType.AttributeName,
            Value = new AttributeValue
            {
                Val = "Cyclic"
            }
        };

        public static readonly AttributeDefault
        MsgCycleTimeDefault = new AttributeDefault
        {
            AttributeName = MsgCycleTime.AttributeName,
            Value = new AttributeValue
            {
                Num = 100
            }
        };

        public static readonly AttributeDefault
        SigStartValueDefault = new AttributeDefault
        {
            AttributeName = SigStartValue.AttributeName,
            Value = new AttributeValue
            {
                Num = 0
            }
        };

        public static Model.DBC NewDbcObject()
        {
            Model.DBC dbc = new Model.DBC();

            dbc.Version = "";

            dbc.AddAttrDefinition(MsgSendType);
            dbc.AddAttrDefinition(MsgCycleTime);
            dbc.AddAttrDefinition(SigStartValue);

            dbc.AttributeDefaults.Add(MsgSendTypeDefault);
            dbc.AttributeDefaults.Add(MsgCycleTimeDefault);
            dbc.AttributeDefaults.Add(SigStartValueDefault);

            return dbc;
        }

        public static ObjAttributeValue
        NewMsgCycleTime(int id, int time)
        {
            if (time == MsgCycleTimeDefault.Value.Num)
                return null;

            return new ObjAttributeValue
            {
                AttributeName = SendType_Cyclic,
                Type = Keyword.MESSAGES,
                MsgID = id,
                Value = new AttributeValue
                {
                    Num = time
                }
            };
        }

        public static ObjAttributeValue
        NewMsgSendType(int id, string type)
        {
            if (type == MsgCycleTimeDefault.Value.Val)
                return null;

            return new ObjAttributeValue
            {
                AttributeName = MsgCycleTime.AttributeName,
                Type = Keyword.MESSAGES,
                MsgID = id,
                Value = new AttributeValue
                {
                    Val = type
                }
            };
        }

    }
}
