using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Tables;
using System;
using System.Linq;

namespace CnGalWebSite.APIServer.Infrastructure
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Examine> Examines { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Carousel> Carousels { get; set; }
        public DbSet<FriendLink> FriendLinks { get; set; }
        public DbSet<TokenCustom> TokenCustoms { get; set; }
        public DbSet<FileManager> FileManagers { get; set; }
        public DbSet<UserFile> UserFiles { get; set; }
        public DbSet<ThumbsUp> ThumbsUps { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<SteamInfor> SteamInfors { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<HistoryUser> HistoryUsers { get; set; }
        public DbSet<UserSpaceCommentManager> UserSpaceCommentManagers { get; set; }
        public DbSet<PlayedGame> PlayedGames { get; set; }
        public DbSet<ErrorCount> ErrorCounts { get; set; }
        public DbSet<UserOnlineInfor> UserOnlineInfors { get; set; }
        public DbSet<Disambig> Disambigs { get; set; }
        public DbSet<TimedTask> TimedTasks { get; set; }
        public DbSet<BackUpArchive> BackUpArchives { get; set; }
        public DbSet<BackUpArchiveDetail> BackUpArchiveDetails { get; set; }
        public DbSet<FavoriteObject> FavoriteObjects { get; set; }
        public DbSet<FavoriteFolder> FavoriteFolders { get; set; }
        public DbSet<SendCount> SendCounts { get; set; }
        public DbSet<Loginkey> Loginkeys { get; set; }
        public DbSet<BasicInforTableModel> BasicInforTableModels { get; set; }
        public DbSet<GroupInforTableModel> GroupInforTableModels { get; set; }
        public DbSet<MakerInforTableModel> MakerInforTableModels { get; set; }
        public DbSet<RoleInforTableModel> RoleInforTableModels { get; set; }
        public DbSet<StaffInforTableModel> StaffInforTableModels { get; set; }
        public DbSet<SteamInforTableModel> SteamInforTableModels { get; set; }
        public DbSet<ThirdPartyLoginInfor> ThirdPartyLoginInfors { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<RankUser> RankUsers { get; set; }
        public DbSet<Perfection> Perfections { get; set; }
        public DbSet<PerfectionCheck> PerfectionChecks { get; set; }
        public DbSet<PerfectionOverview> PerfectionOverviews { get; set; }
        public DbSet<Periphery> Peripheries { get; set; }
        public DbSet<PeripheryRelevanceUser> PeripheryRelevanceUsers { get; set; }
        public DbSet<PeripheryRelevanceEntry> PeripheryRelevanceEntries { get; set; }
        public DbSet<PeripheryRelation> PeripheryRelations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //关闭级联删除
            var foreignKeys = modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys());
            foreach (var foreignKey in foreignKeys)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
            //限定外键
            modelBuilder.Entity<Article>()
                .HasOne(b => b.BackUpArchive)
                .WithOne(i => i.Article)
                .HasForeignKey<BackUpArchive>(b => b.ArticleId);
            modelBuilder.Entity<Entry>()
                .HasOne(b => b.BackUpArchive)
                .WithOne(i => i.Entry)
                .HasForeignKey<BackUpArchive>(b => b.EntryId);

            //限定名称唯一
            modelBuilder.Entity<ApplicationUser>().HasIndex(g => g.UserName).IsUnique();
            modelBuilder.Entity<Entry>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Article>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Tag>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Rank>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Periphery>().HasIndex(g => g.Name).IsUnique();

            //设定默认值
            modelBuilder.Entity<Article>().Property(b => b.CanComment).HasDefaultValue(true);
            modelBuilder.Entity<ApplicationUser>().Property(b => b.CanComment).HasDefaultValue(true);
            modelBuilder.Entity<Entry>().Property(b => b.CanComment).HasDefaultValue(true);

            //设置周边自身多对多关系
            modelBuilder.Entity<PeripheryRelation>(entity =>
            {
                entity.Property(e => e.PeripheryRelationId).HasColumnName("PeripheryRelationId");

                entity.HasOne(d => d.FromPeripheryNavigation)
                    .WithMany(p => p.PeripheryRelationFromPeripheryNavigation)
                    .HasForeignKey(d => d.FromPeriphery)
                    .HasConstraintName("FK_PeripheryRelation_Periphery_From");

                entity.HasOne(d => d.ToPeripheryNavigation)
                    .WithMany(p => p.PeripheryRelationToPeripheryNavigation)
                    .HasForeignKey(d => d.ToPeriphery)
                    .HasConstraintName("FK_PeripheryRelation_Periphery_To");
            });


            //角色Id
            const string ADMIN_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
            const string ROLE_ID = ADMIN_ID;
            const string USER_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e576";
            const string SUPER_ADMIN_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e577";


            //创建种子角色
            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = ROLE_ID,
                Name = "Admin",
                NormalizedName = "Admin"
            }, new IdentityRole { Name = "User", NormalizedName = "USER", Id = USER_ROLE_ID }
            , new IdentityRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN", Id = SUPER_ADMIN_ROLE_ID }
            );

            //创建超级管理员
            var hasher = new PasswordHasher<ApplicationUser>();
            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser
            {
                Id = ADMIN_ID,
                UserName = "Admin",
                NormalizedUserName = "ADMIN",
                Email = "1278490989@qq.com",
                NormalizedEmail = "1278490989@qq.com",
                EmailConfirmed = true,
                PersonalSignature = "这个人太懒了，什么也没写额(～￣▽￣)～",
                Integral = 0,
                ContributionValue = 0,
                MainPageContext = "### 这个人太懒了，什么也没写额(～￣▽￣)～",
                Birthday = null,
                RegistTime = DateTime.Now.ToCstTime(),
                PasswordHash = "AQAAAAEAACcQAAAAEDecloBliZOnB0dNPQmr8qhoodaLmPdrKN10/bvLDrHaAJSxqWOnrEsvBhl5kzrZmQ==",//hasher.HashPassword(null, "CngalAdmin123.."),
                SecurityStamp = string.Empty
            });

            //添加管理员角色
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
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
                }
            );

            //添加四个顶级标签
            modelBuilder.Entity<Tag>().HasData(new Tag
            {
                Id = 1,
                Name = "游戏"
            }, new Tag
            {
                Id = 2,
                Name = "角色"
            }, new Tag
            {
                Id = 3,
                Name = "STAFF"
            }, new Tag
            {
                Id = 4,
                Name = "制作组"
            }
            );

        }
    }
}
