using CnGalWebSite.ProjectSite.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Text.Json;

namespace CnGalWebSite.ProjectSite.API.Infrastructure
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectPosition> ProjectPositions { get; set; }
        public DbSet<Stall> Stalls { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<UserAudio> UserAudios { get; set; }
        public DbSet<UserText> UserTexts { get; set; }
        public DbSet<Carousel> Carousels { get; set; }
        public DbSet<FriendLink> FriendLinks { get; set; }
        public DbSet<StallInformationType> StallInformationTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //关闭级联删除
            var foreignKeys = modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys());
            foreach (var foreignKey in foreignKeys)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            //限定名称唯一
            modelBuilder.Entity<ApplicationUser>().HasIndex(g => g.UserName).IsUnique();

            //设置枚举数组转换
            modelBuilder.Entity<StallInformationType>()
                .Property(e => e.Types)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<ProjectPositionType[]>(v, (JsonSerializerOptions)null),
                    new ValueComparer<ProjectPositionType[]>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToArray()));
        }
    }
}
