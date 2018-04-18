using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Imaging_API
{
    public static class EnvironmentSettings
    {
        public static class CurrentEnvironment
        {
            public static string Site { get; set; }
            public static string CoreServices { get; set; }
        }
    }
}