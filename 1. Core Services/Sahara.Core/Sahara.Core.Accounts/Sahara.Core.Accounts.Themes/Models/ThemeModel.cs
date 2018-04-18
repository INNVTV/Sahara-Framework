using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Themes.Models
{
    [DataContract]
    public class ThemeModel
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string NameKey;
        [DataMember]
        public string Font;

        [DataMember]
        public ThemeColorsModel Colors;
    }

}
