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
using Windows.UI.Xaml.Media;

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

        private ObservableCollection<FrameworkElement> drawnPages;
        public ObservableCollection<FrameworkElement> DrawnPages
        {
            get { return drawnPages; }
            set
            {
                drawnPages = value;
                NotifyPropertyChanged("CurrentPage");
            }
        }

        private int currentPageNum;
        public int CurrentPageNum
        {
            get { return currentPageNum; }
            set
            {
                currentPageNum = value;
                NotifyPropertyChanged("CurrentPageNum");
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

        private DelegateCommand<object> updateSizeCommand;
        public DelegateCommand<object> UpdateSizeCommand
        {
            get { return updateSizeCommand; }
            set
            {
                updateSizeCommand = value;
                NotifyPropertyChanged("updateSizeCommand");
            }
        }
        private void OnUpdateSizeCommand(object itemSource)
        {
            //Get current character-location
            FlipView flip = itemSource as FlipView;
            int locToRestore = 0;
            int pageToRestore = 0;
            if (flip != null)
            {
                var currRtb = flip.SelectedItem;
                if (currRtb is RichTextBlock)
                {
                    locToRestore = ((RichTextBlock)currRtb).ContentStart.Offset;
                }
                else if (currRtb is RichTextBlockOverflow)
                {
                    locToRestore = ((RichTextBlockOverflow)currRtb).ContentStart.Offset;
                }
            }

            DrawPages();
            if (flip != null)
            {
                flip.UpdateLayout();
                var dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
                //Force this to run on the UI thread, so we KNOW it'll wait until the text is 
                //properly distributed to all the RTBOverflows
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        RestoreReadingLocation(flip, locToRestore);
                    });
            }
        }

        //If we have the old character-location, restore the reader's current page
        //Search through currentPage's elements until we find the correct offset. 
        //set flip.SelectedIndex to that element's index
        private void RestoreReadingLocation(FlipView flip, int locToRestore)
        {
            int restoreIndex = -1;
            foreach (var subpage in DrawnPages)
            {
                if (subpage is RichTextBlock)
                {
                    if (((RichTextBlock)subpage).ContentEnd.Offset >= locToRestore
                        && !(((RichTextBlock)subpage).ContentStart.Offset > locToRestore))
                    {
                        restoreIndex = DrawnPages.IndexOf(subpage);
                        break;
                    }
                }
                else if (subpage is RichTextBlockOverflow)
                {
                    if (((RichTextBlockOverflow)subpage).ContentEnd.Offset >= locToRestore
                        && !(((RichTextBlockOverflow)subpage).ContentStart.Offset > locToRestore))
                    {
                        restoreIndex = DrawnPages.IndexOf(subpage);
                        break;
                    }
                }
            }
            if (restoreIndex != -1)
            {
                flip.SelectedIndex = restoreIndex;
            }
        }

        private DelegateCommand<object> pageFlippedCommand;
        public DelegateCommand<object> PageFlippedCommand
        {
            get { return pageFlippedCommand; }
            set 
            { 
                pageFlippedCommand = value;
                NotifyPropertyChanged("PageFlippedCommand");
            }
        }

        private void OnPageFlippedCommand(object obj)
        {
            throw new NotImplementedException();
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
            var size = Window.Current.Bounds;
            double heightMargin = 40.00;
            double widthMargin = 10.00;
            this.TextboxMaxHeight = size.Height - HEADER_HEIGHT - heightMargin;
            this.TextboxMaxWidth = size.Width / 2 - widthMargin;

            //Define commands
            this.updateSizeCommand = new DelegateCommand<object>(this.OnUpdateSizeCommand);
            this.pageFlippedCommand = new DelegateCommand<object>(this.OnPageFlippedCommand);

            //Initialize CurrentPage
            this.DrawnPages = new ObservableCollection<FrameworkElement>();

            //Debug
            this.WindowBoundsString = "MaxHeight: " + this.TextboxMaxHeight + " MaxWidth: " + this.TextboxMaxWidth;
        }

        //Used to maintain TextboxMaxHeight and TextboxMaxWidth
        void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var size = e.Size;
#if WINDOWS_APP
            double heightMargin = 40.00;
            double widthMargin = 10.00;
#elif WINDOWS_PHONE_APP
            double heightMargin = 0.00;
            double widthMargin = 0.00;
#endif
            this.TextboxMaxHeight = size.Height - HEADER_HEIGHT - heightMargin;
            this.TextboxMaxWidth = size.Width / 2 - widthMargin;

#if DEBUG
            this.WindowBoundsString = "MaxHeight: " + this.TextboxMaxHeight + " MaxWidth: " + this.TextboxMaxWidth;
#endif

        }

        async private void BuildPages()
        {
            //Test code. This method probably belongs somewhere in TaleBook.cs
            string filePath = "Snowbird\\Snowbird.html";
            string rawHtml = await BookBuilder.HtmlToString(filePath);
            List<string> slicedPages = BookBuilder.GetSlicedPages(rawHtml);

            currentBook = new TaleBook();
            currentBook.Pages = new List<TalePage>();
            //TODO: Generate/have an XML file that defines colors/images for each page?
            int pageNum = 0;
            foreach (var page in slicedPages)
            {
                currentBook.Pages.Add(new TalePage(page, null, Colors.Black, pageNum));
                pageNum++;
            }

            currentBook.Cover = new Image();
            currentBook.Description = "Test description";
            currentBook.Title = "Test Title";
            CurrentBook = currentBook;
            DrawPages();
            //End test code
        }

        //IDEA: Multi-pass rendering? One pass to get everything down, then make another pass on the UI thread to see
        //if we missed anything/rendered too much?
        private void DrawPages()
        {
            DrawnPages.Clear();
            foreach (var page in CurrentBook.Pages)
            {
                RichTextBlockOverflow lastOverflow;
                lastOverflow = DrawOnePage(null, page.PageContent);
                DrawnPages.Add(lastOverflow);

                while (lastOverflow.HasOverflowContent)
                {
                    lastOverflow = DrawOnePage(lastOverflow, page.PageContent);
                }
            }
        }

        //TODO: Split this out into DrawFirstPage and DrawSubpages
        private RichTextBlockOverflow DrawOnePage(RichTextBlockOverflow lastOverflow, string pageContent)
        {
            bool isFirstPage = lastOverflow == null;
            RichTextBlockOverflow rtbo = new RichTextBlockOverflow();

            if (isFirstPage)
            {
                RichTextBlock pageOne = new RichTextBlock();
                pageOne.FontSize = 16.00;
                pageOne.MaxWidth = this.TextboxMaxWidth;
                pageOne.MaxHeight = this.TextboxMaxHeight;
                pageOne.HorizontalAlignment = HorizontalAlignment.Left;
                pageOne.VerticalAlignment = VerticalAlignment.Top;
                pageOne.IsDoubleTapEnabled = false;
                pageOne.IsHoldingEnabled = false;
                pageOne.SetValue(Helpers.Properties.HtmlProperty, pageContent);
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
                DrawnPages.Add(pageOne);
                if (pageOne.HasOverflowContent)
                {
                    pageOne.OverflowContentTarget = rtbo;
                    rtbo.Measure(new Size(this.TextboxMaxWidth, this.TextboxMaxHeight));
                }
            }
            else
            {
                lastOverflow.SetBinding(RichTextBlockOverflow.MaxWidthProperty, new Binding
                    {
                        Source = TextboxMaxWidth,
                        Path = new PropertyPath("MaxWidth")
                    });
                lastOverflow.SetBinding(RichTextBlockOverflow.MaxHeightProperty, new Binding
                    {
                        Source = TextboxMaxHeight,
                        Path = new PropertyPath("MaxHeight")
                    });
                if (lastOverflow.HasOverflowContent)
                {
                    lastOverflow.OverflowContentTarget = rtbo;
                    lastOverflow.Measure(new Size(this.TextboxMaxWidth, this.TextboxMaxHeight));
                    rtbo.Measure((new Size(this.TextboxMaxWidth, this.TextboxMaxHeight)));
                }
                this.DrawnPages.Add(rtbo);
            }
            return rtbo;
        }
    }
}
