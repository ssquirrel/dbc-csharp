using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class Signal
    {
        public string Name { get; set; }
        //multiplexerIndicator
        public string StartBit { get; set; }
        public string SignalSize { get; set; }
        public string ByteOrder { get; set; } //"0" Motorola, big endian{get;set;} "1" Intel, little endian
        public string ValueType { get; set; }
        public string Factor { get; set; }
        public string Offset { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public string Unit { get; set; }
        public IList<string> Receivers { get; set; }
    }
}
