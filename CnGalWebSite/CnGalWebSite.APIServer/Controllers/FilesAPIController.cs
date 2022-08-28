using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Files;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/files/[action]")]
    public class FilesAPIController : ControllerBase
    {
        private readonly IRepository<FileManager, int> _fileManagerRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IAppHelper _appHelper;

        public FilesAPIController(IRepository<UserFile, int> userFileRepository, IRepository<FileManager, int> fileManagerRepository, IAppHelper appHelper, UserManager<ApplicationUser> userManager,
        IFileService fileService)
        {
            _appHelper = appHelper;
            _fileManagerRepository = fileManagerRepository;
            _userFileRepository = userFileRepository;
            _fileService = fileService;
            _userManager = userManager;
        }

        /// <summary>
        /// 添加用户已上传的文件信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> AddUserUploadFileInfor(AddUserUploadFileInforModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user == null)
            {
                return new Result { Successful = false, Error = "该用户不存在" };
            }
            //加载文件管理
            var fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (fileManager == null)
            {
                fileManager = new FileManager
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    TotalSize = 500 * 1024 * 1024,
                    UsedSize = 0,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }

            //检测是否已添加相同文件
            if(string.IsNullOrWhiteSpace(model.Sha1)==false&&await _userFileRepository.GetAll().AnyAsync(s=>s.Sha1==model.Sha1))
            {
                return new Result { Successful = true };
            }


            fileManager.UserFiles.Add(new UserFile
            {
                FileName = model.FileName,
                UploadTime = DateTime.Now.ToCstTime(),
                FileSize = model.FileSize,
                Sha1 = model.Sha1,
                Duration = model.Duration,
                Type = model.Type,
                UserId = fileManager.ApplicationUserId
            });
            fileManager.UsedSize += (model.FileSize ?? 0);
            //更新用户文件列表
            await _fileManagerRepository.UpdateAsync(fileManager);

            return new Result { Successful = true };

        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result>> GetSameFileAsync([FromQuery]string sha1)
        {
            if(string.IsNullOrWhiteSpace(sha1))
            {
                return new Result { Successful = false };
            }

            var file = await _userFileRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Sha1 == sha1);
            if(file==null)
            {
                return new Result { Successful = false };
            }

            return new Result { Successful = true,Error=file.FileName };
           
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result>> GetRandomImageAsync()
        {
            var random = new Random();
            var length = await _userFileRepository.CountAsync();
            if (length > 0)
            {
                var p = random.Next(1, length - 1);
                var temp = await _userFileRepository.FirstOrDefaultAsync(s => s.Id == p);
                if (temp != null)
                {
                    return new Result { Successful = true, Error = _appHelper.GetImagePath(temp.FileName, "") };
                }
                else
                {
                    return new Result { Successful = false };
                }
            }
            else
            {
                return new Result { Successful = false };
            }
        }

        /// <summary>
        /// 获取图片列表 用于预览
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<PagedResultDto<ImageInforTipViewModel>> GetImageListAsync(PagedSortedAndFilterInput input)
        {
            return await _fileService.GetImagePaginatedResult(input);
        }

        /// <summary>
        /// 获取音频列表 用于预览
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<PagedResultDto<ListFileAloneModel>> GetAudioListAsync(PagedSortedAndFilterInput input)
        {
            return await _fileService.GetAudioPaginatedResult(input);
        }
    }
}
