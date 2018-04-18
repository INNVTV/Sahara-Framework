using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Themes.Models
{
    [DataContract]
    public class ThemeColorsModel
    {
        [DataMember]
        public string Background;
        [DataMember]
        public string BackgroundGradianetTop;
        [DataMember]
        public string BackgroundGradientBottom;
        [DataMember]
        public string Shadow;
        [DataMember]
        public string Highlight;
        [DataMember]
        public string Foreground;
        [DataMember]
        public string Overlay;
        [DataMember]
        public string Trim;
    }
}
