using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Common.Validation;
using Sahara.Core.Platform.Users.DataContext;
using Sahara.Core.Platform.Users.Models;
using Sahara.Core.Platform.Users.Internal;
using Sahara.Core.Platform.Users.Sql.Statements;
using Sahara.Core.Platform.Users.TableEntities;
using StackExchange.Redis;
using Sahara.Core.Common.Redis.PlatformManagerServer.Hashes;
using Sahara.Core.Common.Redis;
using Newtonsoft.Json;
using Sahara.Core.Logging.PlatformLogs.Helpers;

namespace Sahara.Core.Platform.Users
{
    public class PlatformUserManager
    {

        #region Authentication & Claims

        /// <summary>
        /// Used to log users into client applications
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static DataAccessResponseType GetUserWithLogin(string email, string password)
        {
            var response = new DataAccessResponseType();

            try
            {
                var userManager = new UserManager<PlatformUserIdentity>(
                    new UserStore<PlatformUserIdentity>(new PlatformUserIdentityDbContext()));

                PlatformUserIdentity user = userManager.Find(email, password);

                if (user != null)
                {
                    response.isSuccess = true;
                    response.ResponseObject = user; //<-- ResponseObject can be converted to PlatformUser by consuming application

                    //Update to non-identity version in Cache
                    GetUserIdentity(user.Id.ToString());

                    return response;
                }
                else
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Invalid login, please try again.";

                    return response;
                }


            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to login platform user with email: " + email,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                response.isSuccess = false;
                response.ErrorMessage = e.Message;

                return response;
            }


        }

        // For MVC/WebForms based web applications:
        public static ClaimsIdentity GetUserClaimsIdentity(PlatformUserIdentity user, string authenticationType)
        {
            try
            {
                var userManager = new UserManager<PlatformUserIdentity>(
                        new UserStore<PlatformUserIdentity>(new PlatformUserIdentityDbContext()));

                //return userManager.CreateIdentityAsync(user, authenticationType).Result;
                return userManager.CreateIdentity(user, authenticationType);

            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to get platform user claim identity for: " + user.UserName,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        #endregion

        #region Get Users

        internal static IdentityResult CreateUserInternal(PlatformUserIdentity user, string password)
        {

            var userManager = new UserManager<PlatformUserIdentity>(
                new UserStore<PlatformUserIdentity>(new PlatformUserIdentityDbContext()));

            //Allows for use of email address as username:
            userManager.UserValidator = new UserValidator<PlatformUserIdentity>(userManager) { AllowOnlyAlphanumericUserNames = false };

            //var idResult = await userManager.CreateAsync(user, password);
            var idResult = userManager.Create(user, password);


            InvalidatePlatformUsersListCache();
            
            return idResult;

        }


        /// <summary>
        /// Only pulls from Cache created by GetUserIdentity. Mostly used to keep views in sync with latest user role & properties
        /// </summary>
        /// <param name="userId"></param>
        public static PlatformUser GetUser(string userId, bool useCachedVersion = true)
        {
            if (!useCachedVersion)
            {
                //cache non cached version is requested, transform it from UserIdentity (Will cache the latest version for us)
                return TransformPlatformUserIdentityToPlatformUser(GetUserIdentity(userId));
            }

            PlatformUser platformUserCache = null;

            try
            {
                //Get user from Cache, if doesn't exist, get Idetity version, convert and cache it for next call
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    var redisValue = cache.HashGet(UserHash.Key(userId), UserHash.Fields.Model()); //<-- only cache the "model" or "non-Identity" version by ID (This is for WCF clients that cannot consume Identity veresions of users)
                    if (redisValue.HasValue)
                    {
                        platformUserCache = JsonConvert.DeserializeObject<PlatformUser>(redisValue);
                    }
                    //var platformUserCache = JsonConvert.DeserializeObject< PlatformUser > (); 
                }
                catch
                {

                }


                //DataCache dataCache = new DataCache(platformUserCacheName);
                //string cacheID = PlatformUserCacheID.PlatformUser(userId); //<-- only cache non Identoty version by ID (This is for WCF clients that cannot consume Identity veresions of users)

                //object platformUserCache = dataCache.Get(cacheID);
            }
            catch
            {

            }


            if (platformUserCache != null)
            {
                //return the cached version of the User
                return platformUserCache;
            }
            else
            {

                //cache is empty, transform a fresh version of AccountUserIdentity (will create a cache of the model when retreived)
                return TransformPlatformUserIdentityToPlatformUser(GetUserIdentity(userId));
            }

        }


        public static PlatformUserIdentity GetUserIdentity(string userNameOrId)
        {
            var db = new PlatformUserIdentityDbContext();

            PlatformUserIdentity user = null;

            #region Get User From DB/Entity

            try
            {
                try
                {
                    user = db.Users.FirstOrDefault(u => u.Id == userNameOrId);
                }
                catch
                {
                    //Get by UserName
                    user = db.Users.FirstOrDefault(u => u.UserName == userNameOrId);
                }

                if (user == null)
                {
                    //Get by UserName
                    user = db.Users.FirstOrDefault(u => u.UserName == userNameOrId);
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to get platform user identity for " + userNameOrId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );
                //user = null;
            }

            if (user != null)
            {
                //Store user into Cache as Non Identity version
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
                try
                {
                    cache.HashSet(UserHash.Key(user.Id.ToString()), UserHash.Fields.Model(), JsonConvert.SerializeObject(TransformPlatformUserIdentityToPlatformUser(user)), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }
            }

            #endregion

            return user;
        }

        public static List<PlatformUser> GetUsers(bool useCachedVersion = true)
        {
            var users = new List<PlatformUser>();

            List<PlatformUser> platformUsersCache = null;
            //DataCache dataCache = new DataCache(platformUserListCacheName);
            //string cacheID = PlatformUserCacheID.PlatformUserList();
            
            if(useCachedVersion)
            {
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    var redisValue = cache.HashGet(PlatformHash.Key, PlatformHash.Fields.Users); 
                    if (redisValue.HasValue)
                    {
                        platformUsersCache = JsonConvert.DeserializeObject<List<PlatformUser>>(redisValue);
                    }

                    //platformUsersCache = JsonConvert.DeserializeObject<List<PlatformUser>>(cache.HashGet(PlatformHash.Key, PlatformHash.Fields.Users));
                    //platformUsersCache = dataCache.Get(cacheID);
                }
                catch
                {

                }
            }

            if(platformUsersCache == null)
            {
                var db = new PlatformUserIdentityDbContext();
                var identityUsers = db.Users.ToList();

                foreach (PlatformUserIdentity identityUser in identityUsers)
                {
                    users.Add(TransformPlatformUserIdentityToPlatformUser(identityUser));
                }

                if (users != null)
                {
                    //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
                    //IDatabase cache = con.GetDatabase();
                    //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                    IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                    try
                    {
                        cache.HashSet(PlatformHash.Key, PlatformHash.Fields.Users, JsonConvert.SerializeObject(users), When.Always, CommandFlags.FireAndForget);
                        //dataCache.Put(cacheID, users);
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                users = platformUsersCache;
            }

            return users;
        }




        public static DataAccessResponseType DeleteUser(string userId)
        {
            
            var response = new DataAccessResponseType();

            var identityUser = GetUserIdentity(userId);

            if (identityUser == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "User with that ID does not exist";
            }
            else
            {
                //nvalidate Cache(s)
                InvalidatePlatformUsersListCache();
                InvalidateUserRoleCache(identityUser.Id.ToString());

                // Remove user from all associated roles:
                Sql.Statements.DeleteStatements.DeleteUserRoles(identityUser.Id.ToString());

                response.isSuccess = DeleteStatements.DeleteUser(userId);

                if (response.isSuccess)
                {

                    DeleteUserHashReference(userId);
                    GetUserIdentity(userId); //<-- we run this to update cache
                    //UpdatePlatformUserHashReferenceInCache(userForCache);


                    //Clear/Update Cache(s):
                    //DeletePlatformUsersIdentityReferencesInCache(identityUser);
                    DeleteAllUserCacheReferences(identityUser);
                    InvalidateUserRoleCache(userId);
                    //InvalidatePlatformUsersListCache();
                }
            }

            return response;
        }

        #endregion

        #region Create Users & Manage Invitations

        /// <summary>
        /// Used by the platform to invite users, users will then varify and create password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public static DataAccessResponseType InviteUser(string email, string firstName, string lastName, string roleName)
        {
            var result = new DataAccessResponseType();

            //Validate all varialbles
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName) || String.IsNullOrEmpty(roleName))
            {
                result.isSuccess = false;
                result.ErrorMessage = "Not all parameters contain a value.";
                return result;
            }

            //Validate UserName(Email)
            var emailValidation = ValidationManager.IsValidEmail(email);
            if (!emailValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = emailValidation.validationMessage;
                return result;
            }

                //Validate that email does not alraedy exist as a user on the platform
                if (GetUserIdentity(email) != null)
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "This email is registered to another user on the platform";
                    return result;
                }

                //Verify email does not already exist in the invitation list 
                if (PlatformInvitationsManager.DoesEmailExist(email))
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "This email address has a pending invite.";
                    return result;
                }


                string invitationKey = String.Empty;

                //Store user in invited users table
                try
                {
                    invitationKey = PlatformInvitationsManager.StoreInvitedUser(email, firstName, lastName, roleName);
                }
                catch (Exception e)
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to invite a platform user with email: " + email,
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );

                    result.isSuccess = false;
                    result.ErrorMessage = e.Message;
                    return result;
                }


                //Send invite an email (or override for tests)
                bool emailSent;
                try
                {
                    emailSent = EmailInvitation(email, firstName, invitationKey);
                }
                catch
                {
                    emailSent = false;
                }

                if (!emailSent)
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "Couldn't send an invite to this email, please try again.";
                    //Clear the invite
                    DeleteInvitation(invitationKey);
                    //return the failure
                    return result;
                }


                result.isSuccess = true;
                result.SuccessMessage = invitationKey;
            

            return result;
        }

        //leave out invitation key if you are bypassing invitation/verification
        public static DataAccessResponseType CreatePlatformUser(string email, string firstName, string lastName, string password, string roleName, string invitationKey = null)
        {
            DataAccessResponseType result = new DataAccessResponseType();

            //Verify Input
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(roleName))
            {
                result.isSuccess = false;
                result.ErrorMessage = "Not all inputs contain a value.";
                return result;
            }

            if (invitationKey != null)
            {
                //Make sure invitation is still valid if passing in an invitationKey:
                var invitation = PlatformInvitationsManager.GetInvitedUser(invitationKey);
                if (invitation == null)
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "This invitation is no longer valid";
                    return result;
                }
            }

            var user = new PlatformUserIdentity
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedDate = DateTime.UtcNow,
                Active = true    
            };

            //Set result to success before validating
            result.isSuccess = true;

            //Validate Email
            var emailValidation = ValidationManager.IsValidEmail(user.Email);
            if (!emailValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = emailValidation.validationMessage;
                result.ErrorMessages.Add(emailValidation.validationMessage);
            }

            //Validate Email/UserName Unique
            if (GetUserIdentity(email) != null)
            {
                result.isSuccess = false;
                result.ErrorMessage = "This email is already registered.";

                result.ErrorMessages.Add("This email is already registered.");
            }

            //Validate Password
            var passwordValidation = ValidationManager.IsValidPlatformUserPassword(password);
            if (!passwordValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = passwordValidation.validationMessage;

                result.ErrorMessages.Add(passwordValidation.validationMessage);
            }

            //Validate FirstName
            var firstNameValidation = ValidationManager.IsValidFirstName(firstName);
            if (!firstNameValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = firstNameValidation.validationMessage;

                result.ErrorMessages.Add(firstNameValidation.validationMessage);
            }

            //Validate LastName
            var lastNameValidation = ValidationManager.IsValidLastName(lastName);
            if (!lastNameValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = lastNameValidation.validationMessage;

                result.ErrorMessages.Add(lastNameValidation.validationMessage);
            }

            //If no validations pass, return the result with error messages:
            if (!result.isSuccess)
            {
                return result;
            }

            //Validation passed, Create User:
            var identityResult = CreateUserInternal(user, password);

            if (!identityResult.Succeeded)
            {
                //Creation of user failed
                result.isSuccess = false;

                result.ErrorMessages = identityResult.Errors.ToList();
            }
            else
            {
                //Creation of user succeeded
                result.isSuccess = true;

                //Assign user to role:
                PlatformUserManager.UpdateUserRole(user.Id.ToString(), roleName);

                //Clear cache for the accounts list
                InvalidatePlatformUsersListCache();

                if (invitationKey != null)
                {
                    //Clear the invitation from table storage:
                    var clearRecord = PlatformInvitationsManager.DeleteInvitedUser(invitationKey);
                }
            }




            return result;
        }
        
        public static PlatformInvitation GetInvitation(string invitationKey)
        {
            var userInvitationTableEntity = PlatformInvitationsManager.GetInvitedUser(invitationKey);
            var platformInvitation = Transformations.TransformToPlatformInvitation(userInvitationTableEntity);

            return platformInvitation;
        }

        public static List<PlatformInvitation> GetInvitations()
        {
            var results = new List<PlatformInvitation>();

            foreach (var userInvitation in PlatformInvitationsManager.GetInvitedUsers())
            {
                results.Add(Transformations.TransformToPlatformInvitation(userInvitation));
            }

            var result = PlatformInvitationsManager.GetInvitedUsers();

            return results;
        }

        public static DataAccessResponseType ResendInvitation(string invitationKey)
        {
            var response = new DataAccessResponseType();

            try
            {
                var invitation = GetInvitation(invitationKey);

                EmailInvitation(invitation.Email, invitation.FirstName, invitationKey);

                response.isSuccess = true;
            }
            catch(Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to resend a platform user invitation for: " + invitationKey,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }
            return response;
        }

        public static DataAccessResponseType DeleteInvitation(string invitationKey)
        {
            var result = new DataAccessResponseType();

            result.isSuccess = PlatformInvitationsManager.DeleteInvitedUser(invitationKey);

            return result;
        }


        #endregion

        #region Update

        public static DataAccessResponseType UpdateFullName(string userId, string newFirstName, string newLastName)
        {
            var response = new DataAccessResponseType();


            //Verify Input
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(newFirstName) || String.IsNullOrEmpty(newLastName))
            {
                response.isSuccess = false;
                response.ErrorMessage = "Not all inputs contain a value.";
                return response;
            }

            //Verify change
            var userCheck = GetUser(userId);
            if (userCheck.FirstName == newFirstName && userCheck.LastName == newLastName)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This is already the users name.";
                return response;
            }


            //Validate name(s)
            var fNameValidation = ValidationManager.IsValidFirstName(newFirstName);
            var lNameValidation = ValidationManager.IsValidLastName(newLastName);

            if (!fNameValidation.isValid && !lNameValidation.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This is not a valid name.";
                response.ErrorMessages.Add(fNameValidation.validationMessage);
                response.ErrorMessages.Add(lNameValidation.validationMessage);
                return response;

            }
            else if (!fNameValidation.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = fNameValidation.validationMessage;
                response.ErrorMessages.Add(fNameValidation.validationMessage);
                return response;
            }
            else if (!lNameValidation.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = lNameValidation.validationMessage;
                response.ErrorMessages.Add(lNameValidation.validationMessage);
                return response;
            }

            response.isSuccess = Sql.Statements.UpdateStatements.UpdateUserFullName(userId, newFirstName, newLastName);

            if (response.isSuccess)
            {
                //var user = GetUserIdentity(userId, false);
                DeleteUserHashReference(userId);
                GetUserIdentity(userId); //<-- we run this to update cache
                //UpdatePlatformUserHashReferenceInCache(userForCache);
                InvalidatePlatformUsersListCache();
                //DeletePlatformUsersIdentityReferencesInCache(user);
                //UpdatePlatformUserIdentityReferencesInCache(user);
            }

            return response;
        }

        public static DataAccessResponseType UpdateEmail(string userId, string email)
        {
            var response = new DataAccessResponseType();

            //Verify Input
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(userId))
            {
                response.isSuccess = false;
                response.ErrorMessage = "Not all inputs contain a value.";
                return response;
            }

            //Verify that this is a different email before performing the transaction
            var userCheck = GetUser(userId);
            if (userCheck.Email == email)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This user is already assigned this email address.";
                return response;
            }

            //Validate Email
            var emailValidation = ValidationManager.IsValidEmail(email);
            if (!emailValidation.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = emailValidation.validationMessage;

                response.ErrorMessages.Add(emailValidation.validationMessage);

                return response;
            }

            response.isSuccess = Sql.Statements.UpdateStatements.UpdateUserEmail(userId, email);

            if (response.isSuccess)
            {

                DeleteUserHashReference(userId);
                GetUserIdentity(userId); //<-- we run this to update cache
                //UpdatePlatformUserHashReferenceInCache(userForCache);


                //var user = GetUserIdentity(userId, false);
                //DeletePlatformUsersIdentityReferencesInCache(user);
                //Also delete the prevous GlobalUniqueUserName from the cache:
                //DeleteGlobalUserNameReferencesInCache(userCheck.UserName);
                InvalidatePlatformUsersListCache();
                //UpdatePlatformUserIdentityReferencesInCache(user);
            }

            return response;
        }

        public static DataAccessResponseType UpdateProfilePhoto(string userId, byte[] byteArray)
        {
            var response = new DataAccessResponseType();

            //Verify, Edit & Store the image:
            var profilePhotoProcessor = new Imaging.PlatformUserProfilePhotoProcessor();
            response = profilePhotoProcessor.ProcessPlatformUserProfilePhoto(byteArray);

            if(!response.isSuccess)
            {
                return response; //<--return error
            }

            //Update ImageID of Photo for user:
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateUserPhoto(userId, response.SuccessMessage);

            if (response.isSuccess)
            {
                //Invalidate Caches:
                DeleteUserHashReference(userId);
                GetUserIdentity(userId); //<-- we run this to update cache
                InvalidatePlatformUsersListCache();
            }
            else
            {
                response.ErrorMessage = "Photo was uploaded, but we ran into an issue when updating the user. Please try again.";
            }

            return response;
        }

        #endregion

        #region Passwords

        #region Reset Lost Passwords

        /// <summary>
        /// Sends an email to a user claiming a lost password with a PasswordClaimKey that allows for a password reset
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static DataAccessResponseType ClaimLostPassword(string email)
        {
            var result = new DataAccessResponseType();

            //verify that the user  exists:
            var user = GetUserIdentity(email);

            if (user == null)
            {
                string errorMessage = "This user does not exist.";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }

            if (user.Active == false)
            {
                string errorMessage = "The user is inactive.";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }

            //Delete any existing claims on this email for this account
            var passwordClaimKey = PasswordClaimManager.StorePasswordClaim(email);

            //Send email to user to reset password:
            EmailPasswordClaim(email, user.FirstName, passwordClaimKey);

            result.isSuccess = true;
            result.SuccessMessage = "An email has been sent to the address for this user along with a link to reset the password.";

            return result;
        }

        public static List<PlatformPasswordResetClaim> GetPasswordClaims()
        {
            //Request approved, make the request:
            var results = new List<PlatformPasswordResetClaim>();

            foreach (var passwordClaim in PasswordClaimManager.GetPasswordClaims())
            {
                results.Add(Transformations.TransformToPasswordResetClaim(passwordClaim));
            }

            return results;
        }

        public static PasswordClaimTableEntity GetPasswordClaim(string passwordClaimKey)
        {
            return PasswordClaimManager.GetPasswordClaim(passwordClaimKey);
        }

        /// <summary>
        /// used by password reset link, sent to user that created claim
        /// </summary>
        /// <param name="resetPassword"></param>
        /// <param name="confirmResetPassword"></param>
        /// <returns></returns>
        public static DataAccessResponseType ResetPassword(string passwordClaimKey, string resetPassword)
        {

            var result = new DataAccessResponseType();


            if(resetPassword.Length < Sahara.Core.Settings.Platform.Users.Authentication.PasswordMinimumLength)
            {
                result.isSuccess = false;
                result.SuccessMessage = "New password must be at least " + Sahara.Core.Settings.Platform.Users.Authentication.PasswordMinimumLength + "characters in length.";

                return result;
            }

            try
            {
                //Get the claim:
                var passwordClaim = PasswordClaimManager.GetPasswordClaim(passwordClaimKey);

                //Update the user password:

                UpdatePassword(passwordClaim.Email, resetPassword);

                //Delete the claim:
                PasswordClaimManager.DeletePasswordClaim(passwordClaimKey);

                result.isSuccess = true;
                result.SuccessMessage = "Your password has been updated, please log into your account to verify.";

                return result;

            }
            catch
            {
                string errorMessage = "There was an error verifying the password claim, or claim does not exist";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }


        }


        #endregion

        #region Change Password

        /// <summary>
        /// Used by users who have their password after authentication
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <param name="confirmNewPassword"></param>
        /// <returns></returns>
        public static DataAccessResponseType ChangePassword(string email, string currentPassword, string newPassword)
        {
            var result = new DataAccessResponseType();

            //Verify password is valid
            var passwordValidation = ValidationManager.IsValidAccountUserPassword(newPassword);
            if (!passwordValidation.isValid)
            {
                string errorMessage = "Password must be at least " + Sahara.Core.Settings.Platform.Users.Authentication.PasswordMinimumLength + " characters in length";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }



            //Verify password by logging user in:

           // string globalUniqueUserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, accountID);

            var response = PlatformUserManager.GetUserWithLogin(email, currentPassword);

            if (!response.isSuccess)
            {
                string errorMessage = "Current password is incorrect.";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }


            //Update the user password:

            result.isSuccess = UpdatePassword(email, newPassword);

            if (result.isSuccess)
            {
                result.SuccessMessage = "Your password has been updated.";
            }
            else
            {
                result.ErrorMessage = "An error occured while updating the password.";
            }

            return result;

        }

        #endregion

        #region Private Password Methods

        private static bool UpdatePassword(string email, string newPassword)
        {
            try
            {

                UserStore<PlatformUserIdentity> store = new UserStore<PlatformUserIdentity>(new PlatformUserIdentityDbContext());

                var userManager = new UserManager<PlatformUserIdentity>(
                new UserStore<PlatformUserIdentity>(new PlatformUserIdentityDbContext()));

                String hashedNewPassword = userManager.PasswordHasher.HashPassword(newPassword);

                PlatformUserIdentity user = store.FindByNameAsync(email).Result;

                store.SetPasswordHashAsync(user, hashedNewPassword);
                store.UpdateAsync(user);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region Roles

        public static string GetUserRole(string userId, bool useCachedVersion = true)
        {
            string role = string.Empty;

            string platformUserRoleCache = null;

            if (useCachedVersion)
            {
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                //string redisKey = UserRoleKeys.UserRole(userId);

                try
                {
                    platformUserRoleCache = cache.HashGet(UserHash.Key(userId), UserHash.Fields.Role());
                    //con.Close();
                }
                catch
                {

                }
            }

            if (platformUserRoleCache == null)
            {

                var userManager = new UserManager<PlatformUserIdentity>(
                        new UserStore<PlatformUserIdentity>(new PlatformUserIdentityDbContext()));

                //Allows for use of email address as username:
                userManager.UserValidator = new UserValidator<PlatformUserIdentity>(userManager) { AllowOnlyAlphanumericUserNames = false };

                //var test = userManager.GetRoles(userId);
                try
                {
                    role = userManager.GetRoles(userId).ToList()[0]; //<-- Only one role per user should exist

                    //cache or re-cache:
                    //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
                    //IDatabase cache = con.GetDatabase();
                    //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                    IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                    try
                    {
                        //string redisKey = UserRoleKeys.UserRole(userId);
                        cache.HashSet(UserHash.Key(userId), UserHash.Fields.Role(), role);

                        //con.Close();
                    }
                    catch
                    {

                    }
                }
                catch
                {
                    role = null;
                }

            }
            else
            {
                role = platformUserRoleCache;
            }

            return role;
        }

       
        public static bool InvalidateUserRoleCache(string userId)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                //Clear the enitre user hash
                cache.KeyDelete(UserHash.Key(userId), CommandFlags.FireAndForget);

                //con.Close();
            }
            catch
            {

            }

            return true;
        } 



        public static async Task<bool> CreateRoles(List<string> roles)
        {
            var roleManager = new RoleManager<IdentityRole>(
                 new RoleStore<IdentityRole>(new PlatformUserIdentityDbContext()));

            var idResult = new IdentityResult();

            foreach (string role in roles)
            {
                idResult = await roleManager.CreateAsync(new IdentityRole(role));
            }

            return idResult.Succeeded;
        }


        public static DataAccessResponseType UpdateUserRole(string userId, string roleName)
        {
            var response = new DataAccessResponseType();

            var currentUserRole = GetUserRole(userId);

            //Verify Input
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(roleName))
            {
                response.isSuccess = false;
                response.ErrorMessage = "Not all inputs contain a value.";
                return response;
            }

            //verify user is not already set to this role before performing transaction:
            if (currentUserRole == roleName)
            {
                // Send Transaction Error Message
                response.isSuccess = false;
                response.ErrorMessage = "User is already assigned to this role.";

                return response;
            }


            if (roleName != Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin)
            {
                //Only perform if we are setting a platform user to something other than SuperAdmin & the current user IS both Active and a SuperAdmin
                if (currentUserRole == Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin && GetUser(userId, false).Active)
                {
                    //If we are downgrading a SuperAdmin then we must verify that at least one other SuperAdmin exists that is active before updating the status (Platform must always have at least ONE active SuperAdmin)
                    if (Sql.Statements.SelectStatements.SelectActiveSuperAdminCount() <= 1)
                    {
                        // Send Transaction Error Message
                        response.isSuccess = false;
                        response.ErrorMessage = "You must have at least one active SuperAdmin on the platform!";

                        return response;
                    }
                }
            }

            var userManager = new UserManager<PlatformUserIdentity>(
                new UserStore<PlatformUserIdentity>(new PlatformUserIdentityDbContext()));

            //Allows for use of email address as username:
            userManager.UserValidator = new UserValidator<PlatformUserIdentity>(userManager) { AllowOnlyAlphanumericUserNames = false };


            //Clear user of all roles (Users are only ever in ONE role at a time):
            Sql.Statements.DeleteStatements.DeleteUserRoles(userId);

            //Add user to role
            var result = userManager.AddToRole(userId, roleName);
            response.isSuccess = result.Succeeded;

            if (response.isSuccess)
            {

                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    //string redisKey = UserRoleKeys.UserRole(userId);
                    cache.HashSet(UserHash.Key(userId), UserHash.Fields.Role(), roleName);

                    //Inalidate/Update user role in cache
                    InvalidatePlatformUsersListCache();
                }
                catch
                {

                }


            }
            else
            {
                try
                {
                    response.ErrorMessage = result.Errors.ToList()[0];
                }
                catch
                {

                }
                try
                {
                    response.ErrorMessages = result.Errors.ToList();
                }
                catch
                {

                }
            }

            return response;
        }

        

        #endregion

        #region Status

        public static DataAccessResponseType UpdateActiveState(string userId, bool isActive)
        {
            var result = new DataAccessResponseType();

            //Verify Input
            if (String.IsNullOrEmpty(userId))
            {
                result.isSuccess = false;
                result.ErrorMessage = "Not all inputs contain a value.";
                return result;
            }


            var user = GetUser(userId);

            //Verify that the user is not already set to requested active state
            if(user.Active == isActive)
            {
                // Send Transaction Error Message
                result.isSuccess = false;
                result.ErrorMessage = "This user is already set to " + isActive + ".";

                return result;
            }


            if (!isActive)
            {
                //Only perform if we are setting a platform user to inactive
                if (user.Role == Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin)
                {
                    //If we are making a SuperAdmin inactive then we verify that another SuperAdmin exists that is active before updating the status (Platform must always have at least ONE active SuperAdmin)
                    if (Sql.Statements.SelectStatements.SelectActiveSuperAdminCount() <= 1)
                    {
                        // Send Transaction Error Message
                        result.isSuccess = false;
                        result.ErrorMessage = "You must have at least one active SuperAdmin on the platform!";

                        return result;
                    }
                }
            }


            //Make the update
            result.isSuccess = Sql.Statements.UpdateStatements.UpdateUserActiveState(userId, isActive).isSuccess;

            if (result.isSuccess)
            {
                DeleteUserHashReference(userId);

                var userForCache = GetUser(userId, false);
                UpdatePlatformUserHashReferenceInCache(userForCache);

                //var userNonCached = GetUserIdentity(userId, false);
                //DeletePlatformUsersIdentityReferencesInCache(userNonCached);
                InvalidatePlatformUsersListCache();
                //UpdatePlatformUserIdentityReferencesInCache(userNonCached);
            }

            return result;
        }

        #endregion


        #region Internal Cache Processing

        internal static void UpdatePlatformUserHashReferenceInCache(PlatformUser platformUser)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.HashSet(UserHash.Key(
                    platformUser.Id.ToString()),
                    UserHash.Fields.Model(),
                    //JsonConvert.SerializeObject(platformUser),
                    JsonConvert.SerializeObject(platformUser),
                    When.Always,
                    CommandFlags.FireAndForget);
            }
            catch
            {

            }

        }

        internal static void DeleteUserHashReference(string userId)
        {

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                //Delete the entire Hash for this user
                cache.KeyDelete(UserHash.Key(userId), CommandFlags.FireAndForget);
            }
            catch
            {

            }

        }

        internal static void DeleteAllUserCacheReferences(PlatformUserIdentity platformUserIdentity)
        {

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                //Delete the entire Hash for this user
                cache.KeyDelete(UserHash.Key(platformUserIdentity.Id.ToString()), CommandFlags.FireAndForget);

                //Delete the id lookup table
                cache.HashDelete(UserIdByUserNameHash.Key, UserIdByUserNameHash.Fields.UserId(platformUserIdentity.UserName), CommandFlags.FireAndForget);
            }
            catch
            {

            }
        }

        /*
        public static bool UpdateUserRoleInCache(string userId, string roleName)
        {
            DataCache dataCache = new DataCache(platformUserRoleCacheName);
            string cacheID = PlatformUserRoleCacheID.PlatformUserRole(userId);
            dataCache.Put(cacheID, roleName);

            return true;
        }

       */
        public static bool InvalidatePlatformUsersListCache()
        {

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.HashDelete(
                    PlatformHash.Key,
                    PlatformHash.Fields.Users,
                    CommandFlags.FireAndForget
                );
            }
            catch
            {

            }

            return true;
        }
        
        /*
        internal static void UpdatePlatformUserIdentityReferencesInCache(PlatformUserIdentity platformUserIdentity)
        {

            DataCache accountCache = new DataCache(platformUserCacheName);

            //Store both ways with all account options
            accountCache.Put(PlatformUserCacheID.PlatformUserIdentity(platformUserIdentity.Id.ToString()), platformUserIdentity);
            accountCache.Put(PlatformUserCacheID.PlatformUserIdentity(platformUserIdentity.UserName), platformUserIdentity);

            //Store as non entity version for external WCF clients
            accountCache.Put(PlatformUserCacheID.PlatformUser(platformUserIdentity.Id.ToString()), TransformPlatformUserIdentityToPlatformUser(platformUserIdentity));

        }*/
        /*
        internal static void DeletePlatformUsersIdentityReferencesInCache(PlatformUserIdentity platformUserIdentity)
        {
            DataCache accountCache = new DataCache(platformUserCacheName);

            //Remove account user in all possble ways with all options and in lists
            accountCache.Remove(PlatformUserCacheID.PlatformUserIdentity(platformUserIdentity.Id.ToString()));
            accountCache.Remove(PlatformUserCacheID.PlatformUserIdentity(platformUserIdentity.UserName));

            //Remove as non entity version for external WCF clients
            accountCache.Remove(PlatformUserCacheID.PlatformUser(platformUserIdentity.Id.ToString()));
        }

        internal static void DeleteGlobalUserNameReferencesInCache(string userName)
        {
            DataCache accountCache = new DataCache(platformUserCacheName);

            accountCache.Remove(PlatformUserCacheID.PlatformUserIdentity(userName));
        }
         */

        #endregion

        #region Private Methods

        public static PlatformUser TransformPlatformUserIdentityToPlatformUser(PlatformUserIdentity platformUserIdentity)
        {
            var platformUser = new PlatformUser();

            platformUser.Photo = platformUserIdentity.Photo;
            platformUser.Active = platformUserIdentity.Active;
            platformUser.FirstName = platformUserIdentity.FirstName;
            platformUser.LastName = platformUserIdentity.LastName;
            platformUser.UserName = platformUserIdentity.UserName;
            platformUser.Id = platformUserIdentity.Id;
            platformUser.CreatedDate = platformUserIdentity.CreatedDate;
            platformUser.Email = platformUserIdentity.Email;

            platformUser.Role = GetUserRole(platformUserIdentity.Id);

            platformUser.FullName = platformUserIdentity.FullName;

            return platformUser;
        }

        /*
        public static PlatformUserIdentity TransformPlatformUserToPlatformUserIdentity(PlatformUser platformUserIdentity)
        {
            var platformUserIdentity = new PlatformUserIdentity();

            platformUser.Active = platformUserIdentity.Active;
            platformUser.FirstName = platformUserIdentity.FirstName;
            platformUser.LastName = platformUserIdentity.LastName;
            platformUser.UserName = platformUserIdentity.UserName;
            platformUser.Id = platformUserIdentity.Id;
            platformUser.CreatedDate = platformUserIdentity.CreatedDate;
            platformUser.Email = platformUserIdentity.Email;

            platformUserIdentity.Role = GetUserRole(platformUserIdentity.Id);

            return platformUserIdentity;
        }*/

        private static bool EmailInvitation(string email, string firstName, string invitationKey)
        {

            EmailManager.Send(
                email,
                Settings.Endpoints.Emails.FromPlatform,
                Sahara.Core.Settings.Application.Name,
                String.Format(Settings.Copy.EmailMessages.PlatformInvitation.Subject, Settings.Application.Name),
                String.Format(Settings.Copy.EmailMessages.PlatformInvitation.Body, firstName, invitationKey),
                true
            );

            return true;
        }


        private static bool EmailPasswordClaim(string email, string firstName, string passwordClaimKey)
        {

            EmailManager.Send(
                email,
                Settings.Endpoints.Emails.FromPlatform,
                Sahara.Core.Settings.Application.Name,
                String.Format(Settings.Copy.EmailMessages.PlatformPasswordClaim.Subject),
                String.Format(Settings.Copy.EmailMessages.PlatformPasswordClaim.Body, firstName, passwordClaimKey),
                true
            );

            return true;
        }

        #endregion
    }
}
