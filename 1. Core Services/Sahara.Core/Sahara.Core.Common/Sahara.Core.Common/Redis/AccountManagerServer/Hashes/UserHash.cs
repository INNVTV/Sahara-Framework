using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class UserHash
    {
       


        public static string Key(string userId)
        {
            return "user:" + userId.Replace("-", "").ToLower();
        }

        public static class Fields
        {
            public static string Role()
            {
                return "role";
            }

            public static string Model()
            {
                return "model";
            }

            //public static string Identity()
            //{
                //return "identity";
            //}

            /*
            public static string Notifications(string notificationStatus)
            {
                return "notifications:all:" + notificationStatus;
            }*/

            public static string Notifications(string notificationType, string notificationStatus)
            {
                return "notifications:" + notificationType + ":" + notificationStatus;
            }

        }
    }

    /// <summary>
    /// Lookup Hash's for storing UserId's by UserName, sharded by first 2 letters of username
    /// </summary>
    public static class UserIdByUserNameHash
    {
        public static string Key(string userName)
        {
            return "useridbyusername:" + userName.Substring(0,2).ToLower(); //<-- We hash the list by the first 2 letters
        }

        public static class Fields
        {
            public static string UserId(string userName)
            {
                return userName;
            }
        }
    }
}
