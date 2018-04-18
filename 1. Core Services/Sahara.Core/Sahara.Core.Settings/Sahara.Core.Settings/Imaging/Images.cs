using Sahara.Core.Settings.Models.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Imaging
{

    public static class Images
    {
        #region Private Properties

        private static string _platformBlobUri;
        private static string _platformCdnUri;

        private static string _intermediaryBlobUri;
        private static string _intermediaryCdnUri;

        #endregion

        #region Images

        //Profile Photos (No source - server edits based on settings)
        public static ImageFormatModel PlatformUserProfilePhoto = new ImageFormatModel(); //<--- Does not use a source file, backend handles all editing
        public static ImageFormatModel AccountUserProfilePhoto = new ImageFormatModel();
        public static String SwatchPropertyImageFolderName = "swatchimages";

        #endregion 

        //public static ImageFormatModel ProfileImage;

        #region INITIALIZATION

        internal static void Initialize()
        {

            #region Apply Image Settings


            #region Platform User Profile Photo

            //PlatformUserProfilePhoto.AspectRatio = new Size { X = 1, Y = 1 };
            PlatformUserProfilePhoto.MinSize = new Size { X=16, Y=16 };

            PlatformUserProfilePhoto.ParentName = "userphotos";

            PlatformUserProfilePhoto.SetSizes = new List<Size>(); //<-- if no set set sizes exist we only resize an image by the aspect ratio
            PlatformUserProfilePhoto.SetSizes.Add(new Size { X = 16, Y = 16 });
            PlatformUserProfilePhoto.SetSizes.Add(new Size { X = 32, Y = 32 });
            PlatformUserProfilePhoto.SetSizes.Add(new Size { X = 64, Y = 64 });
            PlatformUserProfilePhoto.SetSizes.Add(new Size { X = 128, Y = 128 });

            #endregion

            #region Account User Profile Photo

            //AccountUserProfilePhoto.AspectRatio = new Size { X = 1, Y = 1 };
            AccountUserProfilePhoto.MinSize = new Size { X = 16, Y = 16 };

            AccountUserProfilePhoto.ParentName = "userphotos";

            AccountUserProfilePhoto.SetSizes = new List<Size>(); //<-- if no set set sizes exist we only resize an image by the aspect ratio
            AccountUserProfilePhoto.SetSizes.Add(new Size { X = 16, Y = 16 });
            AccountUserProfilePhoto.SetSizes.Add(new Size { X = 32, Y = 32 });
            AccountUserProfilePhoto.SetSizes.Add(new Size { X = 64, Y = 64 });
            AccountUserProfilePhoto.SetSizes.Add(new Size { X = 128, Y = 128 });

            #endregion


            #region ApplicationImage

            #endregion

            #endregion

            #region Initialize Imaging URI's

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    _platformBlobUri = "[Config_Name].blob.core.windows.net";
                    _platformCdnUri = "https://[Config_Name]-platform.azureedge.net";

                    _intermediaryBlobUri = "[Config_Name]intermediate.blob.core.windows.net";
                    _intermediaryCdnUri = "https://[Config_Name]-intermediary.azureedge.net";

                    break;

                #endregion


                #region Stage

                case "stage":

                    _platformBlobUri = "[Config_Name].blob.core.windows.net";
                    _platformCdnUri = "https://[Config_Name]-platform-stage.azureedge.net";

                    _intermediaryBlobUri = "[Config_Name]stage.blob.core.windows.net";
                    _intermediaryCdnUri = "https://[Config_Name]-intermediary-stage.azureedge.net";

                    break;


                #endregion


                #region Local/Debug

                case "debug":

                    _platformBlobUri = "[Config_Name].blob.core.windows.net";
                    _platformCdnUri = "https://[Config_Name]-platform-debug.azureedge.net";

                    _intermediaryBlobUri = "[Config_Name]intermediate.blob.core.windows.net";
                    _intermediaryCdnUri = "https://[Config_Name]-intermediary.azureedge.net";

                    break;


                case "local":

                    _platformBlobUri = "[Config_Name].blob.core.windows.net";
                    _platformCdnUri = "https://[Config_Name]-platform.azureedge.net";

                    _intermediaryBlobUri = "[Config_Name]intermediate.blob.core.windows.net";
                    _intermediaryCdnUri = "https://[Config_Name]-intermediary.azureedge.net";

                    break;

                #endregion

                default:

                    break;


            }

            #endregion

        }

        #endregion

        // Platform -------------------------------

        public static string PlatformBlobUri
        {
            get
            {
                return _platformBlobUri;
            }
        }

        public static string PlatformCdnUri
        {
            get
            {
                return _platformCdnUri;
            }
        }


        // Intermediary -------------------------------

        public static string IntermediaryBlobUri
        {
            get
            {
                return _intermediaryBlobUri;
            }
        }

        public static string IntermediaryCdnUri
        {
            get
            {
                return _intermediaryCdnUri;
            }
        }

    }





    /*
    public class ImageFormatModel
    {
        public int height;
        public int width;

    }
     * */
}
