using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts.TableEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Internal
{
    internal static class Transformations
    {
        internal static UserInvitation TransformToUserInvitation(UserInvitationsTableEntity userInvitationTableEntity, string accountAtribute = null)
        {
            UserInvitation userInvitation = null;

            if (userInvitationTableEntity != null)
            {
                userInvitation = new UserInvitation
                {
                    Email = userInvitationTableEntity.Email,
                    FirstName = userInvitationTableEntity.FirstName,
                    LastName = userInvitationTableEntity.LastName,
                    Role = userInvitationTableEntity.Role,
                    InvitationKey = userInvitationTableEntity.InvitationKey,
                    Owner = userInvitationTableEntity.Owner
                };

                if (accountAtribute != null)
                {
                    var account = AccountManager.GetAccount(accountAtribute);

                    userInvitation.AccountID = account.AccountID.ToString();
                    userInvitation.AccountName = account.AccountName;
                    userInvitation.AccountNameKey = account.AccountNameKey;
                    userInvitation.AccountLogoUrl = ""; //<-- To Add
                }
            }

            return userInvitation;
        }

        internal static UserPasswordResetClaim TransformToPasswordResetClaim(PasswordClaimTableEntity passwordClaimTableEntity)
        {
            UserPasswordResetClaim passwordClaim = null;

            if (passwordClaimTableEntity != null)
            {
                passwordClaim = new UserPasswordResetClaim
                {
                    Email = passwordClaimTableEntity.Email,
                    PasswordClaimKey = passwordClaimTableEntity.PasswordClaimKey
                };
            }

            return passwordClaim;
        }
    }
}
