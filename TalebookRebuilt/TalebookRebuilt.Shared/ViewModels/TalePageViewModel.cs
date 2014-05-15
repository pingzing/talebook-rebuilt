using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using TalebookRebuilt.Helpers;
using TalebookRebuilt.Models;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TalebookRebuilt.ViewModels
{
    public class TalePageViewModel : INotifyPropertyChanged
    {
        private TaleBook currentBook;
        public TaleBook CurrentBook
        {
            get { return currentBook; }
            set
            {
                currentBook = value;
                NotifyPropertyChanged("CurrentBook");
            }
        }

        private TalePage currentPage;
        public TalePage CurrentPage
        {
            get { return currentPage; }
            set
            {
                currentPage = value;
                NotifyPropertyChanged("CurrentPage");
            }
        }

        private TaleSubpage currentSubpage;
        public TaleSubpage CurrentSubpage
        {
            get { return currentSubpage; }
            set
            {
                currentSubpage = value;
                NotifyPropertyChanged("CurrentSubpage");
            }
        }

        private string windowBoundsString;
        public string WindowBoundsString
        {
            get { return windowBoundsString; }
            set
            {
                windowBoundsString = value;
                NotifyPropertyChanged("WindowBoundsString");
            }
        }

        private int textboxHeight;
        public int TextboxHeight
        {
            get { return textboxHeight; }
            set
            {
                textboxHeight = value;
                NotifyPropertyChanged("TextboxHeight");
            }
        }

        private int textboxWidth;
        public int TextboxWidth
        {
            get { return textboxWidth; }
            set
            {
                this.textboxWidth = value;
                NotifyPropertyChanged("TextboxWidth");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public TalePageViewModel()
        {
            BuildPages();
            this.WindowBoundsString = "Height: " + Window.Current.Bounds.Height + " Width: " + Window.Current.Bounds.Width;
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var size = e.Size;
            this.WindowBoundsString = "Height: " + size.Height + " Width: " + size.Width;
        }

        async private void BuildPages()
        {
            //Test code. This method probably belongs somewhere in TaleBook.cs
            string filePath = "Snowbird.html";
            string resultString = await BookBuilder.HtmlToString(filePath);

            currentBook = new TaleBook();
            currentBook.Pages = new System.Collections.ObjectModel.ObservableCollection<TalePage>();
            currentBook.Pages.Add(new TalePage(resultString, null, Colors.Black));

            currentBook.Cover = new Image();
            currentBook.Description = "Test description";
            currentBook.Title = "Test Title";
            CurrentBook = currentBook;

            CurrentPage = CurrentBook.Pages[0];
            //End test code
        }

    }
}
