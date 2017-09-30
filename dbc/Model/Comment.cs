﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.DBC.Lex;

namespace DbcLib.Model
{
    public class Comment
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public int MsgID { get; set; }
        public string Val { get; set; }
    }
}
