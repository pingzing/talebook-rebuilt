using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TalebookRebuilt.Helpers
{
    public static class BookBuilder
    {
        //Take some form of text document and convert it to a TaleBook object

        /// <summary>
        /// Reads an RTF file into a string and returns it.
        /// </summary>
        /// <param name="fileName"> The file name of the .RTF file to be read. </param>
        /// <returns>A string containing the entirety of the RTF document, including formatting 
        /// text, or an empty string if the file does not exist.</returns>
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
    }
}
