using CnGalWebSite.ProjectSite.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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
        }
    }
}
