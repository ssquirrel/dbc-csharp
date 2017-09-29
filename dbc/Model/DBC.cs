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
        private IDictionary<string, AttributeDefinition> attributeDefinitions =
            new Dictionary<string, AttributeDefinition>();

        private IDictionary<string, AttributeDefault> attributeDefaults =
            new Dictionary<string, AttributeDefault>();

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

        public IEnumerable<AttributeDefinition>
        AttributeDefinitions => attributeDefinitions.Values;

        //SIGTYPE_ATTR_LIST

        public IEnumerable<AttributeDefault>
        AttributeDefaults => attributeDefaults.Values;

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

        public void AddAttrDefinition(AttributeDefinition def)
        {
            attributeDefinitions.Add(def.AttributeName, def);

        }

        public AttributeDefinition GetAttrDefinition(string name)
        {
            if (attributeDefinitions.TryGetValue(name,
                out AttributeDefinition def))
                return def;

            return null;
        }

        public void AddAttrDefault(AttributeDefault ad)
        {
            attributeDefaults.Add(ad.AttributeName, ad);

        }

        public AttributeDefault GetAttrDefault(string name)
        {
            if (attributeDefaults.TryGetValue(name,
                out AttributeDefault ad))
                return ad;

            return null;
        }

    }
}
