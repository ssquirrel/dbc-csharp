using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class DBC
    {
        public string Version { get; set; } = "";
        public List<string> NewSymbols { get; } = new List<string>();
        //BIT_TIMING
        public List<string> Nodes { get; } = new List<string>();
        public List<Message> Messages { get; } = new List<Message>();
        //MESSAGE_TRANSMITTERS
        //ENVIRONMENT_VARIABLES
        //ENVIRONMENT_VARIABLES_DATA
        //SIGNAL_TYPES
        public List<Comment> Comments { get; } = new List<Comment>();

        public List<AttributeDefinition> AttributeDefinitions { get; } =
            new List<AttributeDefinition>();

        //SIGTYPE_ATTR_LIST

        public List<AttributeDefault> AttributeDefaults { get; } =
            new List<AttributeDefault>();

        public List<ObjAttributeValue> AttributeValues { get; } =
            new List<ObjAttributeValue>();

        public List<SignalValueDescription> ValueDescriptions { get; } =
            new List<SignalValueDescription>();


        //CATEGORY_DEFINITIONS
        //CATEGORIES
        //FILTER
        //SIGNAL_TYPE_REFS
        //SIGNAL_GROUPS
        //SIGNAL_EXTENDED_VALUE_TYPE_LIST
    }
}
