using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using CnGalWebSite.TimedTask.Models.DataModels;
using CnGalWebSite.TimedTask.Extentions;

namespace CnGalWebSite.TimedTask.Infrastructure
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }


        public DbSet<TimedTaskModel> TimedTasks { get; set; }

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
        }
    }
}
