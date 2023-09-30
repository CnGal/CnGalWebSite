using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.Extensions;
using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using IdentityModel;
using System.Security.Claims;

namespace CnGalWebSite.ProjectSite.API.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpService _httpService;
        private readonly IConfiguration _configuration;

        public UserService(IRepository<ApplicationUser, string> userRepository, IHttpContextAccessor httpContextAccessor, IHttpService httpService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _httpService = httpService;
            _configuration = configuration;
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var id = _httpContextAccessor.HttpContext.User?.Claims?.GetUserId();
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (user != null)
            {
                return user;
            }

            //新用户
            var (email, name) = _httpContextAccessor.HttpContext.User.Claims.GetExternalUserInfor();
            if (await _userRepository.AnyAsync(s => s.Email == email))
            {
                email = null;
            }
            while (await _userRepository.AnyAsync(s => s.UserName == name))
            {
                name = "用户_" + new Random(DateTime.Now.Millisecond).Next(0, 99999).ToString();
            }
            //获取CnGal信息
            var cngal = await GetCnGalUserInfo(id);

            user = new ApplicationUser
            {
                Id = id,
                UserName = name,
                NormalizedUserName = name.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                RegistTime = DateTime.Now.ToCstTime(),
                Avatar=cngal?.PhotoPath,
                BriefIntroduction=cngal?.PersonalSignature,
                BackgroundImage=cngal?.BackgroundImage,
            };

            user = await _userRepository.InsertAsync(user);
            return user;
        }

        public async Task<DataModel.ViewModel.Space.UserInforViewModel> GetCnGalUserInfo(string id)
        {
            try
            {
                var user = await _httpService.GetAsync<DataModel.ViewModel.Space.UserInforViewModel>($"{_configuration["CnGalAPI"]}api/space/GetUserData/{id}");
                if (user.PhotoPath.Contains("default"))
                {
                    user.PhotoPath = null;
                }
                if (user.BackgroundImage.Contains("default"))
                {
                    user.BackgroundImage = null;
                }
                if (user.PersonalSignature == "哇，这里什么都没有呢")
                {
                    user.PersonalSignature = null;
                }
                return user;
            }
            catch
            {
                return null;
            }
        }

        public bool CheckCurrentUserRole(string role)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                //无法获取http上下文即为内部调用 默认拥有全部权限
                return true;
            }
            var id = _httpContextAccessor.HttpContext?.User?.Claims?.GetUserId();
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return _httpContextAccessor.HttpContext.User.Claims.Any(s => (s.Type == JwtClaimTypes.Role || s.Type == ClaimTypes.Role) && s.Value == role);
        }
    }
}
