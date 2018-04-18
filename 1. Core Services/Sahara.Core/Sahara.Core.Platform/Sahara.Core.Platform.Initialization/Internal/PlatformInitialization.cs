using Sahara.Core.Accounts;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Infrastructure.Azure.Models.Database;
using Sahara.Core.Infrastructure.Azure.Types.Database;
using Sahara.Core.Platform.Users;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Diagnostics;
using Sahara.Core.Accounts.Themes.Public;
using Sahara.Core.Accounts.Themes.Models;

namespace Sahara.Core.Platform.Initialization
{
    internal class PlatformInitialization
    {

        internal DataAccessResponseType InitializePlatform()
        {

            DataAccessResponseType response = new DataAccessResponseType();

            /*======================================
                        CREATE DATABASES        
            ========================================*/
      
            //Create Platform Database:
            DatabaseModel platformDatabase = new DatabaseModel();
            platformDatabase.DatabaseTier = DatabaseTier.Basic;
            platformDatabase.DatabaseName = "Platform";
            platformDatabase.DatabaseSize = MaxSize_Basic._2GB;

            Sql.Scripts.Create.CreateDatabase(platformDatabase, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);



            //Create Accounts Database:
            DatabaseModel accountsDatabase = new DatabaseModel();
            accountsDatabase.DatabaseTier = DatabaseTier.Basic;
            accountsDatabase.DatabaseName = "Accounts";
            accountsDatabase.DatabaseSize = MaxSize_Basic._2GB;

            Sql.Scripts.Create.CreateDatabase(accountsDatabase, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);


            /*======================================
                    Schema & Tables & Seed       
            ========================================*/

            //Run Schema for Accounts on Accounts DB:
            var accountDBInitialization = new Sql.Scripts.AccountsDB.Initialization.Initialization();
            accountDBInitialization.InitializeAccountsDB(Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);

            //using (Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection)
            //{

            //}

            //Run Schema for Platform on Platform DB:
            var platformDBInitialization = new Sql.Scripts.PlatformDB.Initialization.Initialization();
            platformDBInitialization.InitializePlatformDB(Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);



            /*======================================
                Create Initial Platform Roles      
            ========================================*/

            var createPlatformRolesResult = PlatformUserManager.CreateRoles(Sahara.Core.Settings.Platform.Users.Authorization.Roles.GetRoles()).Result;

            /*======================================
                 Create Initial AccountUser Roles      
            ========================================*/

            var createAccountRolesResult = AccountUserManager.CreateRoles(Sahara.Core.Settings.Accounts.Users.Authorization.Roles.GetRoles()).Result;


            /*=====================================
                Initialize Available Themes
           ========================================*/

            
            #region Create Default Theme(s)

            ThemesManager.CreateTheme(new ThemeModel
            {
                Name = "Light",
                Font = "segoe",
                Colors = new ThemeColorsModel
                {
                    Background = "FFFFFF",
                    BackgroundGradianetTop = "FFFFFF",
                    BackgroundGradientBottom = "DEDEDE",
                    Shadow = "858585",
                    Highlight = "000000",
                    Foreground = "000000",
                    Overlay = "3D3D3D",
                    Trim = "5E5E5E"
                }
            });


            ThemesManager.CreateTheme(new ThemeModel
            {
                Name = "Charcoal",
                Font = "segoe",
                Colors = new ThemeColorsModel
                {
                    Background = "000000",
                    BackgroundGradianetTop = "000000",
                    BackgroundGradientBottom = "4A4A4A",
                    Shadow = "A8A8A8",
                    Highlight = "D9D9D9",
                    Foreground = "FFFFFF",
                    Overlay = "828282",
                    Trim = "4D4D4D"
                }
            });

            ThemesManager.CreateTheme(new ThemeModel
            {
                Name = "Cyan",
                Font = "segoe",
                Colors = new ThemeColorsModel
                {
                    Background = "9DD9ED",
                    BackgroundGradianetTop = "9DD9ED",
                    BackgroundGradientBottom = "4F94AB",
                    Shadow = "1D4E5E",
                    Highlight = "CFF3FF",
                    Foreground = "1D4E5E",
                    Overlay = "A18F28",
                    Trim = "FF8800"
                }
            });


            #endregion
            

            response.isSuccess = true;

            return response;
        }
    }
}
