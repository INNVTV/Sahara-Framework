using InventoryHawk.Account.Public.Api.ApplicationImageRecordsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Dynamics
{
    public static class Images
    {
        public static IDictionary<string, object> BuildDynamicImagesListForJson(string accountNameKey, string imageFormatGroupTypeNameKey, string objectId, bool listingsOnly)
        {
            //Get listing image records for each object from Table Storage
            var imageRecordGroups = DataAccess.ImageRecords.GetImageRecordsForObject(accountNameKey, imageFormatGroupTypeNameKey, objectId, listingsOnly);

            IDictionary<string, object> images = new System.Dynamic.ExpandoObject();

            foreach (var imageGroup in imageRecordGroups)
            {
                IDictionary<string, object> dynamicImageGroup = new System.Dynamic.ExpandoObject();

                //List<IDictionary<string, object>> dynamicImageRecords = new List<IDictionary<string, object>>();

                foreach (ImageRecordModel imageRecord in imageGroup.ImageRecords)
                {

                    //IDictionary<string, object> dynamicImageRecord = new System.Dynamic.ExpandoObject();
                    IDictionary<string, object> dynamicImageRecordProperties = new System.Dynamic.ExpandoObject();

                    if(imageRecord.Type == "gallery" && imageRecord.GalleryImages != null)
                    {
                        //dynamicImageRecordProperties["urls"] = imageRecord.GalleryImages;

                        List<IDictionary<string, object>> galleryImages = new List<IDictionary<string, object>>();

                        foreach (ImageRecordGalleryModel galleryItem in imageRecord.GalleryImages)
                        {
                            IDictionary<string, object> galleryImage = new System.Dynamic.ExpandoObject();

                            galleryImage["url"] = galleryItem.Url;
                            galleryImage["title"] = galleryItem.Title;
                            galleryImage["description"] = galleryItem.Description;
                            galleryImage["filename"] = galleryItem.FileName;

                            galleryImage["height"] = imageRecord.Height;
                            if (imageRecord.Width == 0)
                            {
                                galleryImage["width"] = null;
                            }
                            else
                            {
                                galleryImage["width"] = imageRecord.Width;
                            }

                            galleryImages.Add(galleryImage);
                            
                        }

                        dynamicImageRecordProperties["images"] = galleryImages;
                    }
                    else
                    {
                        dynamicImageRecordProperties["url"] = imageRecord.Url;
                        dynamicImageRecordProperties["title"] = imageRecord.Title;
                        dynamicImageRecordProperties["description"] = imageRecord.Description;
                        dynamicImageRecordProperties["filename"] = imageRecord.FileName;
                    }

                    dynamicImageRecordProperties["height"] = imageRecord.Height;
                    if (imageRecord.Width == 0)
                    {
                        dynamicImageRecordProperties["width"] = null;
                    }
                    else
                    {
                        dynamicImageRecordProperties["width"] = imageRecord.Width;
                    }


                    if (!((IDictionary<String, object>)images).ContainsKey(imageGroup.GroupNameKey))
                    {
                        images[imageGroup.GroupNameKey] = new System.Dynamic.ExpandoObject();
                    }

                    ((IDictionary<String, Object>)(images[imageGroup.GroupNameKey]))[imageRecord.FormatNameKey] = dynamicImageRecordProperties;

                    //dynamicImageRecords.Add(dynamicImageRecord);
                }

                //images[imageGroup.GroupNameKey] = dynamicImageRecords;
            }

            return images;
        }

    }
}