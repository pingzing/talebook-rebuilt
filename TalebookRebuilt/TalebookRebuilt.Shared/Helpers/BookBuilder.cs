using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TalebookRebuilt.Helpers
{
    public static class BookBuilder
    {
        //Take some form of text document and convert it to a TaleBook object

        /// <summary>
        /// Reads an HTML file, converts it into a string and returns the string.
        /// </summary>
        /// <param name="fileName"> The file name of the .HTML file to be read. </param>
        /// <returns>A string containing the entirety of the HTML document,
        /// or an empty string if the file does not exist.</returns>
        public async static Task<string> HtmlToString(string fileName)
        {
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync("Tales");            
            StorageFile tale = await folder.GetFileAsync(fileName);
            string htmlString = "";

            if (tale != null)
            {
                using (var stream = await tale.OpenReadAsync())
                {
                    using(var readStream = stream.AsStreamForRead())
                    {
                        using (StreamReader streamReader = new StreamReader(readStream))
                        {
                            htmlString = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            return htmlString;
        }

        public static List<string> GetSlicedPages(string htmlString)
        {
            //Slice the big HTML page into smaller pages based on page break style tags
            string pageBreakToken = "<p style=\"page-break-before: always\"></p>";
            List<string> slicedPages = new List<string>();
            slicedPages.AddRange(htmlString.Split(new string[] { pageBreakToken }, StringSplitOptions.None));  
         
            //Add HTML headers for stylesheets and <head> and <body> tags to all our new little slices of HTML
            Regex bodyRegex = new Regex("<head>(.*)</head>", RegexOptions.IgnoreCase);
            string htmlHeader = bodyRegex.Match(htmlString).Value;
            for (int i = 0; i < slicedPages.Count; i++)
            {
                slicedPages[i] = htmlHeader + "<body>" + slicedPages[i] + "</body>";
            }
                return slicedPages;
        }
    }
}
