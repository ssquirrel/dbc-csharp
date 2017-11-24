using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public class Signal
    {
        public const int Motorola = 0;
        public const int Intel = 1;
        public const string Signed = "";
        public const string Unsigned = "+";

        public string Name { get; set; }
        public bool IsMultiplexerSwitch { get; set; }
        public int MultiplexerSwitchValue { get; set; }
        public int StartBit { get; set; }
        public int SignalSize { get; set; }
        public int ByteOrder { get; set; } //0 Motorola, big endian; 1 Intel, little endian
        public string ValueType { get; set; } //"-" signed "+" unsigned
        public double Factor { get; set; }
        public double Offset { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Unit { get; set; }
        public IList<string> Receivers { get; set; }
    }
}
