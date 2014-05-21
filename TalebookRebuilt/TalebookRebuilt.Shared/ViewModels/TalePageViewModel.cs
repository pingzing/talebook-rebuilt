using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using TalebookRebuilt.Helpers;
using TalebookRebuilt.Models;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace TalebookRebuilt.ViewModels
{
    public class TalePageViewModel : INotifyPropertyChanged
    {
        private const int HEADER_HEIGHT = 120;

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

        private ObservablePages currentPage;
        public ObservablePages CurrentPage
        {
            get { return currentPage; }
            set
            {
                currentPage = value;
                NotifyPropertyChanged("CurrentPage");
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

        private double textboxMaxHeight;
        public double TextboxMaxHeight
        {
            get { return textboxMaxHeight; }
            set
            {
                textboxMaxHeight = value;
                NotifyPropertyChanged("TextboxMaxHeight");
            }
        }

        private double textboxMaxWidth;
        public double TextboxMaxWidth
        {
            get { return textboxMaxWidth; }
            set
            {
                this.textboxMaxWidth = value;
                NotifyPropertyChanged("TextboxMaxWidth");
            }
        }

        private DelegateCommand<ObservablePages> updateSizeCommand;
        public DelegateCommand<ObservablePages> UpdateSizeCommand
        {
            get { return updateSizeCommand; }
            set { updateSizeCommand = value; }
        }
        private void OnUpdateSizeCommand(ObservablePages itemSource)
        {
            if (itemSource.Count > 1 && ((RichTextBlock)itemSource[itemSource.Count - 1]).HasOverflowContent)
            {
                //add more overflow boxes
            }
            RichTextBlock mainBlock = itemSource[0] as RichTextBlock;

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
            Window.Current.SizeChanged += Current_SizeChanged;
            InitializeControls();
            BuildPages();
        }

        private void InitializeControls()
        {
            //Prepare sizes for our poor, silly StackPanel
            var size = Window.Current.Bounds;
            double heightMargin = 40.00;
            double widthMargin = 10.00;
            this.TextboxMaxHeight = size.Height - HEADER_HEIGHT - heightMargin;
            this.TextboxMaxWidth = size.Width / 2 - widthMargin;

            //Define commands
            this.updateSizeCommand = new DelegateCommand<ObservablePages>(this.OnUpdateSizeCommand);

            //Initialize CurrentPage
            this.CurrentPage = new ObservablePages();

            //Debug
            this.WindowBoundsString = "MaxHeight: " + this.TextboxMaxHeight + " MaxWidth: " + this.TextboxMaxWidth;
        }

        void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var size = e.Size;
            double heightMargin = 40.00;
            double widthMargin = 10.00;
            this.TextboxMaxHeight = size.Height - HEADER_HEIGHT - heightMargin;
            this.TextboxMaxWidth = size.Width / 2 - widthMargin;

#if DEBUG
            this.WindowBoundsString = "MaxHeight: " + this.TextboxMaxHeight + " MaxWidth: " + this.TextboxMaxWidth;
#endif

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
            BuildPagesNew();

            //RichTextBlock pageOne = new RichTextBlock();
            //pageOne.Width = double.NaN;
            //pageOne.Height = double.NaN;
            //pageOne.MaxWidth = this.TextboxMaxWidth;
            //pageOne.MaxHeight = this.TextboxMaxHeight;
            //pageOne.HorizontalAlignment = HorizontalAlignment.Left;
            //pageOne.VerticalAlignment = VerticalAlignment.Top;
            //pageOne.IsDoubleTapEnabled = false;
            //pageOne.IsHitTestVisible = false;
            //pageOne.IsHoldingEnabled = false;
            //pageOne.IsTextSelectionEnabled = false;
            //pageOne.IsTapEnabled = false;
            //pageOne.SetValue(Helpers.Properties.HtmlProperty, CurrentBook.Pages[0].PageContent);

            //pageOne.Measure(new Size(this.TextboxMaxWidth, this.TextboxMaxHeight));
            //CurrentPage.Add(pageOne);
            //CurrentPage = CurrentBook.Pages[0];
            //End test code
        }

        //TODO: Add the RichTextBoxOverflows to the visual tree
        private void BuildPagesNew()
        {
            CurrentPage.Clear();            
            RichTextBlockOverflow lastOverflow;
            lastOverflow = AddOnePage(null);
            
            while(lastOverflow.HasOverflowContent)
            {
                lastOverflow = AddOnePage(lastOverflow);
            }
        }

        private RichTextBlockOverflow AddOnePage(RichTextBlockOverflow lastOverflow)
        {
            bool isFirstPage = lastOverflow == null;
            RichTextBlockOverflow rtbo = new RichTextBlockOverflow();

            if (isFirstPage)
            {
                RichTextBlock pageOne = new RichTextBlock();
                pageOne.Width = double.NaN;
                pageOne.Height = double.NaN;
                pageOne.MaxWidth = this.TextboxMaxWidth;
                pageOne.MaxHeight = this.TextboxMaxHeight;
                pageOne.HorizontalAlignment = HorizontalAlignment.Left;
                pageOne.VerticalAlignment = VerticalAlignment.Top;
                pageOne.IsDoubleTapEnabled = false;
                pageOne.IsHitTestVisible = false;
                pageOne.IsHoldingEnabled = false;
                pageOne.IsTextSelectionEnabled = false;
                pageOne.IsTapEnabled = false;
                pageOne.SetValue(Helpers.Properties.HtmlProperty, CurrentBook.Pages[0].PageContent);
                pageOne.SetBinding(RichTextBlock.MaxWidthProperty, new Binding
                    {
                        Source = TextboxMaxWidth,
                        Path = new PropertyPath("MaxWidth")
                    });
                pageOne.SetBinding(RichTextBlock.MaxHeightProperty, new Binding
                    {
                        Source = TextboxMaxHeight,
                        Path = new PropertyPath("MaxHeight")
                    });

                pageOne.Measure(new Size(this.TextboxMaxWidth, this.TextboxMaxHeight));
                CurrentPage.Add(pageOne);
                if (pageOne.HasOverflowContent)
                {
                    pageOne.OverflowContentTarget = rtbo;
                    //set width and height here?
                    rtbo.Measure(new Size(this.TextboxMaxWidth, this.TextboxMaxHeight));
                }
            }
            else
            {
                //set rtbo width and height here?
                //Maybe set maxheight and maxwidth bindings too
                if (lastOverflow.HasOverflowContent)
                {
                    lastOverflow.OverflowContentTarget = rtbo;
                    lastOverflow.Measure(new Size(this.TextboxMaxWidth, this.TextboxMaxHeight));
                    rtbo.Measure((new Size(this.TextboxMaxWidth, this.TextboxMaxHeight)));
                }
                this.CurrentPage.Add(rtbo);
            }
            return rtbo;
        }

    }
}
