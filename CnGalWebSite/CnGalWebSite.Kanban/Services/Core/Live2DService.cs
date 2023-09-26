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
        private readonly ISettingService _settingService;
        private readonly IUserDataService _userDataService;
        private DotNetObjectReference<Live2DService>? objRef;
        private readonly IConfiguration _configuration;

        public Live2DService(IJSRuntime jSRuntime, IRepository<ClothesModel> clothesRepository, ISettingService settingService, IUserDataService userDataService, IRepository<ExpressionModel> expressionRepository, IRepository<MotionGroupModel> motionGroupRepository,
             IConfiguration configuration)
        {
            _jSRuntime = jSRuntime;
            _clothesRepository = clothesRepository;
            _settingService = settingService;
            _userDataService = userDataService;
            _expressionRepository = expressionRepository;
            _configuration= configuration;
            _motionGroupRepository = motionGroupRepository;
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

            //读取用户数据
            await _userDataService.LoadAsync();

            //加载UI相关设置
            await _settingService.LoadAsync();

            //初始化Live2D核心
            await InitLive2DAsync();
        }

        /// <summary>
        /// 初始化Live2D核心
        /// </summary>
        /// <returns></returns>
        private async Task InitLive2DAsync()
        {
            var clothesString = string.Join(",", _clothesRepository.GetAll().Select(s => s.Name));
            var clothesIndex = _userDataService.GetCurrentClothesIndex();

            await _jSRuntime.InvokeVoidAsync("initKanbanLive2D", objRef, clothesString, clothesIndex, _configuration["Live2D_ResourcesPath"]);

        }

        /// <summary>
        /// 设置动作
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetMotion(string group,int index)
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
        public async Task SetExpression(int index)
        {
            await _jSRuntime.InvokeVoidAsync("switchLiv2DExpression", _expressionRepository.GetAll()[index].Name);
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
        /// 设置服装
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetClothesAsync(int index)
        {
            await _userDataService.SetCurrentClothesIndex(index);
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

        public void Dispose()
        {
            objRef?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
