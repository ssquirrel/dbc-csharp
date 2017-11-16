using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;
using DbcLib.Model.PropTree;

namespace DbcLib.Excel.Writer
{
    class SemanticAnalysis
    {
        private IList<string> sendTypes;

        public SemanticAnalysis(Tree tree)
        {
            var def = tree.DBC.AttributeDefinitions.FirstOrDefault(ad =>
            {
                return ad.AttributeName == MsgSendType.AttributeName;
            });

            sendTypes = def?.Values;


            if (tree.Def.TryGetValue(MsgSendType.AttributeName, out var st))
            {
                MsgCycleTimeDefault = st.Value;
            }

            if (tree.Def.TryGetValue(MsgCycleTime.AttributeName, out var ct))
            {
                MsgCycleTimeDefault = ct.Value;
            }

            if (tree.Def.TryGetValue(SigStartValue.AttributeName, out var sv))
            {
                SigStartValDefault = sv.Value;
            }
        }

        public AttributeValue MsgSendTypeDefault { get; }
        public AttributeValue MsgCycleTimeDefault { get; }
        public AttributeValue SigStartValDefault { get; }

        public MsgSendTypeEnum GetSendType(AttributeValue av)
        {
            if (sendTypes == null || av == null)
            {
                return MsgSendTypeEnum.NoMsgSendType;
            }

            if (av.Type == AttrValType.Number)
            {
                var type = sendTypes[(int)av.Num];
                return MsgSendType.ToEnum(type);
            }

            return MsgSendType.ToEnum(av.Val);
        }
    }
}
