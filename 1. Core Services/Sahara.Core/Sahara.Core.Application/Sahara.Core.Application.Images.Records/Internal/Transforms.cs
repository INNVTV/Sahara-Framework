using Sahara.Core.Application.Images.Formats.Models;
using Sahara.Core.Application.Images.Records.Models;
using Sahara.Core.Application.Images.Records.TableEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Records.Internal
{
    internal static class Transforms
    {
        public static ImageRecordModel TableEntity_To_ImageRecord(ImageRecordTableEntity imageRecordTableEntity, ImageFormatModel imageFormat, bool isGallery = false)
        {
            var imageRecord = new ImageRecordModel();

            
            imageRecord.Height = imageRecordTableEntity.Height;
            imageRecord.Width = imageRecordTableEntity.Width;
            imageRecord.ContainerName = imageRecordTableEntity.ContainerName;

            if (!isGallery)
            {
                imageRecord.Type = "single";

                imageRecord.Title = imageRecordTableEntity.Title;
                imageRecord.Description = imageRecordTableEntity.Description;

                imageRecord.BlobPath = imageRecordTableEntity.BlobPath;
                imageRecord.Url = imageRecordTableEntity.Url;
                imageRecord.FileName = imageRecordTableEntity.FileName;
                imageRecord.FilePath = imageRecordTableEntity.FilePath;

                imageRecord.BlobPath_sm = imageRecordTableEntity.BlobPath.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                imageRecord.Url_sm = imageRecordTableEntity.Url.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                imageRecord.FileName_sm = imageRecordTableEntity.FileName.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                imageRecord.FilePath_sm = imageRecordTableEntity.FilePath.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");

                imageRecord.BlobPath_xs = imageRecordTableEntity.BlobPath.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                imageRecord.Url_xs = imageRecordTableEntity.Url.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                imageRecord.FileName_xs = imageRecordTableEntity.FileName.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                imageRecord.FilePath_xs = imageRecordTableEntity.FilePath.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");


            }
            else
            {
                imageRecord.Type = "gallery";


                var blobPaths = imageRecordTableEntity.BlobPath.Split('|').ToList();

                var titles = imageRecordTableEntity.Title.Split('|').ToList();
                var descriptions = imageRecordTableEntity.Description.Split('|').ToList();
                var urls = imageRecordTableEntity.Url.Split('|').ToList();
                var fileNames = imageRecordTableEntity.FileName.Split('|').ToList();
                var filePaths = imageRecordTableEntity.FilePath.Split('|').ToList();

                if(urls.Count > 0)
                {
                    imageRecord.GalleryImages = new List<ImageRecordGalleryModel>();

                    for (int i = 0; i < urls.Count; i++)
                    {
                        var imageGalleryRecord = new ImageRecordGalleryModel
                        {
                            Url = urls[i],
                            Url_sm = urls[i].Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png"),
                            Url_xs = urls[i].Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png")
                        };

                        try
                        {
                            imageGalleryRecord.Title = titles[i];
                            imageGalleryRecord.Description = descriptions[i];

                            imageGalleryRecord.BlobPath = blobPaths[i];
                            imageGalleryRecord.FileName = fileNames[i];
                            imageGalleryRecord.FilePath = filePaths[i];

                            imageGalleryRecord.BlobPath_sm = blobPaths[i].Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                            imageGalleryRecord.FileName_sm = fileNames[i].Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                            imageGalleryRecord.FilePath_sm = filePaths[i].Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");

                            imageGalleryRecord.BlobPath_xs = blobPaths[i].Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                            imageGalleryRecord.FileName_xs = fileNames[i].Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                            imageGalleryRecord.FilePath_xs = filePaths[i].Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                        }
                        catch
                        {

                        }

                        imageRecord.GalleryImages.Add(imageGalleryRecord);
                    }
                }
            }

            imageRecord.FormatName = imageFormat.ImageFormatName;
            imageRecord.FormatNameKey = imageFormat.ImageFormatNameKey;

            return imageRecord;
        }

        public static ImageRecordModel ImageFormat_To_ImageRecord(ImageFormatModel imageFormat)
        {
            var imageRecord = new ImageRecordModel();

            if (!imageFormat.Gallery)
            {
                imageRecord.Type = "single";
            }
            else
            {
                imageRecord.Type = "gallery";
            }

            imageRecord.FormatName = imageFormat.ImageFormatName;
            imageRecord.FormatNameKey = imageFormat.ImageFormatNameKey;

            imageRecord.Height = imageFormat.Height;
            imageRecord.Width = imageFormat.Width;

            return imageRecord;
        }
    }
}
