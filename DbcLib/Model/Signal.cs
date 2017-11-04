using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public class Signal
    {
        public string Name { get; set; }
        //multiplexerIndicator
        public int StartBit { get; set; }
        public int SignalSize { get; set; }
        public string ByteOrder { get; set; } //"0" Motorola, big endian{get;set;} "1" Intel, little endian
        public string ValueType { get; set; }
        public double Factor { get; set; }
        public double Offset { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Unit { get; set; }
        public IList<string> Receivers { get; set; }
    }
}
