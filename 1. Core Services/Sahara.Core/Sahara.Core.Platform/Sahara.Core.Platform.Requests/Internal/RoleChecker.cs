using System.Collections.Generic;
using Sahara.Core.Platform.Requests.Models;

namespace Sahara.Core.Platform.Requests.Internal
{
    internal static class RoleChecker
    {
        /// <summary>
        /// Checks if the userRole is at or above the lowestRoleRequirement
        /// </summary>
        /// <param name="requsterType"></param>
        /// <param name="userRole"></param>
        /// <param name="lowestRoleRequirement"></param>
        /// <returns></returns>
        public static bool IsRoleAllowed(RequesterType requsterType, string userRole, string lowestRoleRequirement)
        {
            bool isAllowed = false;

            List<string> comparisonRoles = null;

            if (lowestRoleRequirement != null)
            {
                //Get comparison roles:
                switch (requsterType)
                {
                    case RequesterType.PlatformUser:
                        comparisonRoles = Sahara.Core.Settings.Platform.Users.Authorization.Roles.GetRoles();
                        break;
                    case RequesterType.AccountUser:
                        comparisonRoles = Sahara.Core.Settings.Accounts.Users.Authorization.Roles.GetRoles();
                        break;
                    default:
                        break;
                }

                //Check if the userRole is at or above the lowestRoleRequirement
                if (comparisonRoles != null)
                {
                    //Get index of lowestRoleRequirement
                    var lowestIndex = comparisonRoles.IndexOf(lowestRoleRequirement);
                    var comparisonIndex = comparisonRoles.IndexOf(userRole);

                    if (comparisonIndex >= lowestIndex)
                    {
                        isAllowed = true;
                    }
                }
            }
            else
            {
                isAllowed = false;
            }



            return isAllowed;
        }
    }
}
