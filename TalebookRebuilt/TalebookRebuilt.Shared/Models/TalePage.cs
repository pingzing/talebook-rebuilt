using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace TalebookRebuilt.Models
{
    public class TalePage
    {
        [JsonIgnore]
        public string PageContent { get; set; }
        
        [JsonIgnore]
        public BitmapImage PageImage { get; set; }       
        public Color PageColor { get; set; }
        public int PageNumber { get; set; }

        public TalePage(string pageContent, BitmapImage pageImage, Color pageColor)
        {
            PageContent = pageContent;
            PageImage = pageImage;
            PageColor = pageColor;            
        }

        public TalePage(string pageContent, BitmapImage pageImage, Color pageColor, int pageNumber)
        {
            PageContent = pageContent;
            PageImage = pageImage;
            PageColor = pageColor;
            PageNumber = pageNumber;            
        }

        public TalePage()
        {
            PageContent = "I'm a test page!";
            PageImage = null;
            PageColor = Colors.Black;            
        }
    }
}
