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

        public SemanticAnalysis(Model.DBC dbc)
        {
            var def = dbc.AttributeDefinitions.FirstOrDefault(ad =>
            {
                return ad.AttributeName == MsgSendType.AttributeName;
            });

            sendTypes = def.Values;
        }

        public MsgSendTypeEnum GetSendType(IAttributeValue av)
        {
            if(av.Type == AttrValType.Number)
            {
                var type = sendTypes[(int)av.Num];

                return MsgSendType.ToEnum(type);
            }

            return MsgSendType.ToEnum(av.Val);
        }
    }
}
