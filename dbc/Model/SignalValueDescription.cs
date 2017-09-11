using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    class SignalValueDescription
    {
        public string messageId;
        public string name;
        public List<ValueDescription> descriptions = new List<ValueDescription>();
    }
}
