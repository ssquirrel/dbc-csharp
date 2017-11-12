using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;

namespace DbcLib.Excel.Parser
{
    class DbcBuilder
    {
        private HashSet<string> defs = new HashSet<string>();

        public Model.DBC DBC { get; } = new Model.DBC();

        public IList<Message> Messages => DBC.Messages;

        public IList<SignalValueDescription> ValueDescriptions =>
            DBC.ValueDescriptions;

        public void NewMsgComment(long id, string cm)
        {
            if (cm.Length == 0)
                return;

            DBC.Comments.Add(new Comment
            {
                Type = Keyword.MESSAGES,
                MsgID = id,
                Val = cm
            });
        }

        public void NewSigComment(long id, string name, string cm)
        {
            if (cm.Length == 0)
                return;

            DBC.Comments.Add(new Comment
            {
                Type = Keyword.SIGNAL,
                MsgID = id,
                Name = name,
                Val = cm
            });
        }

        public void NewMsgSendType(long id, MsgSendTypeEnum type)
        {
            if (type == MsgSendTypeEnum.NoMsgSendType)
                return;

            if (defs.Add(MsgSendType.AttributeName))
            {
                DBC.AttributeDefinitions.Add(MsgSendType.Definition());
                DBC.AttributeDefaults.Add(MsgSendType.Default());
            }

            DBC.AttributeValues.Add(new ObjAttributeValue
            {
                AttributeName = MsgSendType.AttributeName,
                ObjType = Keyword.MESSAGES,
                MsgID = id,
                Value = new AttributeValue
                {
                    Num = (int)type
                }
            });
        }

        public void NewCycleTime(long id, MsgSendTypeEnum type, int time)
        {
            if (type != MsgSendTypeEnum.Cyclic)
                return;

            if (defs.Add(MsgCycleTime.AttributeName))
            {
                DBC.AttributeDefinitions.Add(MsgCycleTime.Definition());
                DBC.AttributeDefaults.Add(MsgCycleTime.Default());
            }

            DBC.AttributeValues.Add(new ObjAttributeValue
            {
                AttributeName = MsgCycleTime.AttributeName,
                ObjType = Keyword.MESSAGES,
                MsgID = id,
                Value = new AttributeValue
                {
                    Num = time
                }
            });
        }

        /*
        public static ObjAttributeValue
        NewSigStartValue(long id, string name, int num)
        {
            return new ObjAttributeValue
            {
                AttributeName = Attr_SigStartValue,
                ObjType = Keyword.SIGNAL,
                MsgID = id,
                Name = name,
                Value = new AttributeValue
                {
                    Num = num
                }
            };
        }
        */
    }

}
