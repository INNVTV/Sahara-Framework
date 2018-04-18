using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Capacity.Models
{
    [Serializable]
    public class AccountCapacity
    {
        public int UsersCount;
        public int UsersMax;
        public int UsersRemaining;
        public int UsersPercentageUsed;

        public int ProductsCount;
        public int ProductsMax;
        public int ProductsRemaining;
        public int ProductsPercentageUsed;

        public int CategorizationsCount;
        public int CategorizationsMax;
        public int CategorizationsRemaining;
        public int CategorizationsPercentageUsed;

        public int TagsCount;
        public int TagsMax;
        public int TagsRemaining;
        public int TagsPercentageUsed;

        public int PropertiesCount;
        public int PropertiesMax;
        public int PropertiesRemaining;
        public int PropertiesPercentageUsed;

        public int CustomImageGroupsCount;
        public int CustomImageGroupsMax;
        public int CustomImageGroupsRemaining;
        public int CustomImageGroupsPercentageUsed;

        public int CustomImageFormatsCount;
        public int CustomImageFormatsMax;
        public int CustomImageFormatsRemaining;
        public int CustomImageFormatsPercentageUsed;

        public int CustomImageGalleriesCount;
        public int CustomImageGalleriesMax;
        public int CustomImageGalleriesRemaining;
        public int CustomImageGalleriesPercentageUsed;

        public int ImagesPerGalleryMax;

    }
}
