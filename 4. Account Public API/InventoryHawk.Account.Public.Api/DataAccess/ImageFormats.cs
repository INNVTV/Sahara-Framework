using InventoryHawk.Account.Public.Api.ApplicationImageFormatsService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.DataAccess
{
    public static class ImageFormats
    {
        public static List<ImageFormatGroupModel> GetImageFormatsHelper(string accountNameKey, string imageFormatGroupTypeNameKey)
        {
            List<ImageFormatGroupModel> imageGroups = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":imageformats";
                string hashField = "grouptype:" + imageFormatGroupTypeNameKey + ":all";

                try
                {
                    var redisValue = cache.HashGet(hashKey, hashField);

                    if (redisValue.HasValue)
                    {
                        imageGroups = JsonConvert.DeserializeObject<List<ImageFormatGroupModel>>(redisValue);
                    }
                }
                catch
                {

                }
                


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (imageGroups == null)
            {
                #region (Plan B) Get data from WCF

                var applicationImageFormatsServiceClient = new ApplicationImageFormatsService.ApplicationImageFormatsServiceClient();

                try
                {
                    applicationImageFormatsServiceClient.Open();
                    imageGroups = applicationImageFormatsServiceClient.GetImageFormats(accountNameKey, imageFormatGroupTypeNameKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationImageFormatsServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationImageFormatsServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            return imageGroups;
        }
    }
}