using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace TalebookRebuilt.Models
{
    public class TaleBook
    {
        public ObservableCollection<TalePage> Pages { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Image Cover { get; set; }


    }
}
