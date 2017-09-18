using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Model
{
    public class SignalValueDescription
    {
        public string MsgID { get; set; }
        public string Name { get; set; }
        public IList<ValueDesc> Descs { get; set; }
    }
}
