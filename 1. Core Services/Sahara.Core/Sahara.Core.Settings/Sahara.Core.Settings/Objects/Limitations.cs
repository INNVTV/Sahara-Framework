using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Objects
{
    public static class Limitations
    {
        public static int MaximumImageFormatsAsListings = 12; //<--To ensure fast API calls
        public static int ImageFormatTitleMaxLength = 60;
        public static int ImageFormatDescriptionMaxLength = 140;
    }
}
