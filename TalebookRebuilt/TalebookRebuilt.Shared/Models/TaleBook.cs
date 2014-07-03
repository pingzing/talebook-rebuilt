using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace TalebookRebuilt.Models
{
    public class TaleBook
    {
        public string Title { get; set; }
        public string Description { get; set; }        
        [JsonIgnore]
        public BitmapImage Cover { get; set; }               
        public List<TalePage> Pages { get; set; }       
    }

   
}
