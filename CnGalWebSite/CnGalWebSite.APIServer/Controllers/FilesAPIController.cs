using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
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

        private async Task<ActionResult<List<UploadResult>>> PostFileMain(IEnumerable<IFormFile> files, double x = 0, double y = 0, int server = 0)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断剩余空间
            long total = 0;
            foreach (var item in files)
            {
                total += item.Length;
            }
            if (total > 500 * 1024 * 1024 && await _userManager.IsInRoleAsync(user, "Editor") == false)
            {
                return new List<UploadResult> { new UploadResult { Uploaded = false, Error = "该用户总文件空间不足" } };
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
                    UsedSize = total,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }
            else
            {
                if (fileManager.UsedSize + total > fileManager.TotalSize && await _userManager.IsInRoleAsync(user, "Editor") == false)
                {
                    return new List<UploadResult> { new UploadResult { Uploaded = false, Error = "该用户总文件空间不足" } };
                }
            }

            var maxAllowedFiles = 10;
            long maxFileSize = 1024 * 1024 * 5;
            var filesProcessed = 0;
            var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");

            var results = new List<UploadResult>();
            foreach (var item in files)
            {
                var uploadResult = new UploadResult();
                if (filesProcessed < maxAllowedFiles)
                {
                    if (item.Length == 0)
                    {
                        //文件大小为0
                        uploadResult.Uploaded = false;
                        uploadResult.Error = "文件大小为0";
                    }
                    else if (item.Length > maxFileSize && server != 1)
                    {
                        //文件大小超过上限
                        uploadResult.Uploaded = false;
                        uploadResult.Error = "文件大小超过上限";
                    }
                    else
                    {
                        try
                        {
                            uploadResult.FileName = await _appHelper.UploadImageNewAsync(fileManager, item, x, y, server);
                            uploadResult.FileURL = _appHelper.GetImagePath(uploadResult.FileName, "");
                            uploadResult.Uploaded = true;
                        }
                        catch (IOException ex)
                        {
                            //文件没有被上传
                            uploadResult.Error = "文件没有被上传，异常详细信息：" + ex.Message;
                        }
                    }

                    filesProcessed++;
                }
                else
                {
                    uploadResult.Uploaded = false;
                    uploadResult.Error = "超过单次文件上传上限";
                }
                results.Add(uploadResult);
            }


            return results;
        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files);
        }
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFileB([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 0, 0, 1);
        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile16_9([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 460, 215);
        }
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile9_16([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 9, 16);
        }
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile1_1([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 1, 1);
        }
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile4_1A2([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 4, 1.2);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> AddUserLoadedFileInfor(ImageInforTipViewModel model)
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
            fileManager.UserFiles.Add(new UserFile
            {
                FileName = model.FileName,
                UploadTime = DateTime.Now.ToCstTime(),
                FileSize = model.FileSize,
                Sha1 = model.Sha1,
                UserId = fileManager.ApplicationUserId
            });
            fileManager.UsedSize += (long)model.FileSize;
            //更新用户文件列表
            await _fileManagerRepository.UpdateAsync(fileManager);

            return new Result { Successful = true };

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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<PagedResultDto<ImageInforTipViewModel>> GetFileListAsync(PagedSortedAndFilterInput input)
        {
            return await _fileService.GetPaginatedResult(input);
        }


        [HttpPost]
        public async Task<ActionResult<UploadResult>> PostFileUrlBefore(ImageInforTipViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断剩余空间
            long total = 0;

            if (model.FileSize > 50 * 1024 * 1024)
            {
                return new UploadResult { Uploaded = false, Error = "文件上限为 50 MB" };
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
                    UsedSize = total,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }
            else
            {
                if (fileManager.UsedSize + total > fileManager.TotalSize)
                {
                    return new UploadResult { Uploaded = false, Error = "该用户总文件空间不足" };
                }
            }
            //检查是否已经上传过该文件
            if (string.IsNullOrWhiteSpace(model.Sha1) == false)
            {
                //检查数据库中是否有相同的文件
                var userFile = await _userFileRepository.FirstOrDefaultAsync(s => s.Sha1 == model.Sha1);
                if (userFile != null)
                {
                    return new UploadResult { Uploaded = true, FileName = userFile.FileName, FileURL = userFile.FileName };
                }
            }
            return new UploadResult { Uploaded = true };
        }

        [HttpPost]
        public async Task<ActionResult<UploadResult>> PostFileUrlAfter(ImageInforTipViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断剩余空间
            long total = 0;

            if (model.FileSize > 50 * 1024 * 1024)
            {
                return new UploadResult { Uploaded = false, Error = "文件上限为 50 MB" };
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
                    UsedSize = total,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }
            else
            {
                if (fileManager.UsedSize + total > fileManager.TotalSize)
                {
                    return new UploadResult { Uploaded = false, Error = "该用户总文件空间不足" };
                }
            }

            //检查通过后添加文件
            fileManager.UserFiles.Add(new UserFile
            {
                FileName = model.FileName,
                UploadTime = DateTime.Now.ToCstTime(),
                FileSize = model.FileSize,
                Sha1 = model.Sha1,
                UserId = fileManager.ApplicationUserId
            });
            fileManager.UsedSize += (long)model.FileSize;
            //更新用户文件列表
            await _fileManagerRepository.UpdateAsync(fileManager);

            return new UploadResult { Uploaded = true };

        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFileBase64(ImageBase64UploadModel model)
        {
            //解码
            var temp = Convert.FromBase64String(model.Base64Str);
            using Stream stream = new MemoryStream(temp);
            IFormFile fromFile = new FormFile(stream, 0, stream.Length, "测试.png", "测试.png");
            //上传

            return await PostFileMain(new List<IFormFile> { fromFile }, model.Width, model.Height);
        }

    }
}
