using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model.PropTree
{
    public class DefaultAttributes : IEnumerable<AttributeDefault>
    {
        private IDictionary<string, AttributeDefault> dict =
            new Dictionary<string, AttributeDefault>();

        public DefaultAttributes(IEnumerable<AttributeDefault> defaults)
        {
            dict = defaults.ToDictionary(def => def.AttributeName);
        }

        public IAttributeValue this[string name]
        {
            get
            {
                if (dict.TryGetValue(name, out var def))
                    return def.Value;

                return PropTree.EmptyAttributeValue;
            }
        }

        public IEnumerator<AttributeDefault> GetEnumerator()
        {
            return dict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
