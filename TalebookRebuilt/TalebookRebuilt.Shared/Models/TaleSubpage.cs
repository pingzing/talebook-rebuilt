using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TalebookRebuilt.Models;
using Windows.UI.Xaml.Controls;

namespace TalebookRebuilt
{
    public class TaleSubpage
    {
        public RichTextBlockOverflow SubpageContent { get; set; }
        public int CharStartIndex { get; set; }
        public int CharEndIndex { get; set; }
        public TalePage ParentPage { get; private set; }

        public TaleSubpage(RichTextBlockOverflow subpageContent, int charStartIndex, int charEndIndex, TalePage parentPage)
        {
            this.SubpageContent = subpageContent;
            this.CharStartIndex = charStartIndex;
            this.CharEndIndex = charEndIndex;
            this.ParentPage = parentPage;
        }
    }
}
