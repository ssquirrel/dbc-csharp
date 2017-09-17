using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class Signal
    {
        public string name;
        //public string multiplexerIndicator;
        public string startBit;
        public string signalSize;
        public string byteOrder; //"0" Motorola, big endian; "1" Intel, little endian
        public string valueType;
        public string factor;
        public string offset;
        public string min;
        public string max;
        public string unit;
        public List<string> receivers = new List<string>();
    }
}
