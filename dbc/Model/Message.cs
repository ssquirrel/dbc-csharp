using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public class Message
    {
        public int MsgID { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string Transmitter { get; set; }
        public IList<Signal> Signals { get; set; }
    }
}
