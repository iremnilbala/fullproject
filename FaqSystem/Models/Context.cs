using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FaqSystem.Models
{
    public class Context : IdentityDbContext<User>
    {
        //public Context():base("Context") {
        //    Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, Migrations.Configuration>("Context"));
        //}
        public DbSet<Faq> Faqs { get; set; }
        //public DbSet<User> User { get; set; }

        public DbSet<AuthorizedUser> AuthorizedUsers { get; set; }

    }
}

    