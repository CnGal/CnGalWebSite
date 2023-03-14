using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;
using System;
using CnGalWebSite.IdentityServer.Models.Account;
using CnGalWebSite.IdentityServer.Models.Messages;

namespace CnGalWebSite.IdentityServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<SendRecord> SendRecords { get; set; }


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

            //角色Id
            const string ADMIN_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
            const string ROLE_ID = ADMIN_ID;
            const string USER_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e576";
            const string SUPER_ADMIN_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e577";
            const string EDITOR_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e578";

            //创建种子角色
            builder.Entity<IdentityRole>().HasData(new IdentityRole {Id = ROLE_ID, Name = "Admin", NormalizedName = "Admin"},
                new IdentityRole { Name = "User", NormalizedName = "USER", Id = USER_ROLE_ID },
                new IdentityRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN", Id = SUPER_ADMIN_ROLE_ID } ,
                new IdentityRole { Name = "Editor", NormalizedName = "EDITOR", Id = EDITOR_ROLE_ID });

            //创建超级管理员
            var hasher = new PasswordHasher<ApplicationUser>();
            builder.Entity<ApplicationUser>().HasData(new ApplicationUser
            {
                Id = ADMIN_ID,
                UserName = "Admin",
                NormalizedUserName = "ADMIN",
                Email = "123456789@qq.com",
                NormalizedEmail = "123456789@qq.com",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAEDecloBliZOnB0dNPQmr8qhoodaLmPdrKN10/bvLDrHaAJSxqWOnrEsvBhl5kzrZmQ==",//hasher.HashPassword(null, "CngalAdmin123.."),
                SecurityStamp = string.Empty
            });

            //添加管理员角色
            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
                {
                    RoleId = ROLE_ID,
                    UserId = ADMIN_ID
                }, new IdentityUserRole<string>
                {
                    RoleId = USER_ROLE_ID,
                    UserId = ADMIN_ID
                },
                new IdentityUserRole<string>
                {
                    RoleId = SUPER_ADMIN_ROLE_ID,
                    UserId = ADMIN_ID
                }, new IdentityUserRole<string>
                {
                    RoleId = EDITOR_ROLE_ID,
                    UserId = ADMIN_ID
                });
        }
    }
}
