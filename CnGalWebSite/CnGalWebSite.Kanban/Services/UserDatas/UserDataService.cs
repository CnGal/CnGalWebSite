using Blazored.LocalStorage;
using Live2DTest.DataRepositories;
using CnGalWebSite.Kanban.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.UserDatas
{
    public class UserDataService:IUserDataService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IRepository<ClothesModel> _clothesRepository;


        private UserDataModel _userDataModel = new();

        private const string _key = "kanban_userdata";

        public UserDataModel UserData
        {
            get
            {
                return _userDataModel;
            }
        }

        public UserDataService(ILocalStorageService localStorageService, IRepository<ClothesModel> clothesRepository)
        {
            _localStorageService = localStorageService;
            _clothesRepository = clothesRepository;
        }

        public async Task LoadAsync()
        {
            _userDataModel = await _localStorageService.GetItemAsync<UserDataModel>(_key);

            //保存数据
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            //没有数据则重置
            if (_userDataModel == null)
            {
                Reset();
            }
            //服装
            CheckClothes();

            //保存
            await _localStorageService.SetItemAsync(_key, _userDataModel);
        }

        public void Reset()
        {
            _userDataModel = new UserDataModel();
        }

        /// <summary>
        /// 检查服装是否存在
        /// </summary>
        /// <returns></returns>
        private void CheckClothes()
        {
            if (!_clothesRepository.GetAll().Any(s => s.Name == _userDataModel.Clothes.CurrentName))
            {
                _userDataModel.Clothes.CurrentName = _clothesRepository.GetAll().FirstOrDefault()?.Name;
            }
        }

        /// <summary>
        /// 获取当前服装索引
        /// </summary>
        /// <returns></returns>
        public int GetCurrentClothesIndex()
        {
            var list = _clothesRepository.GetAll();
            return list.IndexOf(list.FirstOrDefault(s => s.Name == _userDataModel.Clothes.CurrentName));
        }

        /// <summary>
        /// 设置当前服装
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task SetCurrentClothesIndex(int index)
        {
            _userDataModel.Clothes.CurrentName = _clothesRepository.GetAll()[index].Name;
            await SaveAsync();
        }
    }
}
