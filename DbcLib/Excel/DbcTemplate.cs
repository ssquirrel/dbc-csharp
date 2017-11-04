using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.DBC.Parser;
using DbcLib.Model;

namespace DbcLib.Excel
{
    static class DbcTemplate
    {
        public static readonly string Attr_MsgSendType = "GenMsgSendType";
        public static readonly string Attr_MsgCycleTime = "GenMsgCycleTime";

        public static readonly int MsgSendType_Cyclic = 0;
        public static readonly int MsgSendType_IfActive = 7;
        public static readonly int MsgSendType_CyclicEvent = 1;

        public static readonly int MsgSendTypeDefault = 0;
        public static readonly int MsgCycleTimeDefault = 100;

        public static Model.DBC LoadTemplate(string fn)
        {
            return DbcParser.Parse(fn);
        }
    }
}
