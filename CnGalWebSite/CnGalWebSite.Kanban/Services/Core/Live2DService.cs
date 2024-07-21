using BlazorComponent;
using Live2DTest.DataRepositories;
using CnGalWebSite.Kanban.Models;
using CnGalWebSite.Kanban.Services.Settings;
using CnGalWebSite.Kanban.Services.UserDatas;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Core
{
    public class Live2DService : ILive2DService, IDisposable
    {
        private readonly IJSRuntime _jSRuntime;
        private readonly IRepository<ClothesModel> _clothesRepository;
        private readonly IRepository<ExpressionModel> _expressionRepository;
        private readonly IRepository<MotionGroupModel> _motionGroupRepository;
        private readonly IRepository<StockingsModel> _strockingsRepository;
        private readonly IRepository<ShoesModel> _shoesRepository;

        private readonly ISettingService _settingService;
        private readonly IUserDataService _userDataService;
        private DotNetObjectReference<Live2DService>? objRef;
        private readonly IConfiguration _configuration;

        public event Action Live2DInitialized;
        public event Action<string> KanbanImageGenerated;

        public Live2DService(IJSRuntime jSRuntime, IRepository<ClothesModel> clothesRepository, ISettingService settingService, IUserDataService userDataService, IRepository<ExpressionModel> expressionRepository, IRepository<MotionGroupModel> motionGroupRepository,
             IConfiguration configuration, IRepository<ShoesModel> shoesRepository, IRepository<StockingsModel> strockingsRepository)
        {
            _jSRuntime = jSRuntime;
            _clothesRepository = clothesRepository;
            _settingService = settingService;
            _userDataService = userDataService;
            _expressionRepository = expressionRepository;
            _configuration = configuration;
            _motionGroupRepository = motionGroupRepository;
            _shoesRepository = shoesRepository;
            _strockingsRepository = strockingsRepository;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            //创建对象引用
            objRef = DotNetObjectReference.Create(this);

            //获取模型数据
            await _clothesRepository.LoadAsync();
            await _expressionRepository.LoadAsync();
            await _motionGroupRepository.LoadAsync();
            await _strockingsRepository.LoadAsync();
            await _shoesRepository.LoadAsync();

            //读取用户数据
            await _userDataService.LoadAsync();

            //加载UI相关设置
            await _settingService.LoadAsync();

            //初始化Live2D核心
            await InitLive2DAsync();
        }

        [JSInvokable]
        public async Task Live2dInitCallback()
        {
            //设置上次的衣服
            await SetClothes(_userDataService.UserData.Clothes.ClothesName);
            await SetShoes(_userDataService.UserData.Clothes.ShoesName);
            await SetStockings(_userDataService.UserData.Clothes.StockingsName);
            //触发事件
            Live2DInitialized?.Invoke();
        }

        /// <summary>
        /// 初始化Live2D核心
        /// </summary>
        /// <returns></returns>
        private async Task InitLive2DAsync()
        {
            await _jSRuntime.InvokeVoidAsync("initKanbanLive2D", objRef, "cngal形象", 0, _configuration["Live2D_ResourcesPath"]);
        }

        /// <summary>
        /// 设置动作
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetMotion(string group, int index)
        {
            await _jSRuntime.InvokeVoidAsync("switchLiv2DMotion", group, index);
        }

        /// <summary>
        /// 清空动作
        /// </summary>
        /// <returns></returns>
        public async Task CleanMotion()
        {
            await _jSRuntime.InvokeVoidAsync("switchLiv2DMotion", "Idle");
        }


        /// <summary>
        /// 设置表情
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetExpression(string name)
        {
            await _jSRuntime.InvokeVoidAsync("switchLiv2DExpression", name);
        }

        /// <summary>
        /// 清空表情
        /// </summary>
        /// <returns></returns>
        public async Task CleanExpression()
        {
            await _jSRuntime.InvokeVoidAsync("switchLiv2DExpression");
        }

        /// <summary>
        /// 设置衣服
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetClothes(string name)
        {
            if(name == null)
            {
                return;
            }

            _userDataService.UserData.Clothes.ClothesName = name;
            await _userDataService.SaveAsync();


            if (name.EndsWith('0'))
            {
                await _jSRuntime.InvokeVoidAsync("switchLiv2DClothes");
            }
            else
            {
                await _jSRuntime.InvokeVoidAsync("switchLiv2DClothes", name);
            }
        }

        /// <summary>
        /// 设置丝袜
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetStockings(string name)
        {
            if (name == null)
            {
                return;
            }

            _userDataService.UserData.Clothes.StockingsName = name;
            await _userDataService.SaveAsync();

            if (name.EndsWith('0'))
            {
                await _jSRuntime.InvokeVoidAsync("switchLiv2DStockings");
            }
            else
            {
                await _jSRuntime.InvokeVoidAsync("switchLiv2DStockings", name);
            }
        }

        /// <summary>
        /// 设置鞋子
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetShoes(string name)
        {
            if (name == null)
            {
                return;
            }

            _userDataService.UserData.Clothes.ShoesName = name;
            await _userDataService.SaveAsync();



            if (name.EndsWith('0'))
            {
                await _jSRuntime.InvokeVoidAsync("switchLiv2DShoes");
            }
            else
            {
                await _jSRuntime.InvokeVoidAsync("switchLiv2DShoes", name);
            }
        }

        /// <summary>
        /// 设置模型
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetModelAsync(int index)
        {
            await _jSRuntime.InvokeVoidAsync("switchLiv2DModel", index);
        }


        /// <summary>
        /// 设置看板娘坐标
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        [JSInvokable]
        public async Task SetKanbanPosition(int left, int top)
        {
            _settingService.Setting.Kanban.Position.Left = left;
            _settingService.Setting.Kanban.Position.Top = top;

            await _settingService.SaveAsync();
        }

        /// <summary>
        /// 设置按钮组坐标
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        [JSInvokable]
        public async Task SetButtonGroupPosition(int left, int bottom)
        {
            _settingService.Setting.Button.Position.Left = left;
            _settingService.Setting.Button.Position.Bottom = bottom;

            await _settingService.SaveAsync();
        }

        /// <summary>
        /// 设置对话框坐标
        /// </summary>
        /// <param name="left"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        [JSInvokable]
        public async Task SetDialogBoxPosition(int left, int bottom)
        {
            _settingService.Setting.DialogBox.Position.Left = left;
            _settingService.Setting.DialogBox.Position.Bottom = bottom;

            await _settingService.SaveAsync();
        }

        /// <summary>
        /// 开始生成看板娘图片
        /// </summary>
        /// <returns></returns>
        public async Task StartKanbanImageGeneration()
        {
            await _jSRuntime.InvokeVoidAsync("startKanbanImageGeneration", objRef);
        }

        /// <summary>
        /// 看板娘生成图片成功回调
        /// </summary>
        /// <param name="url"></param>
        [JSInvokable]
        public void OnKanbanImageGenerated(string url)
        {
            KanbanImageGenerated?.Invoke(url);
        }

        public void ReleaseLive2D()
        {
            _ = _jSRuntime.InvokeVoidAsync("window.releaseLive2d");
        }

        public void Dispose()
        {
            objRef?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
