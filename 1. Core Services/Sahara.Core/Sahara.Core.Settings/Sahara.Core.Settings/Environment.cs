using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings
{
    public static class Environment
    {
        /// <summary>
        /// This is the one setting that all parent projects MUST have in their .Config files
        /// </summary>
        public static string Current; // = ConfigurationManager.AppSettings["Environment"];
    }
  
}
