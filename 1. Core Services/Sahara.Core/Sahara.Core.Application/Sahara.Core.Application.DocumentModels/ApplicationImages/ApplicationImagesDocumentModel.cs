using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Microsoft.Azure.Documents;


namespace Sahara.Core.Application.DocumentModels.Application.ApplicationImages.DocumentModels
{
    /**/
    [DataContract]
    public class ApplicationImageDocumentModel
    {
        [DataMember]
        [JsonProperty(PropertyName = "id")] //<-- Required for all Documents
        public string Id;

        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; internal set; }

        //----------- Isolate Tenant Images via AccountID & DocumentType -------------------------

        [DataMember]
        public string AccountID; 

        [DataMember]
        public string DocumentType;

        //------------- Relational Data ------------------------

        [DataMember]
        public string CategoryID;

        [DataMember]
        public string SubcategoryID;

        //------------------------------------------------------


        [DataMember]
        public string Title;

        [DataMember]
        public string Description;

        [DataMember]
        public string FilePath; 

        [DataMember]
        public ApplicationImageFileSizes FileNames; //<-- Just the file name

        //-------------- Flat Data -------------------------------

        [DataMember]
        public List<string> Tags; //<-- Searchable tags

        //---------------------------------------------------------
    }

    /// <summary>
    /// In this scenario we have the image processor start with the "Large" size (whixh is our default), half it once for 'Medium', once again for 'Small' and then generate a 96px tall thumbnail for 'Thumbnail' 
    /// In future scenarios you may manage this via CMS, Static classes, specific sizes for each, or base it on aspect ratio
    /// </summary>
    [DataContract]
    public class ApplicationImageFileSizes
    {

        [DataMember]
        public string Large;

        [DataMember]
        public string Medium;

        [DataMember]
        public string Small;

        [DataMember]
        public string Thumbnail;
    }
}
