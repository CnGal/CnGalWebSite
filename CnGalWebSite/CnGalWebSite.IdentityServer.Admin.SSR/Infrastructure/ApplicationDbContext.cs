using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;
using System;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Messages;
using CnGalWebSite.IdentityServer.Models.DataModels.Records;
using CnGalWebSite.IdentityServer.Admin.SSR.Models;

namespace CnGalWebSite.IdentityServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<AppUserAccessToken> AppUserAccessTokens { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

           
        }
    }
}
