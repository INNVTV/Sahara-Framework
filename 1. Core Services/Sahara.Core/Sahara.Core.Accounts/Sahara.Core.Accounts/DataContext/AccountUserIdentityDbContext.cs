using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sahara.Core.Accounts.Models;

namespace Sahara.Core.Accounts.DataContext
{
    internal class AccountUserIdentityDbContext : IdentityDbContext<AccountUserIdentity>, IDisposable
    {
        public AccountUserIdentityDbContext()
            : base(Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.ConnectionString)

        {

            #if DEBUG

                //Write SQL out to output log
                Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

            #endif

            // DISABLE MODEL SCHEMA MIGRATIONS
            // Database.SetInitializer<ClientIdentityDbContext>(null);

            // Destroy Database When Model Changes (Comment out if using DataBase first and want to ignore creating/using __Migrations table):
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<AccountUserIdentityDbContext>());


            // REQUIRED FOR OPTIMAL QUERIES WHEN SELECTING PRIMARY KEYS
            // outputs "where userId= 'blah" instead of
            // "where userId = 'blah' and userId is not null"
            Configuration.UseDatabaseNullSemantics = true;

        }

        //public override DbSet<ClientUser> Users { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


            //modelBuilder.Entity<IdentityUser>()
                //.ToTable("AccountUsers", "dbo");//.Property(p => p.Id).HasColumnName("UserID");
            modelBuilder.Entity<AccountUserIdentity>()
                .ToTable("AccountUsers", "dbo");//.Property(p => p.Id).HasColumnName("UserID");


            modelBuilder.Entity<IdentityRole>()
                .ToTable("AccountUserRoles", "dbo");

            modelBuilder.Entity<IdentityUserClaim>()
                .ToTable("AccountUserClaims", "dbo");

            modelBuilder.Entity<IdentityUserLogin>()
                .ToTable("AccountUserLogins", "dbo");

            modelBuilder.Entity<IdentityUserRole>()
                .ToTable("AccountUsersInRoles", "dbo");

        }
    }
}
