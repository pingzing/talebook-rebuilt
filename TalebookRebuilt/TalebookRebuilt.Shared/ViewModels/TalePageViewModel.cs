using Newtonsoft.Json;
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
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace TalebookRebuilt.ViewModels
{
    public class TalePageViewModel : INotifyPropertyChanged
    {
        private const int HEADER_HEIGHT = 120;
        private FrameworkElement previousPage = null;

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

        private Color pageColor;
        public Color PageColor
        {
            get { return pageColor; }
            set
            {
                this.pageColor = value;
                NotifyPropertyChanged("PageColor");
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
            int pageToRestore = CurrentPageNum;
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

            if(CurrentBook != null)
            {
                DrawPages();
            }            
            if (flip != null)
            {                
                flip.UpdateLayout();
                var dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
                //Force this to run on the UI thread, so we KNOW it'll wait until the text is 
                //properly distributed to all the RTBOverflows
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        RestoreReadingLocation(flip, locToRestore, pageToRestore);
                    });
            }
        }

        //If we have the old character-location, restore the reader's current page        
        private void RestoreReadingLocation(FlipView flip, int locToRestore, int pageToRestore)
        {            
            List<int> pageNumbers = new List<int>(DrawnPages.Count);
            int restoreIndex = -1;
            foreach (var subpage in DrawnPages)
            {
                if (subpage is RichTextBlock)
                {
                    pageNumbers.Add(DrawnPages.IndexOf(subpage));
                }                
            }
            int start = pageNumbers[pageToRestore];            
            for (int i = start; i < flip.Items.Count; i++)
            {
                if(i != start && flip.Items[i] is RichTextBlock)
                {
                    break;  //Only check the current page (pages begin with a RichTextBlock)
                }
                dynamic pageToInspect = flip.Items[i];
                if (pageToInspect.ContentEnd.Offset >= locToRestore && pageToInspect.ContentStart.Offset <= locToRestore)
                {
                    restoreIndex = i;
                    break;
                }
            }

            if(restoreIndex != -1)
            {
                flip.SelectedIndex = restoreIndex;
                //Resize timing happens non-deterministically and may have wiped this out CurrentPageNum. Restore it, just in case
                CurrentPageNum = pageToRestore; 
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

        //TODO: Update subpage number, display to user as pages?
        //Update current-location?
        private void OnPageFlippedCommand(object np)
        {
            var rtb = np as RichTextBlock;
            var rtbo = np as RichTextBlockOverflow;
            if(rtb != null)
            {
                int previousPageIndex = previousPage == null ? 0 : DrawnPages.IndexOf(previousPage);
                int newPageIndex = DrawnPages.IndexOf(rtb);
                //Forward: RTB/O -> RTB
                if(newPageIndex > previousPageIndex)
                {                    
                    CurrentPageNum++;
                }               
                //Backward: RTB <- RTB
                else if(newPageIndex < previousPageIndex && previousPage as RichTextBlock != null)
                {
                    CurrentPageNum--;
                }
                previousPage = rtb;
            }            
            if(rtbo != null)
            {
                int previousPageIndex = previousPage == null ? 0 : DrawnPages.IndexOf(previousPage);
                int newPageIndex = DrawnPages.IndexOf(rtbo);
                //Backward: RTB <- RTBO
                if (newPageIndex < previousPageIndex && previousPage as RichTextBlock != null)
                {
                    CurrentPageNum--;
                }
                previousPage = rtbo;
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

        //TODO: Refactor all this into something a little more dynamic/generic
        async private void BuildPages()
        {            
            string filePath = "Snowbird\\text\\Snowbird.html"; //Make this change based on which tale was selected on the previous page
            string rawHtml = await BookBuilder.HtmlToString(filePath);
            List<string> slicedPages = BookBuilder.GetSlicedPages(rawHtml);

            currentBook = new TaleBook();
            currentBook.Pages = new List<TalePage>();

            //TODO: Read story's JSON file to get each page's info
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync("Tales");
            var file = await folder.GetFileAsync("Snowbird\\snowbird.json");
            var stream = await file.OpenStreamForReadAsync();
            JsonTextReader jtr = new JsonTextReader(new StreamReader(stream));
            JsonSerializer jss = new JsonSerializer();
            jss.Deserialize(jtr, typeof(TaleBook)); //See http://james.newtonking.com/json/help/index.html?topic=html/SerializationCallbacks.htm for fixing Image <-> Path issue

            int pageNum = 0;
            foreach (var page in slicedPages)
            {
                currentBook.Pages.Add(new TalePage(page, null, Colors.Black, pageNum));
                pageNum++;
            }
            currentBook.Cover = null;
            currentBook.Description = "Test description";
            currentBook.Title = "Test Title";
            CurrentBook = currentBook;
            DrawPages();            
        }

        private void DrawPages()
        {
            DrawnPages.Clear();
            foreach (var page in CurrentBook.Pages)
            {
                RichTextBlockOverflow lastOverflow;
                lastOverflow = DrawOnePage(null, page.PageContent);
                if (lastOverflow != null) { DrawnPages.Add(lastOverflow); }
                
                while (lastOverflow != null && lastOverflow.HasOverflowContent)
                {
                    lastOverflow = DrawOnePage(lastOverflow, page.PageContent);
                }               
            }
        }

        //TODO: Split this out into DrawFirstPage and DrawSubpages
        private RichTextBlockOverflow DrawOnePage(RichTextBlockOverflow lastOverflow, string pageContent)
        {
            bool isFirstPage = lastOverflow == null;
            RichTextBlockOverflow rtbo = null;

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
                    rtbo = new RichTextBlockOverflow();
                    pageOne.OverflowContentTarget = rtbo;
                    rtbo.Measure(new Size(this.TextboxMaxWidth, this.TextboxMaxHeight));
                }
            }
            else
            {
                rtbo = new RichTextBlockOverflow();
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
