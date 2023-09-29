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
    public class UserDataService : IUserDataService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IRepository<ClothesModel> _clothesRepository;
        private readonly IRepository<StockingsModel> _strockingsRepository;
        private readonly IRepository<ShoesModel> _shoesRepository;


        private UserDataModel _userDataModel = new();

        private const string _key = "kanban_userdata";

        public UserDataModel UserData
        {
            get
            {
                return _userDataModel;
            }
        }

        public UserDataService(ILocalStorageService localStorageService, IRepository<ClothesModel> clothesRepository, IRepository<StockingsModel> strockingsRepository, IRepository<ShoesModel> shoesRepository)
        {
            _localStorageService = localStorageService;
            _clothesRepository = clothesRepository;
            _strockingsRepository = strockingsRepository;
            _shoesRepository = shoesRepository;
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

        public async Task ResetAsync()
        {
            Reset();
            await SaveAsync();
        }

        /// <summary>
        /// 检查服装是否存在
        /// </summary>
        /// <returns></returns>
        private void CheckClothes()
        {
            if (_userDataModel.Clothes.ClothesName != null && !_clothesRepository.GetAll().Any(s => s.Name == _userDataModel.Clothes.ClothesName))
            {
                _userDataModel.Clothes.ClothesName = null;
            }
            if (_userDataModel.Clothes.StockingsName != null && !_strockingsRepository.GetAll().Any(s => s.Name == _userDataModel.Clothes.StockingsName))
            {
                _userDataModel.Clothes.StockingsName = null;
            }
            if (_userDataModel.Clothes.ShoesName != null && !_shoesRepository.GetAll().Any(s => s.Name == _userDataModel.Clothes.ShoesName))
            {
                _userDataModel.Clothes.ShoesName = null;
            }
        }
    }
}
