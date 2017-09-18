using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class Message
    {
        public string MsgID { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Transmitter { get; set; }
        public IList<Signal> Signals { get; set; }
    }
}
