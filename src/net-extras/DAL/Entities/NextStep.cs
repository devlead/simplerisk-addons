﻿using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public partial class NextStep
    {
        public int Value { get; set; }
        public string Name { get; set; } = null!;
    }
}
