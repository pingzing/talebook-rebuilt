﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TalebookRebuilt.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TalebookRebuilt
{
    public class TaleSubpage
    {
        public FrameworkElement SubpageContent { get; set; }                
        public TalePage ParentPage { get; private set; }

        public TaleSubpage(FrameworkElement subpageContent, TalePage parentPage)
        {
            this.SubpageContent = subpageContent;                        
            this.ParentPage = parentPage;
        }
    }
}
