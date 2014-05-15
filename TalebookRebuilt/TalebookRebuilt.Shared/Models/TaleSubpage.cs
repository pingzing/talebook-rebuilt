using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TalebookRebuilt.Models;

namespace TalebookRebuilt
{
    public class TaleSubpage
    {
        public string SubpageContent { get; set; }
        public int CharStartIndex { get; set; }
        public int CharEndIndex { get; set; }
        public TalePage ParentPage { get; private set; }
    }
}
