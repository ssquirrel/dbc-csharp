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
        public SemanticAnalysis(Model.DBC dbc)
        {
            var sendType = dbc.AttributeDefinitions.FirstOrDefault(ad =>
            {
                return ad.AttributeName == DbcStruct.Attr_MsgSendType;
            });

            if (sendType != null && sendType.Values != null)
            {
                var values = sendType.Values;

                MsgST_Cyclic = values.IndexOf(DbcStruct.MsgST_Cyclic_Key);
                MsgST_IfActive = values.IndexOf(DbcStruct.MsgST_IfActive_Key);
                MsgST_CyclicEvent = values.IndexOf(DbcStruct.MsgST_CyclicEvent_Key);
                MsgST_NoSendType = values.IndexOf(DbcStruct.MsgST_NoSendType_Key);
            }
        }

        public int MsgST_Cyclic { get; }
        public int MsgST_IfActive { get; }
        public int MsgST_CyclicEvent { get; }
        public int MsgST_NoSendType { get; }

        public int GetSendType(IAttributeValue val)
        {
            if (val.Type == AttrValType.Number)
                return (int)val.Num;

            int idx = DbcStruct.MSG_STs.IndexOf(val.Val);
            return idx >= 0 ? idx : int.MinValue;
        }
    }
}
