using System;
using System.Text.RegularExpressions;
using Sahara.Core.Common.Validation.ResponseTypes;
using Sahara.Core.Common.Validation.Sql.Statements;

namespace Sahara.Core.Common.Validation
{
    public static class ValidationManager
    {

        #region Accounts & Users

        /// <summary>
        /// method for determining that provided email address is valid
        /// </summary>
        /// <param name="email">email address to validate</param>
        /// <returns>true is valid, false if not valid</returns>
        public static ValidationResponseType IsValidEmail(string email)
        {
            ValidationResponseType response = new ValidationResponseType();

            //regular expression pattern for valid email
            //addresses, allows for the following domains:
            //com,edu,info,gov,int,mil,net,org,biz,name,museum,coop,aero,pro,tv
            string pattern = @"^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\.[-.a-zA-Z0-9]+)*\.(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2}|[a-zA-Z]{3}|[a-zA-Z]{4})$";
            //Regular expression object
            Regex check = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            //boolean variable to return to calling method
            response.isValid = false;


            //make sure an email address was provided
            if (string.IsNullOrEmpty(email))
            {
                response.isValid = false;
            }
            else
            {
                //use IsMatch to validate the address
                response.isValid = check.IsMatch(email);
            }
            //return the value to the calling method

            //Check if additional invalid characters exist
            /*
            if(email.Contains(")") || email.Contains("(") || email.Contains(""))
            {

            }*/


            if (!response.isValid)
            {
                response.validationMessage = "You must use a valid email address.";
                return response;
            }
            else
            {
                response.isValid = true;
                response.validationMessage = "Email is valid!";
                return response;
            }


        }

        public static ValidationResponseType IsValidPhoneNumber(string phoneNumber)
        {
            ValidationResponseType response = new ValidationResponseType();


            //string pattern = @"(\([2-9]\d\d\)|[2-9]\d\d) ?[-.,]? ?[2-9]\d\d ?[-.,]? ?\d{4}";
            //Regex check = new Regex(pattern);
            

            if (phoneNumber == null)
            {
                response.isValid = false;
                response.validationMessage = "Phone number cannot be blank.";

                return response;
            }
            if (phoneNumber.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Length < 10)
            {
                response.isValid = false;
                response.validationMessage = "Please include area code or country code.";

                return response;
            }
            if (phoneNumber.Length > 28)
            {
                response.isValid = false;
                response.validationMessage = "Phone number is too long.";

                return response;
            }
            //if (!check.IsMatch(phoneNumber.Replace(" ", "").Replace("-", "").Replace(".", "")))
            //{
                //response.isValid = false;
                //response.validationMessage = "Phone number is not valid. Please make sure you include area code and/or country codes.";

                //return response;
            //}
            else
            {
                response.isValid = true;
            }


            return response;
        }

        public static ValidationResponseType IsValidAccountName(string AccountName)
        {
            AccountName = AccountName.Trim(); //<---We trim here, since registration will also trim

            ValidationResponseType response = new ValidationResponseType();

            //Check if name is null
            if (AccountName == null || AccountName == String.Empty)
            {
                response.isValid = false;
                response.validationMessage = "Account name is blank.";

                return response;
            }

            //Check if Accounts Name is a system reserved name:
            foreach (string reservedName in Settings.Accounts.Registration.ReservedAccountNames)
            {
                if (Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(AccountName) == reservedName)
                {
                    response.isValid = false;
                    response.validationMessage = AccountName + " is a reserved name.";

                    return response;
                }
            }
            //Check if name has trailing whitespace(s)
            if (Regex.IsMatch(AccountName, @"\s+$"))
            {
                response.isValid = false;
                response.validationMessage = "Name cannot contain empty spaces at the end.";

                return response;
            }

            //Check if name has whitespace(s) at front
            if (AccountName[0].ToString() == " ")
            {
                response.isValid = false;
                response.validationMessage = "Name cannot contain an empty space as the first character.";

                return response;
            }

            //Check if the character length conforsms to policy
            if (AccountName.Length < Settings.Accounts.Registration.AccountNameMinimumLength || AccountName.Length > Settings.Accounts.Registration.AccountNameMaximunLength)
            {
                response.isValid = false;
                response.validationMessage = "Name must be between " + Settings.Accounts.Registration.AccountNameMinimumLength + "-" + Settings.Accounts.Registration.AccountNameMaximunLength + " characters long.";

                return response;
            }
            //We allow for spaces, dashes, ampersands and apostrophes, so we remove them before doing a regex for other special characters 
            else if ((Regex.IsMatch(Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(AccountName), "^[a-zA-Z0-9\\-]*$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "Name cannot contain special characters";

                return response;
            }
            else
            {
                //Check if AccountName Exists ====================    
                SqlBoolStatements sqlBoolStatements = new SqlBoolStatements();
                bool accountExists = sqlBoolStatements.AccountNameExists(AccountName);

                if (accountExists)
                {
                    response.isValid = false;
                    response.validationMessage = "Account with that name already exists.";

                    return response;
                }
                else
                {
                    response.isValid = true;
                }
            }

            //check if subdomain will be less than minimum character length after shortening (to avoid names like "--T--" being able to create subdomains like "t")
            if (Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(AccountName).Length < Settings.Accounts.Registration.AccountNameMinimumLength)
            {
                response.isValid = false;
                response.validationMessage = "Name must have at least " + Settings.Accounts.Registration.AccountNameMinimumLength + " alphanumeric characters.";

                return response;
            }


            return response;
        }

        public static ValidationResponseType IsValidFirstName(string firstName)
        {
            ValidationResponseType response = new ValidationResponseType();

            if (firstName == null || firstName == "")
            {
                response.isValid = false;
                response.validationMessage = "First name cannot be blank.";

                return response;
            }
            if (firstName.Length < Settings.Accounts.Registration.UserFirstNameMinimumLength || firstName.Length > Settings.Accounts.Registration.UserFirstNameNameMaximunLength)
            {
                response.isValid = false;
                response.validationMessage = "First name must be between " + Settings.Accounts.Registration.UserFirstNameMinimumLength + "-" + Settings.Accounts.Registration.UserFirstNameNameMaximunLength + " characters in length.";

                return response;
            }
            else if (firstName.Contains(" ")) //|| Disallow special characters: (Regex.IsMatch(firstName, "^[a-zA-Z0-9 ]*$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "First name cannot contain spaces."; // or special charcters.";

                return response;
            }
            else
            {
                response.isValid = true;
            }


            return response;
        }

        public static ValidationResponseType IsValidLastName(string lastName)
        {
            ValidationResponseType response = new ValidationResponseType();

            if (lastName == null || lastName == "")
            {
                response.isValid = false;
                response.validationMessage = "Last name cannot be blank.";

                return response;
            }
            if (lastName.Length < Settings.Accounts.Registration.UserLastNameMinimumLength || lastName.Length > Settings.Accounts.Registration.UserLastNameNameMaximunLength)
            {
                response.isValid = false;
                response.validationMessage = "Last name must be between " + Settings.Accounts.Registration.UserLastNameMinimumLength + "-" + Settings.Accounts.Registration.UserLastNameNameMaximunLength + " characters in length.";

                return response;
            }
            else if (lastName.Contains(" ")) //|| Disallow special characters: (Regex.IsMatch(lastName, "^[a-zA-Z0-9 ]*$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "Last name cannot contain spaces."; // or special charcters.";

                return response;
            }
            else
            {
                response.isValid = true;
            }


            return response;
        }

        /*
        public static ValidationResponseType IsUniqueAccountUserDisplayName(DatabaseConnections databaseConnections, Guid AccountID, string DisplayName)
        {
            ValidationResponseType response = new ValidationResponseType();

            //TODO: Check if another user in an account is already using this DisplayName
            return response;

        }*/

        public static ValidationResponseType IsValidAccountUserPassword(string Password)
        {
            ValidationResponseType response = new ValidationResponseType();

            if (Password.Length < Settings.Accounts.Registration.PasswordMinimumLength)
            {
                response.isValid = false;
                response.validationMessage = "Password must be at least " + Settings.Accounts.Registration.PasswordMinimumLength + " characters in length.";

                return response;
            }
            else
            {
                response.isValid = true;
            }


            return response;
        }

        public static ValidationResponseType IsValidPlatformUserPassword(string Password)
        {
            ValidationResponseType response = new ValidationResponseType();

            if (Password.Length < Settings.Platform.Users.Authentication.PasswordMinimumLength)
            {
                response.isValid = false;
                response.validationMessage = "Password must be at least " + Settings.Platform.Users.Authentication.PasswordMinimumLength + " characters in length.";

                return response;
            }
            else
            {
                response.isValid = true;
            }


            return response;
        }

        #endregion

        #region Payment Plans 

        public static ValidationResponseType IsValidPaymentPlanName(string PlanName)
        {
            ValidationResponseType response = new ValidationResponseType();

            //Check if name is null
            if (PlanName == null || PlanName == String.Empty)
            {
                response.isValid = false;
                response.validationMessage = "Plan name is blank.";

                return response;
            }

            //Check if name has any spaces
            if (PlanName.Contains("  "))
            {
                response.isValid = false;
                response.validationMessage = "Plan name cannot contain any double empty spaces";

                return response;

            }

            //Check if the character length conforsms to policy
            if (PlanName.Length < 2 || PlanName.Length > 25)
            {
                response.isValid = false;
                response.validationMessage = "Plan name must be between " + 2 + "-" + 35 + " characters long.";

                return response;
            }
            //We allow for spaces, dashes, ampersands and apostrophes, so we remove them before doing a regex for other special characters
            else if ((Regex.IsMatch(PlanName.Replace("(", "").Replace(")", "").Replace(" ", ""), @"^[a-zA-Z0-9]+$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "Plan name cannot contain special characters";

                return response;
            }
            else
            {
                //Check if PlanName Exists ====================    
                SqlBoolStatements sqlBoolStatements = new SqlBoolStatements();
                bool accountExists = sqlBoolStatements.PlanNameExists(PlanName);

                if (accountExists)
                {
                    response.isValid = false;
                    response.validationMessage = "Plan with that name already exists.";

                    return response;
                }
                else
                {
                    response.isValid = true;
                }
            }


            return response;
        }

        #endregion

        #region Application

        #region Generic Objects

        public static ValidationResponseType IsValidObjectName(string objectName)
        {
            ValidationResponseType response = new ValidationResponseType();

            //Check if name is null
            if (objectName == null || objectName == String.Empty)
            {
                response.isValid = false;
                response.validationMessage = "Name cannot be blank.";

                return response;
            }

            //Check if Objcect Name is a system reserved name:
            foreach (string reservedName in Settings.Objects.Names.ReservedObjectNames)
            {
                if (Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(objectName) == reservedName)
                {
                    response.isValid = false;
                    response.validationMessage = objectName + " is a reserved name.";

                    return response;
                }
            }
            //Check if name has trailing whitespace(s)
            if (Regex.IsMatch(objectName, @"\s+$"))
            {
                response.isValid = false;
                response.validationMessage = "Name cannot contain empty spaces at the end.";

                return response;
            }

            //Check if name has whitespace(s) at front
            if (objectName[0].ToString() == " ")
            {
                response.isValid = false;
                response.validationMessage = "Name cannot contain an empty space as the first character.";

                return response;
            }

            //Check if the character length conforsms to policy
            if (objectName.Length < Settings.Objects.Names.ObjectNameMinimumLength || objectName.Length > Settings.Objects.Names.ObjectNameMaximunLength)
            {
                response.isValid = false;
                response.validationMessage = "Name must be between " + Settings.Objects.Names.ObjectNameMinimumLength + "-" + Settings.Objects.Names.ObjectNameMaximunLength + " characters long.";

                return response;
            }
            //We allow for spaces, dashes, ampersands and apostrophes, so we remove them before doing a regex for other special characters
            else if ((Regex.IsMatch(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(objectName), "^[a-zA-Z0-9\\-]*$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "Name cannot contain special characters";

                return response;
            }
            else
            {

                response.isValid = true;
            }


            return response;
        }

        #endregion

        #region Tags

        public static ValidationResponseType IsValidTagName(string tagName)
        {
            ValidationResponseType response = new ValidationResponseType();

            //Check if name is null
            if (tagName == null || tagName == String.Empty)
            {
                response.isValid = false;
                response.validationMessage = "Tag cannot be blank.";

                return response;
            }

            //Check if Objcect Name is a system reserved name:
            /*
            foreach (string reservedName in Settings.Objects.Names.ReservedObjectNames)
            {
                if (Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(objectName) == reservedName)
                {
                    response.isValid = false;
                    response.validationMessage = objectName + " is a reserved name.";

                    return response;
                }
            }*/
            //Check if name has trailing whitespace(s)
            if (Regex.IsMatch(tagName, @"\s+$"))
            {
                response.isValid = false;
                response.validationMessage = "Tags cannot contain empty spaces at the end.";

                return response;
            }

            //Check if name has whitespace(s) at front
            if (tagName[0].ToString() == " ")
            {
                response.isValid = false;
                response.validationMessage = "Tags cannot contain an empty space as the first character.";

                return response;
            }

            //Check if the character length conforsms to policy
            if (tagName.Length < Settings.Objects.Names.TagNameMinimumLength || tagName.Length > Settings.Objects.Names.TagNameMaximunLength)
            {
                response.isValid = false;
                response.validationMessage = "Tag must be between " + Settings.Objects.Names.TagNameMinimumLength + "-" + Settings.Objects.Names.TagNameMaximunLength + " characters long.";

                return response;
            }
            //We allow for spaces, dashes, ampersands and apostrophes, so we remove them before doing a regex for other special characters
            else if ((Regex.IsMatch(tagName, "^[a-zA-Z0-9\\-\\s]*$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "Tag cannot contain special characters";

                return response;
            }
            else
            {

                response.isValid = true;
            }

            if (tagName.Contains(","))
            {
                response.isValid = false;
                response.validationMessage = "Tag cannot contain commas";

                return response;
            }


            return response;
        }

        #endregion


        #region Properties

        public static ValidationResponseType IsValidPropertyName(string propertyName)
        {
            ValidationResponseType response = new ValidationResponseType();

            //Check if name is null
            if (propertyName == null || propertyName == String.Empty)
            {
                response.isValid = false;
                response.validationMessage = "Property cannot be blank.";

                return response;
            }

            //Check if Property Name is a system reserved name:
            foreach (string reservedName in Settings.Objects.Names.ReservedPropertyNames)
            {
                if (Sahara.Core.Common.Methods.PropertyNames.ConvertToPropertyNameKey(propertyName) == reservedName)
                {
                    response.isValid = false;
                    response.validationMessage = propertyName + " is a reserved name.";

                    return response;
                }
            }

            if(propertyName.ToLower().Contains("locationmetadata"))
            {
                response.isValid = false;
                response.validationMessage = "Property cannot contain the term 'locationmetadata'.";

                return response;
            }

            //Check if name has trailing whitespace(s)
            if (Regex.IsMatch(propertyName, @"\s+$"))
            {
                response.isValid = false;
                response.validationMessage = "Property cannot contain empty spaces at the end.";

                return response;
            }

            //Check if name has whitespace(s) at front
            if (propertyName[0].ToString() == " ")
            {
                response.isValid = false;
                response.validationMessage = "Property cannot contain an empty space as the first character.";

                return response;
            }

            //Check if the character length conforsms to policy
            if (propertyName.Length < Settings.Objects.Names.TagNameMinimumLength || propertyName.Length > Settings.Objects.Names.TagNameMaximunLength)
            {
                response.isValid = false;
                response.validationMessage = "Property must be between " + Settings.Objects.Names.PropertyNameMinimumLength + "-" + Settings.Objects.Names.PropertyNameMaximunLength + " characters long.";

                return response;
            }
            //We allow for spaces, dashes, ampersands and apostrophes, so we remove them before doing a regex for other special characters
            else if ((Regex.IsMatch(propertyName, "^[a-zA-Z0-9\\-\\s]*$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "Property cannot contain special characters";

                return response;
            }
            else
            {

                response.isValid = true;
            }

            if (propertyName.Contains(","))
            {
                response.isValid = false;
                response.validationMessage = "Property cannot contain commas";

                return response;
            }


            return response;
        }

        #endregion

        #region Image Group Objects

        public static ValidationResponseType IsValidImageGroupName(string imageGroupName)
        {
            ValidationResponseType response = new ValidationResponseType();

            //Check if name is null
            if (imageGroupName == null || imageGroupName == String.Empty)
            {
                response.isValid = false;
                response.validationMessage = "Name cannot be blank.";

                return response;
            }

            //Check if Objcect Name is a system reserved name:
            foreach (string reservedName in Settings.Objects.Names.ReservedImageGroupNames)
            {
                if (Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(imageGroupName) == reservedName)
                {
                    response.isValid = false;
                    response.validationMessage = imageGroupName + " is a reserved name.";

                    return response;
                }
            }

            //Check if name has ANY whitespace(s)
            if (imageGroupName.Contains(" "))
            {
                response.isValid = false;
                response.validationMessage = "Name cannot contain any spaces.";

                return response;
            }

            //Check if the character length conforsms to policy
            if (imageGroupName.Length < Settings.Objects.Names.ObjectNameMinimumLength || imageGroupName.Length > Settings.Objects.Names.ObjectNameMaximunLength)
            {
                response.isValid = false;
                response.validationMessage = "Name must be between " + Settings.Objects.Names.ObjectNameMinimumLength + "-" + Settings.Objects.Names.ObjectNameMaximunLength + " characters long.";

                return response;
            }
            //We ONLY allow for letters and numbers, no spaces or special characters
            else if ((Regex.IsMatch(imageGroupName, @"^[a-zA-Z0-9]+$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "Name can only contain letters or numbers. No special characters allowed.";

                return response;
            }
            else
            {
                response.isValid = true;
            }


            return response;
        }

        public static ValidationResponseType IsValidImageFormatName(string imageFormatName)
        {
            ValidationResponseType response = new ValidationResponseType();

            //Check if name is null
            if (imageFormatName == null || imageFormatName == String.Empty)
            {
                response.isValid = false;
                response.validationMessage = "Name cannot be blank.";

                return response;
            }

            //Check if Objcect Name is a system reserved name:
            foreach (string reservedName in Settings.Objects.Names.ReservedImageFormatNames)
            {
                if (Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(imageFormatName) == reservedName)
                {
                    response.isValid = false;
                    response.validationMessage = imageFormatName + " is a reserved name.";

                    return response;
                }
            }

            //Check if name has ANY whitespace(s)
            if (imageFormatName.Contains(" "))
            {
                response.isValid = false;
                response.validationMessage = "Name cannot contain any spaces.";

                return response;
            }

            //Check if the character length conforsms to policy
            if (imageFormatName.Length < Settings.Objects.Names.ObjectNameMinimumLength || imageFormatName.Length > Settings.Objects.Names.ObjectNameMaximunLength)
            {
                response.isValid = false;
                response.validationMessage = "Name must be between " + Settings.Objects.Names.ObjectNameMinimumLength + "-" + Settings.Objects.Names.ObjectNameMaximunLength + " characters long.";

                return response;
            }
            //We ONLY allow for letters
            else if ((Regex.IsMatch(imageFormatName, @"^[a-zA-Z0-9]+$")) == false)
            {
                response.isValid = false;
                response.validationMessage = "Name can only contain letters and numbers. No special characters allowed.";

                return response;
            }
            else
            {
                response.isValid = true;
            }


            return response;
        }

        #endregion

        #endregion
    }


}
