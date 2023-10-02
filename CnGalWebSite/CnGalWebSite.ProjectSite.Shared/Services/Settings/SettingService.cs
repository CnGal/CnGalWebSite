using Blazored.LocalStorage;
using CnGalWebSite.ProjectSite.Shared.Models.Themes;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Services.Themes
{
    public class SettingService:ISettingService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IJSRuntime _jsruntime;

        private SettingModel _settings = new();

        private const string _key = "settings";

        public SettingModel Settings => _settings;

        public event Action SettingChanged;

        /// <summary>
        /// 是否为App模式
        /// </summary>
        public bool IsApp { get; set; }

        public SettingService(ILocalStorageService localStorageService, IJSRuntime jsruntime)
        {
            _localStorageService = localStorageService;
            _jsruntime = jsruntime;
        }

        public async Task LoadAsync()
        {
            _settings = await _localStorageService.GetItemAsync<SettingModel>(_key);

            //保存数据
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            //没有数据则重置
            if (_settings == null)
            {
                Reset();
            }

            await CheckTheme();

            //保存
            await _localStorageService.SetItemAsync(_key, _settings);

            OnSettingChanged();
        }

        /// <summary>
        /// 检查主题设置
        /// </summary>
        public async Task CheckTheme()
        {
            //获取系统主题
            if (_settings.ThemeMode == ThemeMode.System)
            {
                _settings.IsDark = await _jsruntime.InvokeAsync<bool>("checkSystemThemeIsDark");
            }
            else
            {
                _settings.IsDark = _settings.ThemeMode == ThemeMode.Dark;
            }
        }

        private void Reset()
        {
            _settings = new SettingModel();
        }

        public async Task ResetAsync()
        {
            Reset();
            await SaveAsync();
        }

        public void OnSettingChanged()
        {
            SettingChanged?.Invoke();
        }

    }
}
