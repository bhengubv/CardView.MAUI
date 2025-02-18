﻿using System.Collections.Generic;
using PanCardView.Enums;
using Microsoft.Maui;

namespace PanCardView.Utility
{
    public struct ProcessorItem
    {
        public bool IsFront { get; set; }
        public AnimationDirection Direction { get; set; }
        public IEnumerable<View> Views { get; set; }
        public IEnumerable<View> InactiveViews { get; set; }
    }
}
