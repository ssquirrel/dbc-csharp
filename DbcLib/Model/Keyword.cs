using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public static class Keyword
    {
        public const string VERSION = "VERSION";
        public const string NEW_SYMBOLS = "NS_";
        public const string BIT_TIMING = "BS_";
        public const string NODES = "BU_";
        public const string VALUE_TABLES = "VAL_TABLE_";
        public const string MESSAGES = "BO_";
        public const string SIGNAL = "SG_";
        public const string MESSAGE_TRANSMITTERS = "BO_TX_BU_";
        public const string ENVIRONMENT_VARIABLES = "EV_";
        public const string ENVIRONMENT_VARIABLES_DATA = "ENVVAR_DATA_";
        //public const string SIGNAL_TYPES = "";
        public const string COMMENTS = "CM_";
        public const string ATTRIBUTE_DEFINITIONS = "BA_DEF_";
        //public const string SIGTYPE_ATTR_LIST = "";
        public const string ATTRIBUTE_DEFAULTS = "BA_DEF_DEF_";
        public const string ATTRIBUTE_VALUES = "BA_";
        public const string VALUE_DESCRIPTIONS = "VAL_";
        //public const string CATEGORY_DEFINITIONS = "";
        //public const string CATEGORIES = "";
        //public const string FILTER = "";
        //public const string SIGNAL_TYPE_REFS = "";
        public const string SIGNAL_GROUPS = "SIG_GROUP_";
        //public const string SIGNAL_EXTENDED_VALUE_TYPE_LIST = "";

        public static bool IsKeyword(string str)
        {
            switch (str)
            {
                case VERSION:
                case NEW_SYMBOLS:
                case BIT_TIMING:
                case NODES:
                case VALUE_TABLES:
                case MESSAGES:
                case SIGNAL:
                case MESSAGE_TRANSMITTERS:
                case ENVIRONMENT_VARIABLES:
                case ENVIRONMENT_VARIABLES_DATA:
                //case SIGNAL_TYPES:
                case COMMENTS:
                case ATTRIBUTE_DEFINITIONS:
                //case SIGTYPE_ATTR_LIST:
                case ATTRIBUTE_DEFAULTS:
                case ATTRIBUTE_VALUES:
                case VALUE_DESCRIPTIONS:
                //case CATEGORY_DEFINITIONS:
                //case CATEGORIES:
                //case FILTER:
                //case SIGNAL_TYPE_REFS:
                case SIGNAL_GROUPS:
                    return true;
            }

            return false;
        }
    }
}
