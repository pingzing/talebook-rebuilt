using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Windows.UI;

namespace TalebookRebuilt.Helpers
{
    public static class Utilities
    {
        //TODO: Add a fallback, where we also check to see if we just have a hex string.
        //Also, add string manipulation to make sure casing is correct and whitespace is stripped if we're getting a color name
        public static Color StringToColor(string colorString)
        {            
            var property = typeof(Colors).GetRuntimeProperty(colorString);
            Color color = Colors.Gray;
            if (property != null)
            {
                color = (Color)property.GetValue(null);                
            }
            return color;
        }
    }
}
