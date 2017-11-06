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

        public IList<ObjAttributeValue> SendTypes { get; } = new List<ObjAttributeValue>();

        public IList<ObjAttributeValue> CycleTime { get; } = new List<ObjAttributeValue>();

        public IList<SignalValueDescription> ValueDescriptions =>
            dbc.ValueDescriptions;

        public Model.DBC ToDBC()
        {
            foreach (var st in SendTypes)
                dbc.AttributeValues.Add(st);

            foreach (var ct in CycleTime)
                dbc.AttributeValues.Add(ct);

            /*
            int[] sendTypeCount = { 0, 0, 0, 0 };

            foreach (var av in SendTypes)
            {
                ++sendTypeCount[(int)av.Value.Num];
            }

            int winingST = FindMaxIndex(sendTypeCount);

            if (!(winingST == 3 && sendTypeCount[3] == dbc.Messages.Count))
            {
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
                        Num = winingST
                    }
                });

                foreach (var av in SendTypes)
                {
                    if (av.Value.Num == winingST)
                        continue;

                    dbc.AttributeValues.Add(av);
                }
            }
            */
            return dbc;
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
