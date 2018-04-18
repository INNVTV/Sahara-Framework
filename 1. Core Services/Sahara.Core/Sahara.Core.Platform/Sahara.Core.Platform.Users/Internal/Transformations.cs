using Sahara.Core.Platform.Users.Models;
using Sahara.Core.Platform.Users.TableEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Users.Internal
{
    public static class Transformations
    {
        public static PlatformInvitation TransformToPlatformInvitation(PlatformInvitationsTableEntity platformInvitationTableEntity)
        {
            PlatformInvitation userInvitation = null;

            if (platformInvitationTableEntity != null)
            {
                userInvitation = new PlatformInvitation
                {
                    Email = platformInvitationTableEntity.Email,
                    FirstName = platformInvitationTableEntity.FirstName,
                    LastName = platformInvitationTableEntity.LastName,
                    Role = platformInvitationTableEntity.Role,
                    InvitationKey = platformInvitationTableEntity.InvitationKey
                };
            }

            return userInvitation;
        }

        public static PlatformPasswordResetClaim TransformToPasswordResetClaim(PasswordClaimTableEntity passwordClaimTableEntity)
        {
            PlatformPasswordResetClaim passwordClaim = null;

            if (passwordClaimTableEntity != null)
            {
                passwordClaim = new PlatformPasswordResetClaim
                {
                    Email = passwordClaimTableEntity.Email,
                    PasswordClaimKey = passwordClaimTableEntity.PasswordClaimKey
                };
            }

            return passwordClaim;
        }
    }
}
