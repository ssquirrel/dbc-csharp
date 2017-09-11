using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    class DBC
    {
        public string version = null;
        public List<string> newSymbols = new List<string>();
        //BIT_TIMING
        public List<string> nodes = new List<string>();
        public List<ValueTable> valueTables = new List<ValueTable>();
        public List<Message> messages = new List<Message>();
        //MESSAGE_TRANSMITTERS
        //ENVIRONMENT_VARIABLES
        //ENVIRONMENT_VARIABLES_DATA
        //SIGNAL_TYPES
        public List<Comment> comments = new List<Comment>();
        public List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();
        //SIGTYPE_ATTR_LIST
        public List<AttributeDefault> attributeDefaults = new List<AttributeDefault>();
        public List<AttributeValue> attributeValues = new List<AttributeValue>();
        public List<SignalValueDescription> valueDescriptions = new List<SignalValueDescription>();
        //CATEGORY_DEFINITIONS
        //CATEGORIES
        //FILTER
        //SIGNAL_TYPE_REFS
        //SIGNAL_GROUPS
        //SIGNAL_EXTENDED_VALUE_TYPE_LIST
    }
}
