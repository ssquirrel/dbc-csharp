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
        private Model.DBC dbc = new Model.DBC();

        public IList<Message> Messages => dbc.Messages;

        public IList<Comment> Comments => dbc.Comments;

        public IList<int> SendTypes { get; } = new List<int>();

        public IList<int> CycleTime { get; } = new List<int>();

        public IList<SignalValueDescription> ValueDescriptions =>
            dbc.ValueDescriptions;

        public Model.DBC ToDBC()
        {
            ReduceSendTypes();
            ReduceCycleTime();

            return dbc;
        }

        private void ReduceSendTypes()
        {
            int[] sendTypeCount = { 0, 0, 0, 0 };

            foreach (var st in SendTypes)
            {
                ++sendTypeCount[st];
            }

            int winner = FindMaxIndex(sendTypeCount);

            if (winner == 3 &&
                sendTypeCount[winner] == SendTypes.Count)
                return;

            dbc.AttributeDefinitions.Add(new AttributeDefinition
            {
                AttributeName = DbcStruct.Attr_MsgSendType,
                ObjectType = Keyword.MESSAGES,
                ValueType = "ENUM",
                Values = DbcStruct.MSG_STs
            });

            dbc.AttributeDefaults.Add(new AttributeDefault
            {
                AttributeName = DbcStruct.Attr_MsgSendType,
                Value = new AttributeValue
                {
                    Val = DbcStruct.MSG_STs[winner]
                }
            });

            for (int i = 0; i < SendTypes.Count; ++i)
            {
                if (SendTypes[i] == winner)
                    continue;

                dbc.AttributeValues.Add(new ObjAttributeValue
                {
                    AttributeName = DbcStruct.Attr_MsgCycleTime,
                    ObjType = Keyword.MESSAGES,
                    MsgID = dbc.Messages[i].MsgID,
                    Value = new AttributeValue
                    {
                        Num = SendTypes[i]
                    }
                });
            }
        }

        private void ReduceCycleTime()
        {
            Dictionary<int, int> count = new Dictionary<int, int>();

            foreach (var t in CycleTime)
            {
                if (count.ContainsKey(t))
                {
                    ++count[t];
                }
                else
                {
                    count[t] = 1;
                }
            }

            int winner = 0;
            int maxCount = 0;

            foreach (var pair in count)
            {
                if (pair.Value > maxCount)
                {
                    winner = pair.Key;
                    maxCount = pair.Value;
                }
            }

            if (winner == 0)
                return;

            dbc.AttributeDefinitions.Add(new AttributeDefinition
            {
                AttributeName = DbcStruct.Attr_MsgCycleTime,
                ObjectType = Keyword.MESSAGES,
                ValueType = "INT",
                Num1 = 0,
                Num2 = maxCount
            });

            dbc.AttributeDefaults.Add(new AttributeDefault
            {
                AttributeName = DbcStruct.Attr_MsgCycleTime,
                Value = new AttributeValue
                {
                    Num = winner
                }
            });

            for (int i = 0; i < CycleTime.Count; ++i)
            {
                int t = CycleTime[i];

                if (t == 0 || t == winner)
                    continue;

                dbc.AttributeValues.Add(new ObjAttributeValue
                {
                    AttributeName = DbcStruct.Attr_MsgCycleTime,
                    ObjType = Keyword.MESSAGES,
                    MsgID = dbc.Messages[i].MsgID,
                    Value = new AttributeValue
                    {
                        Num = t
                    }
                });
            }
        }

        private static int FindMaxIndex(int[] a)
        {
            int idx = 0;
            int max = a[0];

            for (int i = 1; i < a.Length; ++i)
            {
                if (a[i] > max)
                {
                    idx = i;
                    max = a[i];
                }
            }

            return idx;
        }
    }
}
