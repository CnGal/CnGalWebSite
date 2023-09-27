using Blazored.LocalStorage;
using Live2DTest.DataRepositories;
using CnGalWebSite.Kanban.Services.Core;
using CnGalWebSite.Kanban.Services.Dialogs;
using CnGalWebSite.Kanban.Services.Events;
using CnGalWebSite.Kanban.Services.Settings;
using CnGalWebSite.Kanban.Services.UserDatas;
using Masa.Blazor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKanbanLive2D(this IServiceCollection services)
        {
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IDialogBoxService, DialogBoxService>();
            services.AddScoped<ILive2DService, Live2DService>();
            services.AddScoped<IUserDataService, UserDataService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(sp => new HttpClient());
            services.AddBlazoredLocalStorage();
            return services;
        }
    }
}
