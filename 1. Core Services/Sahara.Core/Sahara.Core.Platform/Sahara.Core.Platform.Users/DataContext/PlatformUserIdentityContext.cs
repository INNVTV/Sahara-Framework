using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sahara.Core.Platform.Users.Models;

namespace Sahara.Core.Platform.Users.DataContext
{
    internal class PlatformUserIdentityDbContext : IdentityDbContext<PlatformUserIdentity>, IDisposable
    {
        public PlatformUserIdentityDbContext()
            : base(Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.ConnectionString)
        {

            #if DEBUG

                //Write SQL out to output log
                Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

            #endif

            // DISABLE MODEL SCHEMA MIGRATIONS
            // Database.SetInitializer<ClientIdentityDbContext>(null);

            // Destroy Database When Model Changes (Comment out if using DataBase first and want to ignore creating/using __Migrations table):
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<PlatformUserIdentityDbContext>());


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
                //.ToTable("PlatformUsers", "dbo");//.Property(p => p.Id).HasColumnName("UserID");
            modelBuilder.Entity<PlatformUserIdentity>()
                .ToTable("PlatformUsers", "dbo");//.Property(p => p.Id).HasColumnName("UserID");


            modelBuilder.Entity<IdentityRole>()
                .ToTable("PlatformUserRoles", "dbo");

            modelBuilder.Entity<IdentityUserClaim>()
                .ToTable("PlatformUserClaims", "dbo");

            modelBuilder.Entity<IdentityUserLogin>()
                .ToTable("PlatformUserLogins", "dbo");

            modelBuilder.Entity<IdentityUserRole>()
                .ToTable("PlatformUsersInRoles", "dbo");

        }
    }
}
