using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel.FavoriteFolders;
using CnGalWebSite.DataModel.ExamineModel.PlayedGames;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/Favorites/[action]")]
    public class FavoriteFolderAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Tag, long> _tagRepository;
        private readonly IAppHelper _appHelper;
        private readonly IFavoriteFolderService _favoriteFolderService;
        private readonly IFavoriteObjectService _favoriteObjectService;
        private readonly IEditRecordService _editRecordService;
        private readonly IUserService _userService;


        public FavoriteFolderAPIController(IRepository<FavoriteFolder, long> favoriteFolderRepository, IRepository<Periphery, long> peripheryRepository, IRepository<Video, long> videoRepository, IRepository<Tag, long> tagRepository,
        IRepository<ApplicationUser, string> userRepository, IRepository<FavoriteObject, long> favoriteObjectRepository, IRepository<Examine, long> examineRepository,
        UserManager<ApplicationUser> userManager, IFavoriteObjectService favoriteObjectService, IEditRecordService editRecordService, IUserService userService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFavoriteFolderService favoriteFolderService)
        {
            _userManager = userManager;
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _userRepository = userRepository;
            _favoriteFolderRepository = favoriteFolderRepository;
            _favoriteObjectRepository = favoriteObjectRepository;
            _favoriteFolderService = favoriteFolderService;
            _favoriteObjectService = favoriteObjectService;
            _peripheryRepository = peripheryRepository;
            _videoRepository = videoRepository;
            _tagRepository = tagRepository;
            _examineRepository = examineRepository;
            _editRecordService= editRecordService;
            _userService = userService;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FavoriteFolderViewModel>> GetView(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取文章
            var folder = await _favoriteFolderRepository.GetAll().AsNoTracking()
                .Include(s => s.ApplicationUser)
                .Include(s => s.FavoriteObjects).ThenInclude(s => s.Video).ThenInclude(s=>s.CreateUser)
                .Include(s => s.FavoriteObjects).ThenInclude(s => s.Article).ThenInclude(s => s.CreateUser)
                .Include(s => s.FavoriteObjects).ThenInclude(s => s.Entry).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.FavoriteObjects).ThenInclude(s => s.Periphery)
                .Include(s => s.FavoriteObjects).ThenInclude(s => s.Tag)
                .Include(s => s.Examines).ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (folder == null)
            {
                return NotFound();
            }


            //判断当前是否隐藏
            if ((folder.ShowPublicly == false) && user?.Id != folder.ApplicationUserId)
            {
                if (user == null || await _userManager.IsInRoleAsync(user, "Admin") != true)
                {
                    return NotFound();
                }
            }

            List<Examine> examineQuery = null;


            //读取审核信息
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
                               .Where(s => s.FavoriteFolderId == folder.Id && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EditFavoriteFolderMain ))
                               .Select(s => new Examine
                               {
                                   Operation = s.Operation,
                                   Context = s.Context
                               })
                               .ToListAsync();
            }

            //读取当前登入用户审核信息 获取待审核的内容
            Examine examine = null;
            if (user != null)
            {
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EditFavoriteFolderMain);
                if (examine != null)
                {
                     _favoriteFolderService.UpdateData(folder, examine);
                }
            }

            //建立视图模型
            var model = _favoriteFolderService.GetViewModel(folder);

            if (user != null)
            {
                if (examineQuery.Any(s => s.Operation == Operation.EditFavoriteFolderMain))
                {
                    model.MainState = EditState.Preview;
                }
             
            }

            //复制数据
            var createUser = folder.ApplicationUser;
            if (createUser == null)
            {
                folder.ApplicationUser = createUser = folder.Examines.First(s => s.IsPassed == true).ApplicationUser;
            }


            model.UserInfor = await _userService.GetUserInforViewModel(createUser);
          

            //判断是否有权限编辑
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                model.Authority = true;
            }
            else
            {
                if (user != null && user.Id == folder.ApplicationUser.Id)
                {
                    model.Authority = true;
                }
                else
                {
                    model.Authority = false;
                }
            }

            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.FavoriteFolderId == folder.Id && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }

            //获取各部分状态
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditFavoriteFolderMain))
                    {
                        model.MainState = EditState.Locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
               
            }


            //增加阅读次数
            _favoriteFolderRepository.Clear();
            _ = await _favoriteFolderRepository.GetAll().Where(s => s.Id == folder.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.ReaderCount, b => b.ReaderCount + 1));

            return model;
        }

        /// <summary>
        /// 创建收藏夹
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> CreateFavoriteFolderAsync(CreateFavoriteFolderViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否重名
            if (await _favoriteFolderRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return new Result { Successful = false, Error = "已经存在名称为“" + model.Name + "”的收藏夹" };
            }

            if (model.ShowPublicly)
            {
                var folder = await _favoriteFolderRepository.InsertAsync(new FavoriteFolder
                {
                    CreateTime = DateTime.Now.ToCstTime(),
                    LastEditTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    BriefIntroduction = model.BriefIntroduction,
                    ShowPublicly = false,
                    MainImage = model.MainImage,
                    Name = model.Name,
                });

                var favoriteFolderMain = new FavoriteFolderMain
                {
                    BriefIntroduction = model.BriefIntroduction,
                    ShowPublicly = model.ShowPublicly,
                    MainImage = model.MainImage,
                    Name = model.Name,
                };

                //保存并尝试应用审核记录
                await _editRecordService.SaveAndApplyEditRecord(folder, user, favoriteFolderMain, Operation.EditFavoriteFolderMain, "");

                folder.IsDefault = model.IsDefault;
                folder.IsHidden = model.IsHidden;

                await _favoriteFolderRepository.UpdateAsync(folder);
            }
            else
            {
                await _favoriteFolderRepository.InsertAsync(new FavoriteFolder
                {
                    Name = model.Name,
                    IsDefault = model.IsDefault,
                    BriefIntroduction = model.BriefIntroduction,
                    ApplicationUserId = user.Id,
                    CreateTime = DateTime.Now.ToCstTime(),
                    LastEditTime = DateTime.Now.ToCstTime(),
                    IsHidden = model.IsHidden,
                    ShowPublicly = model.ShowPublicly,
                    MainImage = model.MainImage,

                });

            }

            return new Result { Successful = true };
        }

        /// <summary>
        /// 添加对象到收藏夹
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> AddFavoriteObjectAsync(AddFavoriteObjectViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var favoriteFolders = await _favoriteFolderRepository.GetAll().Where(s => model.FavoriteFolderIds.Contains(s.Id) && s.ApplicationUserId == user.Id).ToListAsync();
            if (favoriteFolders == null || favoriteFolders.Count == 0)
            {
                return new Result { Successful = false, Error = "无法找到收藏夹" };
            }

            //查找对应对象
            if (model.Type == FavoriteObjectType.Article)
            {
                foreach (var temp in favoriteFolders)
                {
                    //查找是否已经添加
                    if (await _favoriteObjectRepository.GetAll().AnyAsync(s => s.FavoriteFolderId == temp.Id && s.ArticleId == model.ObjectId))
                    {
                        continue;
                    }

                    var item = await _articleRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (item == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该文章" };

                    }
                    await _favoriteObjectRepository.InsertAsync(new FavoriteObject
                    {
                        FavoriteFolder = temp,
                        FavoriteFolderId = temp.Id,
                        ArticleId = item.Id,
                        Type = FavoriteObjectType.Article,
                        CreateTime = DateTime.Now.ToCstTime()
                    });
                }
            }
            else if (model.Type == FavoriteObjectType.Entry)
            {
                foreach (var temp in favoriteFolders)
                {
                    //查找是否已经添加
                    if (await _favoriteObjectRepository.GetAll().AnyAsync(s => s.FavoriteFolderId == temp.Id && s.EntryId == model.ObjectId))
                    {
                        continue;
                    }

                    var item = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (item == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该词条" };

                    }
                    await _favoriteObjectRepository.InsertAsync(new FavoriteObject
                    {
                        FavoriteFolder = temp,
                        FavoriteFolderId = temp.Id,
                        EntryId = item.Id,
                        Type = FavoriteObjectType.Entry,
                        CreateTime = DateTime.Now.ToCstTime()
                    });
                }

            }
            else if (model.Type == FavoriteObjectType.Periphery)
            {
                foreach (var temp in favoriteFolders)
                {
                    //查找是否已经添加
                    if (await _favoriteObjectRepository.GetAll().AnyAsync(s => s.FavoriteFolderId == temp.Id && s.PeripheryId == model.ObjectId))
                    {
                        continue;
                    }

                    var item = await _peripheryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (item == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该周边" };

                    }
                    await _favoriteObjectRepository.InsertAsync(new FavoriteObject
                    {
                        FavoriteFolder = temp,
                        FavoriteFolderId = temp.Id,
                        PeripheryId = item.Id,
                        Type = FavoriteObjectType.Periphery,
                        CreateTime = DateTime.Now.ToCstTime()
                    });
                }

            }
            else if (model.Type == FavoriteObjectType.Video)
            {
                foreach (var temp in favoriteFolders)
                {
                    //查找是否已经添加
                    if (await _favoriteObjectRepository.GetAll().AnyAsync(s => s.FavoriteFolderId == temp.Id && s.VideoId == model.ObjectId))
                    {
                        continue;
                    }

                    var item = await _videoRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (item == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该视频" };

                    }
                    await _favoriteObjectRepository.InsertAsync(new FavoriteObject
                    {
                        FavoriteFolder = temp,
                        FavoriteFolderId = temp.Id,
                        VideoId = item.Id,
                        Type = FavoriteObjectType.Video,
                        CreateTime = DateTime.Now.ToCstTime()
                    });
                }

            }
            else if (model.Type == FavoriteObjectType.Tag)
            {
                foreach (var temp in favoriteFolders)
                {
                    //查找是否已经添加
                    if (await _favoriteObjectRepository.GetAll().AnyAsync(s => s.FavoriteFolderId == temp.Id && s.TagId == model.ObjectId))
                    {
                        continue;
                    }

                    var item = await _tagRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (item == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该标签" };

                    }
                    await _favoriteObjectRepository.InsertAsync(new FavoriteObject
                    {
                        FavoriteFolder = temp,
                        FavoriteFolderId = temp.Id,
                        TagId = item.Id,
                        Type = FavoriteObjectType.Tag,
                        CreateTime = DateTime.Now.ToCstTime()
                    });
                }

            }

            //更新数目
            await _appHelper.UpdateFavoritesCountAsync(model.FavoriteFolderIds);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 在词条页面调用 删除该词条的所有收藏
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> UnFavoriteObjectsAsync(UnFavoriteObjectsModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取所有收藏夹Id
            var favoriteFolderIds = await _favoriteFolderRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).Select(s => s.Id).ToArrayAsync();

            if (model.Type == FavoriteObjectType.Entry)
            {
                await _favoriteObjectRepository.GetAll().Where(s => s.EntryId == model.ObjectId && s.Type == FavoriteObjectType.Entry && favoriteFolderIds.Contains(s.FavoriteFolderId)).ExecuteDeleteAsync();
            }
            else if (model.Type == FavoriteObjectType.Article)
            {
                await _favoriteObjectRepository.GetAll().Where(s => s.ArticleId == model.ObjectId && s.Type == FavoriteObjectType.Article && favoriteFolderIds.Contains(s.FavoriteFolderId)).ExecuteDeleteAsync();
            }
            else if (model.Type == FavoriteObjectType.Periphery)
            {
                await _favoriteObjectRepository.GetAll().Where(s => s.PeripheryId == model.ObjectId && s.Type == FavoriteObjectType.Periphery && favoriteFolderIds.Contains(s.FavoriteFolderId)).ExecuteDeleteAsync();
            }
            else if (model.Type == FavoriteObjectType.Video)
            {
                await _favoriteObjectRepository.GetAll().Where(s => s.VideoId == model.ObjectId && s.Type == FavoriteObjectType.Video && favoriteFolderIds.Contains(s.FavoriteFolderId)).ExecuteDeleteAsync();
            }
            else if (model.Type == FavoriteObjectType.Tag)
            {
                await _favoriteObjectRepository.GetAll().Where(s => s.TagId == model.ObjectId && s.Type == FavoriteObjectType.Tag && favoriteFolderIds.Contains(s.FavoriteFolderId)).ExecuteDeleteAsync();
            }
            //更新数目
            await _appHelper.UpdateFavoritesCountAsync(favoriteFolderIds);
            return new Result { Successful = true };
        }

        /// <summary>
        /// 获取用户收藏夹列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<FavoriteFoldersViewModel> GetUserFavoriteFolders(string userId)
        {
            var model = new FavoriteFoldersViewModel
            {
                Favorites = new List<FavoriteFolderAloneModel>()
            };
            //获取当前用户ID
            var currentUser = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = false;
            if (currentUser != null)
            {
                isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            var user = await _userRepository.GetAll().Include(s => s.FavoriteFolders).FirstOrDefaultAsync(s => s.Id == userId);
            if (user == null)
            {
                return null;
            }

            //如果没有收藏夹则创建默认收藏夹
            if (user.FavoriteFolders.Count == 0)
            {
                var favoritFolder = new FavoriteFolder
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    Name = "默认收藏夹",
                    BriefIntroduction = "系统自动创建的默认收藏夹",
                    IsDefault = true,
                    CreateTime = DateTime.Now.ToCstTime()
                };
                user.FavoriteFolders.Add(favoritFolder);
                await _favoriteFolderRepository.InsertAsync(favoritFolder);
            }


            foreach (var item in user.FavoriteFolders.Where(s => s.IsHidden == false && (currentUser.Id == userId || s.ShowPublicly)))
            {
                model.Favorites.Add(new FavoriteFolderAloneModel
                {
                    Id = item.Id,
                    BriefIntroduction = item.BriefIntroduction,
                    IsDefault = item.IsDefault,
                    MainImage = _appHelper.GetImagePath(item.MainImage, "app.png"),
                    Count = item.Count,
                    CreateTime = item.CreateTime,
                    Name = item.Name
                });
            }

            return model;
        }

        /// <summary>
        /// 获取用户收藏夹列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<FavoriteFoldersViewModel>> GetUserFavoriteFoldersAsync(string id)
        {
            return await GetUserFavoriteFolders(id);
        }

        /// <summary>
        /// 通过单个收藏夹Id获取所有收藏夹列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<FavoriteFoldersViewModel>> GetUserFavoriteInforFromFolderIdAsync(string id)
        {
            //获取关联用户Id
            var currentUserId = (await _favoriteFolderRepository.GetAll().FirstOrDefaultAsync(s => s.Id.ToString() == id))?.ApplicationUserId;
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return NotFound("未找到该收藏夹");
            }

            return await GetUserFavoriteFolders(currentUserId);
        }


        /// <summary>
        /// 在用户管理收藏词条页面调用 删除指定收藏
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> UserDeleteFavoriteObjectAsync(DeleteFavoriteObjectsModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //判断是否为当前用户
            if (isAdmin == false && await _favoriteFolderRepository.GetAll().AnyAsync(s => s.Id == model.FavorieFolderId && s.ApplicationUserId == user.Id) == false)
            {
                return new Result { Successful = false, Error = "访问被拒绝" };
            }

            await _favoriteObjectRepository.GetAll().Where(s => s.FavoriteFolderId == model.FavorieFolderId && model.Ids.Contains(s.Id)).ExecuteDeleteAsync();

            //更新数目
            await _appHelper.UpdateFavoritesCountAsync(new long[] { model.FavorieFolderId });

            return new Result { Successful = true };
        }

        /// <summary>
        /// 删除收藏夹
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> UserDeleteFavoriteFolderAsync(DeleteFavoriteFoldersModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            await _favoriteFolderRepository.GetAll().Where(s => (isAdmin || s.ApplicationUserId == user.Id) && model.Ids.Contains(s.Id)).ExecuteDeleteAsync();

            return new Result { Successful = true };

        }

        /// <summary>
        /// 设置默认收藏夹
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> UserSetFavoriteFolderDefaultAsync(SetDefaultFavoriteFolderModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //判断是否为当前用户
            if (isAdmin == false && await _favoriteFolderRepository.GetAll().CountAsync(s => model.Ids.Contains(s.Id) && s.ApplicationUserId == user.Id) == model.Ids.Length)
            {
                return new Result { Successful = false, Error = "访问被拒绝" };
            }

            await _favoriteFolderRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsDefault, b => model.IsDefault));

            return new Result { Successful = true };
        }


        /// <summary>
        /// 获取收藏夹列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFavoriteFolderAloneModel>>> GetFavoriteFolderListAsync(FavoriteFoldersPagesInfor input)
        {
            //检查 目标用户id与当前用户id不一致 必须要求管理员身份
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //判断是否为当前用户
            if (isAdmin == false && input.UserId != user.Id)
            {
                return NotFound();
            }

            var dtos = await _favoriteFolderService.GetPaginatedResult(input.Options, input.SearchModel, input.UserId);

            return dtos;
        }

        /// <summary>
        /// 获取收藏对象列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFavoriteObjectAloneModel>>> GetFavoriteObjectListAsync(FavoriteObjectsPagesInfor input)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //判断是否为当前用户
            if (isAdmin == false && await _favoriteFolderRepository.GetAll().AnyAsync(s => s.Id == input.FavoriteFolderId && s.ApplicationUserId == user.Id) == false)
            {
                return NotFound();
            }

            var dtos = await _favoriteObjectService.GetPaginatedResult(input.Options, input.SearchModel, input.FavoriteFolderId);

            return dtos;
        }

        /// <summary>
        /// 获取用户收藏对象列表
        /// </summary>
        /// <param name="maxResultCount"></param>
        /// <param name="currentPage"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<PagedResultDto<FavoriteObjectAloneViewModel>> GetUserFavoriteObjectListAsync([FromQuery] int maxResultCount, [FromQuery] int currentPage, [FromQuery] long folderId)
        {
            var currentUser = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            var favoriteFolder = await _favoriteFolderRepository.GetAll().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == folderId);
            if (favoriteFolder == null || favoriteFolder.ApplicationUser == null)
            {
                return null;
            }
            var user = favoriteFolder.ApplicationUser;


            //判断是否为管理员
            var isAdmin = false;
            if (currentUser != null)
            {
                isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            //判断是否有权限访问
            if (favoriteFolder.ShowPublicly == false)
            {
                if (isAdmin == false && user.Id != currentUser.Id)
                {
                    return new PagedResultDto<FavoriteObjectAloneViewModel>() { Data = new List<FavoriteObjectAloneViewModel>() };
                }
            }

            return await _favoriteObjectService.GetPaginatedResult(new PagedSortedAndFilterInput
            {
                CurrentPage = currentPage,
                MaxResultCount = maxResultCount,
            }, folderId);
        }

        /// <summary>
        /// 判断是否被收藏
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("{id}/{type}")]
        public async Task<ActionResult<IsObjectInUserFavoriteFolderResult>> IsObjectInUserFavoriteFolderAsync(long id, FavoriteObjectType type)
        {

            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取所有收藏夹ID
            var favoriteFolderIds = await _favoriteFolderRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).Select(s => s.Id).ToListAsync();
            //查找是否存在
            if (type == FavoriteObjectType.Entry)
            {
                return new IsObjectInUserFavoriteFolderResult
                {
                    Result = await _favoriteObjectRepository.GetAll().AnyAsync(s => favoriteFolderIds.Contains(s.FavoriteFolderId) && s.EntryId == id)
                };
            }
            else if (type == FavoriteObjectType.Article)
            {
                return new IsObjectInUserFavoriteFolderResult
                {
                    Result = await _favoriteObjectRepository.GetAll().AnyAsync(s => favoriteFolderIds.Contains(s.FavoriteFolderId) && s.ArticleId == id)
                };
            }
            else if (type == FavoriteObjectType.Periphery)
            {
                return new IsObjectInUserFavoriteFolderResult
                {
                    Result = await _favoriteObjectRepository.GetAll().AnyAsync(s => favoriteFolderIds.Contains(s.FavoriteFolderId) && s.PeripheryId == id)
                };
            }
            else if (type == FavoriteObjectType.Video)
            {
                return new IsObjectInUserFavoriteFolderResult
                {
                    Result = await _favoriteObjectRepository.GetAll().AnyAsync(s => favoriteFolderIds.Contains(s.FavoriteFolderId) && s.VideoId == id)
                };
            }
            else if (type == FavoriteObjectType.Tag)
            {
                return new IsObjectInUserFavoriteFolderResult
                {
                    Result = await _favoriteObjectRepository.GetAll().AnyAsync(s => favoriteFolderIds.Contains(s.FavoriteFolderId) && s.TagId == id)
                };
            }

            return NotFound();
        }

        /// <summary>
        /// 编辑收藏夹
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<EditFavoriteFolderViewModel>> EditFavoriteFolderAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //获取收藏夹
            var folder = await _favoriteFolderRepository.GetAll().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == id);
            if (folder == null)
            {
                return NotFound();
            }
            //判断是否有权限编辑
            if (folder.ApplicationUserId != user.Id && isAdmin == false)
            {
                return NotFound();
            }

            //获取审核记录
            var examines = await _examineRepository.GetAllListAsync(s => s.FavoriteFolderId ==id && s.ApplicationUserId == user.Id
              && (s.Operation == Operation.EditFavoriteFolderMain) && s.IsPassed == null);

            var examine = examines.FirstOrDefault(s => s.Operation == Operation.EditFavoriteFolderMain);
            if (examine != null)
            {
                _favoriteFolderService.UpdateData(folder, examine);
            }

            EditFavoriteFolderViewModel model = new();
            model.Name = folder.Name;
            model.Id = folder.Id;
            model.BriefIntroduction = folder.BriefIntroduction;
            model.IsDefault = folder.IsDefault;
            model.IsHidden=folder.IsHidden;
            model.ShowPublicly=folder.ShowPublicly;
            model.MainImage = folder.MainImage;

            return model;
        }

        /// <summary>
        /// 编辑收藏夹
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> EditFavoriteFolderAsync(EditFavoriteFolderViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //获取收藏夹
            var folder = await _favoriteFolderRepository.GetAll().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (folder == null)
            {
                return NotFound();
            }
            //判断是否有权限编辑
            if (folder.ApplicationUserId != user.Id && isAdmin == false)
            {
                return NotFound();
            }


            if (model.Name != folder.Name || model.BriefIntroduction != folder.BriefIntroduction || model.ShowPublicly != folder.ShowPublicly || model.MainImage != folder.MainImage)
            {
                if (model.ShowPublicly)
                {
                    var favoriteFolderMain = new FavoriteFolderMain
                    {
                        BriefIntroduction = model.BriefIntroduction,
                        ShowPublicly = model.ShowPublicly,
                        MainImage = model.MainImage,
                        Name = model.Name,
                    };

                    //保存并尝试应用审核记录
                    await _editRecordService.SaveAndApplyEditRecord(folder, user, favoriteFolderMain, Operation.EditFavoriteFolderMain, "");
                }
            }
            else
            {
                folder.Name = model.Name;
                folder.BriefIntroduction = model.BriefIntroduction;
                folder.MainImage = model.MainImage;
                folder.ShowPublicly = model.ShowPublicly;
            }

            folder.LastEditTime = DateTime.Now.ToCstTime();
            folder.IsDefault = model.IsDefault;         
            folder.IsHidden = model.IsHidden;
          
            await _favoriteFolderRepository.UpdateAsync(folder);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 移动收藏对象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> MoveFavoriteObjectsAsync(MoveFavoriteObjectsModel model)
        {
            //检查是否有权限移动
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //获取关联用户Id
            var currentUserId = (await _favoriteFolderRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.CurrentFolderId))?.ApplicationUserId;
            //判断是否有权限编辑
            if (currentUserId != user.Id && isAdmin == false)
            {
                return NotFound("没有权限编辑");
            }

            //获取收藏夹
            var folders = await _favoriteFolderRepository.GetAll().Include(s => s.ApplicationUser).Where(s => s.ApplicationUserId == currentUserId).ToListAsync();
            if (folders == null || folders.Count == 0)
            {
                return NotFound("未找到收藏夹");
            }
            var folderIds = folders.Select(s => s.Id);

            //清除选定目标的关联收藏夹
            var entries = model.ObjectIds.Where(s => s.Key == FavoriteObjectType.Entry).Select(s => s.Value);
            var articles = model.ObjectIds.Where(s => s.Key == FavoriteObjectType.Article).Select(s => s.Value);
            var tags = model.ObjectIds.Where(s => s.Key == FavoriteObjectType.Tag).Select(s => s.Value);
            var peripheries = model.ObjectIds.Where(s => s.Key == FavoriteObjectType.Periphery).Select(s => s.Value);
            var videos = model.ObjectIds.Where(s => s.Key == FavoriteObjectType.Video).Select(s => s.Value);
            await _favoriteObjectRepository.GetAll().Where(s => folderIds.Contains(s.FavoriteFolderId) && s.Type == FavoriteObjectType.Entry && entries.Contains((long)s.EntryId)).ExecuteDeleteAsync();
            await _favoriteObjectRepository.GetAll().Where(s => folderIds.Contains(s.FavoriteFolderId) && s.Type == FavoriteObjectType.Article && articles.Contains((long)s.ArticleId)).ExecuteDeleteAsync();
            await _favoriteObjectRepository.GetAll().Where(s => folderIds.Contains(s.FavoriteFolderId) && s.Type == FavoriteObjectType.Periphery && peripheries.Contains((long)s.PeripheryId)).ExecuteDeleteAsync();
            await _favoriteObjectRepository.GetAll().Where(s => folderIds.Contains(s.FavoriteFolderId) && s.Type == FavoriteObjectType.Tag && tags.Contains((long)s.TagId)).ExecuteDeleteAsync();
            await _favoriteObjectRepository.GetAll().Where(s => folderIds.Contains(s.FavoriteFolderId) && s.Type == FavoriteObjectType.Video && articles.Contains((long)s.VideoId)).ExecuteDeleteAsync();

            //添加到新收藏夹
            foreach (var item in model.FolderIds)
            {
                foreach (var infor in model.ObjectIds)
                {
                    await _favoriteObjectRepository.InsertAsync(new FavoriteObject
                    {
                        Type = infor.Key,
                        ArticleId = infor.Key == FavoriteObjectType.Article ? infor.Value : null,
                        EntryId = (infor.Key == FavoriteObjectType.Entry ? (int)infor.Value : null),
                        PeripheryId = infor.Key == FavoriteObjectType.Periphery ? infor.Value : null,
                        VideoId = infor.Key == FavoriteObjectType.Video ? infor.Value : null,
                        TagId = infor.Key == FavoriteObjectType.Tag ? (int)infor.Value : null,
                        CreateTime = DateTime.Now.ToCstTime(),
                        FavoriteFolderId = item
                    });
                }
            }

            //更新收藏夹数
            await _appHelper.UpdateFavoritesCountAsync(folderIds.ToArray());

            return new Result { Successful = true };
        }

        /// <summary>
        /// 撤销编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> RevokeExamine(RevokeExamineModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //查找审核
            var examine = await _examineRepository.FirstOrDefaultAsync(s => s.FavoriteFolderId == model.Id && s.ApplicationUserId == user.Id && s.Operation == model.ExamineType && s.IsPassed == null);
            if (examine != null)
            {
                await _examineRepository.DeleteAsync(examine);
                //删除以此审核为前置审核的
                await _examineRepository.DeleteAsync(s => s.PrepositionExamineId == examine.Id);
                return new Result { Successful = true };
            }
            else
            {
                return new Result { Successful = false, Error = "找不到目标审核记录" };
            }

        }

        /// <summary>
        /// 获取词条文章标签周边视频的相关收藏夹
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<FavoriteFolderAloneModel>>> GetRelateFavoriteFolders([FromQuery] FavoriteObjectType type, [FromQuery] long id)
        {
            var folders = type switch
            {
                FavoriteObjectType.Entry => await _favoriteFolderRepository.GetAll().AsNoTracking().Where(s =>s.ShowPublicly&& s.FavoriteObjects.Any(s => s.EntryId == id)).ToListAsync(),
                FavoriteObjectType.Article => await _favoriteFolderRepository.GetAll().AsNoTracking().Where(s => s.ShowPublicly && s.FavoriteObjects.Any(s => s.ArticleId == id)).ToListAsync(),
                FavoriteObjectType.Tag => await _favoriteFolderRepository.GetAll().AsNoTracking().Where(s => s.ShowPublicly && s.FavoriteObjects.Any(s => s.TagId == id)).ToListAsync(),
                FavoriteObjectType.Periphery => await _favoriteFolderRepository.GetAll().AsNoTracking().Where(s => s.ShowPublicly && s.FavoriteObjects.Any(s => s.PeripheryId == id)).ToListAsync(),
                FavoriteObjectType.Video => await _favoriteFolderRepository.GetAll().AsNoTracking().Where(s => s.ShowPublicly && s.FavoriteObjects.Any(s => s.VideoId == id)).ToListAsync(),
                _ => null
            } ;

            if (folders == null|| folders.Any()==false)
            {
                return new List<FavoriteFolderAloneModel>();
            }

            var model =new List<FavoriteFolderAloneModel>();

            foreach(var item in folders)
            {
                model.Add(new FavoriteFolderAloneModel
                {
                    Id = item.Id,
                    ShowPublicly = item.ShowPublicly,
                    BriefIntroduction = item.BriefIntroduction,
                    Count = item.Count,
                    CreateTime = item.CreateTime,
                    IsDefault = item.IsDefault,
                    IsHidden = item.IsHidden,
                    MainImage = _appHelper.GetImagePath(item.MainImage,"app.png"),
                    Name = item.Name,
                });
            }

            return  model;

        }
    }
}
