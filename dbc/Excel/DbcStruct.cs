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
    static class DbcStruct
    {
        public static string Attr_MsgSendType => "GenMsgSendType";
        public static string Attr_MsgCycleTime => "GenMsgCycleTime";

        public static IList<string> MSG_STs = new List<string> {
            "Cyclic",
            "IfActive",
            "CyclicEvent",
            "NoMsgSendType"
        };

        public static int MsgST_Cyclic => 0;
        public static int MsgST_IfActive => 1;
        public static int MsgST_CyclicEvent => 2;
        public static int MsgST_NoSendType => 3;

        public static string MsgST_Cyclic_Key => MSG_STs[0];
        public static string MsgST_IfActive_Key => MSG_STs[1];
        public static string MsgST_CyclicEvent_Key => MSG_STs[2];
        public static string MsgST_NoSendType_Key => MSG_STs[3];
    }
}
