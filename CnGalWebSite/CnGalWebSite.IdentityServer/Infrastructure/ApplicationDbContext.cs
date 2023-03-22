using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;
using System;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Messages;
using CnGalWebSite.IdentityServer.Models.DataModels.Records;
using CnGalWebSite.IdentityServer.Admin.SSR.Models;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;

namespace CnGalWebSite.IdentityServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>,
        ApplicationUserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<SendRecord> SendRecords { get; set; }
        public DbSet<OperationRecord> OperationRecords { get; set; }
        public DbSet<AppUserAccessToken> AppUserAccessTokens { get; set; }
        public DbSet<ClientExamine> ClientExamines { get; set; }
        public DbSet<Examine> Examines { get; set; }


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

            //建立用户角色关联
            builder.Entity<ApplicationUserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            //限定名称唯一
            builder.Entity<ApplicationUser>().HasIndex(g => g.UserName).IsUnique();

            //角色Id
            const string ADMIN_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
            const string ROLE_ID = ADMIN_ID;
            const string USER_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e576";
            const string SUPER_ADMIN_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e577";
            const string EDITOR_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e578";

            //创建种子角色
            builder.Entity<ApplicationRole>().HasData(new ApplicationRole { Id = ROLE_ID, Name = "Admin", NormalizedName = "ADMIN"},
                new ApplicationRole { Name = "User", NormalizedName = "USER", Id = USER_ROLE_ID },
                new ApplicationRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN", Id = SUPER_ADMIN_ROLE_ID } ,
                new ApplicationRole { Name = "Editor", NormalizedName = "EDITOR", Id = EDITOR_ROLE_ID });

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
                SecurityStamp = string.Empty,
                LockoutEnabled = true
            });

            //添加管理员角色
            builder.Entity<ApplicationUserRole>().HasData(new ApplicationUserRole
                {
                    RoleId = ROLE_ID,
                    UserId = ADMIN_ID
                }, new ApplicationUserRole
                {
                    RoleId = USER_ROLE_ID,
                    UserId = ADMIN_ID
                },
                new ApplicationUserRole
                {
                    RoleId = SUPER_ADMIN_ROLE_ID,
                    UserId = ADMIN_ID
                }, new ApplicationUserRole
                {
                    RoleId = EDITOR_ROLE_ID,
                    UserId = ADMIN_ID
                });
        }
    }
}
