using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class Message
    {
        public string id;
        public string name;
        public string size;
        public string transmitter;
        public List<Signal> signals = new List<Signal>();
    }
}
