﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.DBC.Lex;

namespace DbcLib.DBC.Model
{
    public class Comment
    {
        public string Str { get; set; }
        public string Type { get; set; } = "";
        public string MsgID { get; set; }
        public string Name { get; set; }
    }
}
