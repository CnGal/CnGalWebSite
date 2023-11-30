using BlazorComponent;
using Blazored.LocalStorage;
using CnGalWebSite.Kanban.Models;
using Masa.Blazor;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Settings
{
    public class SettingService : ISettingService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IJSRuntime _jsRuntime;

        public event Action OnSettingChanged;

        private SettingModel _settingModel = new();

        private const string _key = "kanban_live2d_setting";

        public SettingModel Setting
        {
            get
            {
                return _settingModel;
            }
        }

        public SettingService(ILocalStorageService localStorageService, IJSRuntime jsRuntime)
        {
            _localStorageService = localStorageService;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadAsync()
        {
            _settingModel = await _localStorageService.GetItemAsync<SettingModel>(_key);

            //保存数据
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            //没有数据则重置
            if (_settingModel == null)
            {
                await _resetAsync();
            }

            //检查看板娘是否超出屏幕范围
            await CheckKanbanPositionAsync();
            //检查按钮组
            CheckButtonGrouposition();
            //对话框
            CheckDialogBoxosition();

            //保存
            await _localStorageService.SetItemAsync(_key, _settingModel);

            OnSettingChanged?.Invoke();
        }

        private async Task _resetAsync()
        {
            _settingModel = new SettingModel();

            //设置看板娘位置
            var size = await _jsRuntime.InvokeAsync<Size>("getWindowSize");

            if(size.Width<1600)
            {
                _settingModel.Kanban.Size.Width = 220;
                _settingModel.Kanban.Size.Height = 400;

                _settingModel.Button.Position.Left = 140;

                _settingModel.DialogBox.Position.Left = -200;
                _settingModel.DialogBox.Position.Bottom =410;

            }

            _settingModel.Kanban.Position.Left = size.Width - _settingModel.Kanban.Size.Width;
            _settingModel.Kanban.Position.Top = size.Height - _settingModel.Kanban.Size.Height;
        }

        public async Task ResetAsync()
        {
            await _resetAsync();
            await SaveAsync();
        }

        /// <summary>
        /// 检查看板娘位置是否正确
        /// </summary>
        /// <returns></returns>
        private async Task CheckKanbanPositionAsync()
        {
            var size = await _jsRuntime.InvokeAsync<Size>("getWindowSize");
            var percent = 0.8;

            if (_settingModel.Kanban.Position.Left + _settingModel.Kanban.Size.Width * percent < 0)
            {
                _settingModel.Kanban.Position.Left = 0;
            }

            if (_settingModel.Kanban.Position.Left + _settingModel.Kanban.Size.Width * (1 - percent) > size.Width)
            {
                _settingModel.Kanban.Position.Left = (int)(size.Width - _settingModel.Kanban.Size.Width);
            }

            if (_settingModel.Kanban.Position.Top + _settingModel.Kanban.Size.Height * percent < 0)
            {
                _settingModel.Kanban.Position.Top = 0;
            }

            if (_settingModel.Kanban.Position.Top + _settingModel.Kanban.Size.Height * (1 - percent) > size.Height)
            {
                _settingModel.Kanban.Position.Top = (int)(size.Height - _settingModel.Kanban.Size.Height);
            }
        }

        /// <summary>
        /// 检查按钮组位置是否正确
        /// </summary>
        /// <returns></returns>
        private void CheckButtonGrouposition()
        {
            if (_settingModel.Button.Position.Left < -_settingModel.Kanban.Size.Width)
            {
                _settingModel.Button.Position.Left = -_settingModel.Kanban.Size.Width;
            }

            if (_settingModel.Button.Position.Left > _settingModel.Kanban.Size.Width * 2)
            {
                _settingModel.Button.Position.Left = _settingModel.Kanban.Size.Width * 2;
            }

            if (_settingModel.Button.Position.Bottom < -_settingModel.Kanban.Size.Height)
            {
                _settingModel.Button.Position.Bottom = -_settingModel.Kanban.Size.Height;
            }

            if (_settingModel.Button.Position.Bottom > _settingModel.Kanban.Size.Height * 2)
            {
                _settingModel.Button.Position.Bottom = _settingModel.Kanban.Size.Height * 2;
            }
        }

        /// <summary>
        /// 检查对话框位置是否正确
        /// </summary>
        /// <returns></returns>
        private void CheckDialogBoxosition()
        {
            if (_settingModel.DialogBox.Position.Left < -_settingModel.Kanban.Size.Width)
            {
                _settingModel.DialogBox.Position.Left = -_settingModel.Kanban.Size.Width;
            }

            if (_settingModel.DialogBox.Position.Left > _settingModel.Kanban.Size.Width * 4)
            {
                _settingModel.DialogBox.Position.Left = _settingModel.Kanban.Size.Width * 4;
            }

            if (_settingModel.DialogBox.Position.Bottom < -_settingModel.Kanban.Size.Height)
            {
                _settingModel.DialogBox.Position.Bottom = -_settingModel.Kanban.Size.Height;
            }

            if (_settingModel.DialogBox.Position.Bottom > _settingModel.Kanban.Size.Height * 4)
            {
                _settingModel.DialogBox.Position.Bottom = _settingModel.Kanban.Size.Height * 4;
            }
        }

    }
}
