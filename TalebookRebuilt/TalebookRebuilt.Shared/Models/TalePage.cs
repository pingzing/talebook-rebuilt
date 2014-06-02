using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace TalebookRebuilt.Models
{
    public class TalePage
    {
        public string PageContent { get; set; }
        public Image PageImage { get; set; }
        public Color PageColor { get; set; }
        public List<TaleSubpage> Subpages {get; set;}

        //Idea: Create a tale page such that it holds the maximum amount of text possible for the given device/resolution.
        //Break pages up based on images. 
        //Have a collection of subpages that are a subset of a given page's PageContent. 
        //Have the ViewModel's CurrentPage map to a subpage.
        public TalePage(string pageContent, Image pageImage, Color pageColor)
        {
            PageContent = pageContent;
            PageImage = pageImage;
            PageColor = pageColor;
            Subpages = new List<TaleSubpage>();
        }

        public TalePage()
        {
            PageContent = "I'm a test page!";
            PageImage = null;
            PageColor = Colors.Black;
            Subpages = new List<TaleSubpage>();
        }
    }
}
