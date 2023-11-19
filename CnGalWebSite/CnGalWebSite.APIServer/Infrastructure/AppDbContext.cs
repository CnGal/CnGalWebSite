using CnGalWebSite.APIServer.Extentions;
using CnGalWebSite.APIServer.Model;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Text.Json;

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
        public DbSet<GameNews> GameNewses { get; set; }
        public DbSet<WeeklyNews> WeeklyNewses { get; set; }
        public DbSet<WeiboUserInfor> WeiboUserInfors { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VoteOption> VoteOptions { get; set; }
        public DbSet<VoteUser> VoteUsers { get; set; }
        public DbSet<EntryRelation> EntryRelations { get; set; }
        public DbSet<ArticleRelation> ArticleRelations { get; set; }
        public DbSet<UserIntegral> UserIntegrals { get; set; }
        public DbSet<Lottery> Lotteries { get; set; }
        public DbSet<LotteryUser> LotteryUsers { get; set; }
        public DbSet<LotteryAward> LotteryAwards { get; set; }
        public DbSet<LotteryPrize> LotteryPrizes { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<SteamUserInfor> SteamUserInfors { get; set; }
        public DbSet<SearchCache> SearchCaches { get; set; }
        public DbSet<RobotEvent> RobotEvents { get; set; }
        public DbSet<RobotReply> RobotReplies { get; set; }
        public DbSet<RobotGroup> RobotGroups { get; set; }
        public DbSet<RobotFace> RobotFaces { get; set; }
        public DbSet<GameScoreTableModel> GameScores { get; set; }
        public DbSet<OperationRecord> OperationRecords { get; set; }
        public DbSet<EntryStaff> EntryStaffs { get; set; }
        public DbSet<UserCertification> UserCertifications { get; set; }
        public DbSet<UserMonitor> UserMonitors { get; set; }
        public DbSet<UserReviewEditRecord> UserReviewEditRecords { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<RoleBirthday> RoleBirthdays { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingUser> BookingUsers { get; set; }
        public DbSet<EntryWebsite> EntryWebsites { get; set; }
        public DbSet<StoreInfo> StoreInfo { get; set; }
        public DbSet<Recommend> Recommends { get; set; }
        public DbSet<EntryInformationType> EntryInformationTypes { get; set; }
        public DbSet<BasicEntryInformation> BasicEntryInformation { get; set; }
        public DbSet<Commodity> Commodities { get; set; }
        public DbSet<ApplicationUserCommodity> ApplicationUserCommodities { get; set; }
        public DbSet<Almanac> Almanacs { get; set; }
        public DbSet<AlmanacArticle> AlmanacArticles { get; set; }
        public DbSet<AlmanacEntry> AlmanacEntries { get; set; }


        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {

            //保存时将UTC时间+8小时 配合历史数据UTC+8时区
            //读取时-8小时 转换成UTC时间

            configurationBuilder
                .Properties<DateTime>()
                .HaveConversion(typeof(EFCoreUtcValueConverter));

            configurationBuilder
               .Properties<DateTime?>()
               .HaveConversion(typeof(EFCoreUtcNullableValueConverter));
        }

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
            modelBuilder.Entity<Vote>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Lottery>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Disambig>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Video>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<EntryInformationType>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Commodity>().HasIndex(g => g.Name).IsUnique();

            //限定外键唯一
            modelBuilder.Entity<RoleBirthday>().HasIndex(g => g.RoleId).IsUnique();
            modelBuilder.Entity<GameScoreTableModel>().HasIndex(g => g.GameId).IsUnique();
            modelBuilder.Entity<SteamInfor>().HasIndex(g => g.SteamId).IsUnique();
            modelBuilder.Entity<SteamInforTableModel>().HasIndex(g => g.SteamId).IsUnique();
            modelBuilder.Entity<Recommend>().HasIndex(g => g.EntryId).IsUnique();

            //设定默认值
            modelBuilder.Entity<Article>().Property(b => b.CanComment).HasDefaultValue(true);
            modelBuilder.Entity<ApplicationUser>().Property(b => b.CanComment).HasDefaultValue(true);
            modelBuilder.Entity<Entry>().Property(b => b.CanComment).HasDefaultValue(true);
            modelBuilder.Entity<Periphery>().Property(b => b.CanComment).HasDefaultValue(true);
            modelBuilder.Entity<Vote>().Property(b => b.CanComment).HasDefaultValue(true);
            modelBuilder.Entity<Lottery>().Property(b => b.CanComment).HasDefaultValue(true);

            //多对多链接关系
            modelBuilder.Entity<Commodity>()
                    .HasMany(e => e.Users)
                    .WithMany(e => e.Commodities)
                    .UsingEntity<ApplicationUserCommodity>();

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
            //设置词条自身多对多关系
            modelBuilder.Entity<EntryRelation>(entity =>
            {
                entity.Property(e => e.EntryRelationId).HasColumnName("EntryRelationId");

                entity.HasOne(d => d.FromEntryNavigation)
                    .WithMany(p => p.EntryRelationFromEntryNavigation)
                    .HasForeignKey(d => d.FromEntry)
                    .HasConstraintName("FK_EntryRelation_Entry_From");

                entity.HasOne(d => d.ToEntryNavigation)
                    .WithMany(p => p.EntryRelationToEntryNavigation)
                    .HasForeignKey(d => d.ToEntry)
                    .HasConstraintName("FK_EntryRelation_Entry_To");
            });
            //设置文章自身多对多关系
            modelBuilder.Entity<ArticleRelation>(entity =>
            {
                entity.Property(e => e.ArticleRelationId).HasColumnName("ArticleRelationId");

                entity.HasOne(d => d.FromArticleNavigation)
                    .WithMany(p => p.ArticleRelationFromArticleNavigation)
                    .HasForeignKey(d => d.FromArticle)
                    .HasConstraintName("FK_ArticleRelation_Article_From");

                entity.HasOne(d => d.ToArticleNavigation)
                    .WithMany(p => p.ArticleRelationToArticleNavigation)
                    .HasForeignKey(d => d.ToArticle)
                    .HasConstraintName("FK_ArticleRelation_Entry_To");//笔误 应为 FK_ArticleRelation_Article_To
            });
            //设置Staff与词条多对多关系
            modelBuilder.Entity<EntryStaff>(entity =>
            {
                entity.Property(e => e.EntryStaffId).HasColumnName("EntryStaffId");

                entity.HasOne(d => d.FromEntryNavigation)
                    .WithMany(p => p.EntryStaffFromEntryNavigation)
                    .HasForeignKey(d => d.FromEntry)
                    .HasConstraintName("FK_EntryStaff_Entry_From");

                entity.HasOne(d => d.ToEntryNavigation)
                    .WithMany(p => p.EntryStaffToEntryNavigation)
                    .HasForeignKey(d => d.ToEntry)
                    .HasConstraintName("FK_EntryStaff_Entry_To");
            });
            //设置视频自身多对多关系
            modelBuilder.Entity<VideoRelation>(entity =>
            {
                entity.Property(e => e.VideoRelationId).HasColumnName("VideoRelationId");

                entity.HasOne(d => d.FromVideoNavigation)
                    .WithMany(p => p.VideoRelationFromVideoNavigation)
                    .HasForeignKey(d => d.FromVideo)
                    .HasConstraintName("FK_VideoRelation_Video_From");

                entity.HasOne(d => d.ToVideoNavigation)
                    .WithMany(p => p.VideoRelationToVideoNavigation)
                    .HasForeignKey(d => d.ToVideo)
                    .HasConstraintName("FK_VideoRelation_Video_To");
            });

            //设置枚举数组转换
            modelBuilder.Entity<GameRelease>()
                .Property(e => e.GamePlatformTypes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<GamePlatformType[]>(v, (JsonSerializerOptions)null),
                    new ValueComparer<GamePlatformType[]>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToArray()));
            modelBuilder.Entity<EntryInformationType>()
                .Property(e => e.Types)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<EntryType[]>(v, (JsonSerializerOptions)null),
                    new ValueComparer<EntryType[]>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToArray()));

            //角色Id
            const string ADMIN_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
            const string ROLE_ID = ADMIN_ID;
            const string USER_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e576";
            const string SUPER_ADMIN_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e577";
            const string EDITOR_ROLE_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e578";


            //创建种子角色
            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = ROLE_ID,
                Name = "Admin",
                NormalizedName = "Admin"
            }, new IdentityRole { Name = "User", NormalizedName = "USER", Id = USER_ROLE_ID }
            , new IdentityRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN", Id = SUPER_ADMIN_ROLE_ID }
            , new IdentityRole { Name = "Editor", NormalizedName = "EDITOR", Id = EDITOR_ROLE_ID }
            );

            //创建超级管理员
            var hasher = new PasswordHasher<ApplicationUser>();
            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser
            {
                Id = ADMIN_ID,
                UserName = "Admin",
                NormalizedUserName = "ADMIN",
                Email = "123456789@qq.com",
                NormalizedEmail = "123456789@qq.com",
                EmailConfirmed = true,
                PersonalSignature = "这个人太懒了，什么也没写额(～￣▽￣)～",
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
                }, new IdentityUserRole<string>
                {
                    RoleId = EDITOR_ROLE_ID,
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
