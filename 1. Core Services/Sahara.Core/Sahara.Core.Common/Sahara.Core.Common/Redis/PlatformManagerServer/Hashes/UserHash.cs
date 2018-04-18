using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.PlatformManagerServer.Hashes
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


        }
    }

    /// <summary>
    /// Lookup Hash's for storing UserId's by UserName
    /// </summary>
    public static class UserIdByUserNameHash
    {
        public static string Key = "useridbyusername";

        public static class Fields
        {
            public static string UserId(string userName)
            {
                return userName;
            }
        }
    }

}
