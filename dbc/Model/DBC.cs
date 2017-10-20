using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public class DBC
    {
        public string Version { get; set; } = "";
        public IList<string> NewSymbols { get; } = new List<string>();
        //BIT_TIMING
        public IList<string> Nodes { get; } = new List<string>();
        public IList<Message> Messages { get; } = new List<Message>();
        //MESSAGE_TRANSMITTERS
        //ENVIRONMENT_VARIABLES
        //ENVIRONMENT_VARIABLES_DATA
        //SIGNAL_TYPES
        public IList<Comment> Comments { get; } = new List<Comment>();

        public IList<AttributeDefinition> AttributeDefinitions { get; set; } = new List<AttributeDefinition>();

        //SIGTYPE_ATTR_LIST

        public IList<AttributeDefault> AttributeDefaults { get; set; } = new List<AttributeDefault>();

        public IList<ObjAttributeValue> AttributeValues { get; } =
            new List<ObjAttributeValue>();

        public IList<SignalValueDescription> ValueDescriptions { get; } =
            new List<SignalValueDescription>();

        //CATEGORY_DEFINITIONS
        //CATEGORIES
        //FILTER
        //SIGNAL_TYPE_REFS
        //SIGNAL_GROUPS
        //SIGNAL_EXTENDED_VALUE_TYPE_LIST
    }
}
