﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Model
{
    public class AttributeDefault : AttributeValue, PropTree.IAttribute
    {
        public string AttributeName { get; set; }
    }
}
