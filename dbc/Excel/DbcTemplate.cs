using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.DBC.Parser;
using DbcLib.Model;

namespace DbcLib.Excel
{
    class DbcTemplate
    {
        public static readonly string Attr_MsgSendType = "GenMsgSendType";
        public static readonly string Attr_MsgCycleTime = "GenMsgCycleTime";

        public static readonly int MsgSendType_Cyclic = 0;
        public static readonly int MsgSendType_IfActive = 7;


        private int msgSendTypeDefault = -1;
        private int msgCycleTimeDefault = -1;

        public DbcTemplate(string template)
        {
            DBC = DbcParser.Parse(template);

            AttributeDefault st = DBC.GetAttrDefault(Attr_MsgSendType);
            if (st != null)
            {
                msgSendTypeDefault = (int)st.Value.Num;
            }


            AttributeDefault ct = DBC.GetAttrDefault(Attr_MsgCycleTime);
            if (ct != null)
            {
                msgCycleTimeDefault = (int)ct.Value.Num;
            }

        }

        public void SetMsgSendType(int id, int type)
        {
            if (type == msgSendTypeDefault)
                return;

            DBC.AttributeValues.Add(new ObjAttributeValue
            {
                AttributeName = Attr_MsgSendType,
                Type = Keyword.MESSAGES,
                MsgID = id,
                Value = new AttributeValue
                {
                    Num = type
                }
            });
        }

        public void SetMsgCycleTime(int id, int time)
        {
            if (time < 0 || time == msgCycleTimeDefault)
                return;

            DBC.AttributeValues.Add(new ObjAttributeValue
            {
                AttributeName = Attr_MsgCycleTime,
                Type = Keyword.MESSAGES,
                MsgID = id,
                Value = new AttributeValue
                {
                    Num = time
                }
            });
        }

        public Model.DBC DBC { get; }
    }
}
