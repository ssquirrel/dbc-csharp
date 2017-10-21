using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DbcLib.Model
{
    public class ValueTable
    {
        public string Name { get; set; }
        public IList<ValueDesc> Descs { get; set; }
    }
}
