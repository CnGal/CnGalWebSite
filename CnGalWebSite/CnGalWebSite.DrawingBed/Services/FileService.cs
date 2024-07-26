using Aliyun.OSS;
using CnGalWebSite.DrawingBed.Models.DataModels;
using CnGalWebSite.DrawingBed.Models.ViewModels;
using COSXML;
using COSXML.Auth;
using FFmpeg.NET;
using MediaInfo;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Policy;
using System.Text;
using Tweetinvi.Security;

namespace CnGalWebSite.DrawingBed.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _imageTempPath = "";
        private readonly string _audioTempPath = "";
        private readonly string _fileTempPath = "";
        private readonly ILogger<FileService> _logger;
        private readonly IUploadService _uploadService;
        private readonly IRecordService _recordService;

        public FileService(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, ILogger<FileService> logger, IUploadService uploadService, IRecordService recordService)
        {
            _httpClientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _logger = logger;
            _uploadService = uploadService;
            _recordService = recordService;

            _imageTempPath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "images");
            _fileTempPath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "files");
            _audioTempPath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "audio");

            Directory.CreateDirectory(_imageTempPath);
            Directory.CreateDirectory(_fileTempPath);
            Directory.CreateDirectory(_audioTempPath);
        }

        public async Task<UploadResult> TransferDepositFile(string url, bool gallery, double x = 0, double y = 0, UploadFileType type = UploadFileType.Image)
        {
            string pathSaveFile = null;
            string pathCutFile = null;
            string pathCompressFile = null;
            CutFileResult result = new CutFileResult();
            try
            {

                pathSaveFile = await SaveFileFromUrl(url, type);
                if (type == UploadFileType.Image)
                {
                    pathCutFile = CutLocalImage(pathSaveFile, x, y);
                    pathCompressFile = CompressImage(pathCutFile);
                    result.FileSize = (new FileInfo(pathCompressFile)).Length;
                }
                else
                {
                    result = await CutAudioAsync(pathSaveFile);
                    pathCompressFile = result.FileURL;
                }


                var sha1 = GetSHA1(pathCompressFile);

                var model = new UploadResult
                {
                    Uploaded = true,
                    Sha1 = sha1,
                    Url = await _recordService.Get(sha1),
                    FileSize = result.FileSize,
                    OriginalUrl = url,
                    Duration = result.Duration,
                    Type = type,
                };
                if (string.IsNullOrWhiteSpace(model.Url))
                {
                    if (url.Contains("image.cngal.org") && x == 0 && y == 0)
                    {
                        //原链接已经在图床中，直接返回
                        model.Url = url;
                    }
                    else
                    {
                        model.Url = await UploadLocalFileToServer(pathCompressFile, sha1, type, gallery);
                        //添加文件上传记录
                        await _recordService.Add(model);
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "外部链接转存失败：{url}", url);
                throw;
            }
            finally
            {
                DeleteFile(pathSaveFile);
                DeleteFile(pathCutFile);
                DeleteFile(pathCompressFile);
            }
        }

        public async Task<UploadResult> UploadFormFile(IFormFile file, bool gallery, double x = 0, double y = 0, UploadFileType type = UploadFileType.Image)
        {
            string pathSaveFile = null;
            string pathCompressFile = null;
            string pathCutFile = null;
            CutFileResult result = new CutFileResult();
            try
            {

                pathSaveFile = await SaveFormFile(file, type);

                if (type == UploadFileType.Image)
                {
                    pathCutFile = CutLocalImage(pathSaveFile, x, y);
                    pathCompressFile = CompressImage(pathCutFile);
                }
                else
                {
                    result = await CutAudioAsync(pathSaveFile);
                    pathCompressFile = result.FileURL;
                }

                var sha1 = GetSHA1(pathCompressFile);

                var model = new UploadResult
                {
                    Uploaded = true,
                    Sha1 = sha1,
                    FileName = file.FileName,
                    Url = await _recordService.Get(sha1),
                    FileSize = (new FileInfo(pathCompressFile)).Length,
                    Duration = result.Duration,
                    Type = type,
                };

                if (string.IsNullOrWhiteSpace(model.Url))
                {
                    model.Url = await UploadLocalFileToServer(pathCompressFile, sha1, type, gallery);
                    //添加文件上传记录
                    await _recordService.Add(model);
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传文件失败：{file}", file.Name);
                throw;
            }
            finally
            {
                DeleteFile(pathSaveFile);
                DeleteFile(pathCompressFile);
                DeleteFile(pathCutFile);
            }

        }


        #region 上传文件
        private async Task<string> UploadLocalFileToServer(string filePath, string shar1, UploadFileType type, bool gallery)
        {
            var url = type switch
            {
                UploadFileType.Audio => await _uploadService.UploadToAliyunOSS(filePath, shar1),
                UploadFileType.Image => await _uploadService.UploadToTencentOSS(filePath, shar1),
                _ => null
            };

            if (gallery && type == UploadFileType.Image)
            {
                url = await _uploadService.UploadToTucangCC(filePath) + "?" + url;
            }

            return url;
        }
        #endregion



        #region 处理文件
        private static string GetFileSuffixName(string path, UploadFileType type)
        {
            var temp = path.Split('?').First().Split('.');
            var defaultName = type switch { UploadFileType.Audio => "mp3", UploadFileType.Image => "png", _ => "png" };
            var Suffix = temp.Length == 1 ? defaultName : temp[^1];
            return Suffix.Length > 4 ? defaultName : Suffix;
        }

        private async Task<string> SaveFormFile(IFormFile file, UploadFileType type)
        {

            var tempName = new Random().Next() + "." + GetFileSuffixName(file.FileName, type);
            //保存图片到本地
            var newPath = Path.Combine(type switch { UploadFileType.Image => _imageTempPath, UploadFileType.Audio => _audioTempPath, _ => _fileTempPath }, tempName);
            using var steam = file.OpenReadStream();
            await SaveFile(steam, newPath);

            _logger.LogInformation("保存客户端传输的文件：{file}", file.FileName);

            return newPath;
        }

        public async Task<string> SaveFileFromUrl(string url, UploadFileType type)
        {
            if (url.Contains("http") == false)
            {
                url = "https:" + url;
            }

            var response = await _httpClientFactory.CreateClient().GetAsync(url);
            using var stream = await response.Content.ReadAsStreamAsync();
            var tempName = new Random().Next() + "." + GetFileSuffixName(url, type);

            //保存图片到本地
            var newPath = Path.Combine(type switch { UploadFileType.Image => _imageTempPath, UploadFileType.Audio => _audioTempPath, _ => _fileTempPath }, tempName);
            await SaveFile(stream, newPath);

            _logger.LogInformation("下载远程链接里的文件：{file}", url);

            return newPath;
        }

        private string GetSHA1(string path)
        {
            try
            {
                FileStream file = new FileStream(path, FileMode.Open);
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] retval = sha1.ComputeHash(file);
                file.Close();

                StringBuilder sc = new StringBuilder();
                for (int i = 0; i < retval.Length; i++)
                {
                    sc.Append(retval[i].ToString("x2"));
                }
                return sc.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算文件Sha1失败");
                throw;
            }
        }

        private static async Task SaveFile(Stream file, string destinationPath)
        {
            using var fs = File.Create(destinationPath);
            await file.CopyToAsync(fs);
            file.Close();
            fs.Close();
        }

        #endregion

        #region 处理音频

        /// <summary>
        /// 裁剪音频并转换格式
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private async Task<CutFileResult> CutAudioAsync(string path)
        {
            if (File.Exists(path) == false)
            {
                throw new Exception($"裁剪音频失败，文件不存在：{path}");
            }

            var inputFile = new InputFile(path);
            var outputFile = new OutputFile(Path.Combine(_audioTempPath, Guid.NewGuid().ToString() + ".mp3"));

            var ffmpeg = new Engine(_configuration["FFmpegPath"]);

            var options = new ConversionOptions();
            options.AudioBitRate = 127000;
            options.ExtraArguments = " -codec:a libmp3lame -qscale:a 5 ";

            var output = await ffmpeg.ConvertAsync(inputFile, outputFile, options, default);

            var mediaFile = new MediaInfoWrapper(outputFile.FileInfo.FullName, _logger);

            _logger.LogInformation("成功裁剪音频：{url}", output.FileInfo.FullName);

            return new CutFileResult
            {
                FileURL = output.FileInfo.FullName,
                Duration = new TimeSpan(0, 0, 0, 0, mediaFile.Duration)
            };

        }

        #endregion

        #region 处理图片
        public string CutLocalImage(string path, double x = 0, double y = 0)
        {
            return x == 0 && y == 0 ? path : CutImage(path, x, y);
        }

        public string CompressImage(string path)
        {
            //不再裁剪图片
            return new FileInfo(path).Length > 1024 * 1024 * 1024 ? GetPicThumbnail(path, 0.7) : path;
        }

        /// <summary>
        /// 按比例缩小图片
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="proportion"></param>
        private string GetPicThumbnail(string path, double proportion)
        {
            var newPath = Path.Combine(_imageTempPath, Guid.NewGuid().ToString() + "." + GetFileSuffixName(path, UploadFileType.Image));
            using (var image = Image.Load(path))
            {
                image.Mutate(x => x
                     .Resize((int)(image.Width * proportion), (int)(image.Height * proportion)));
                image.Save(newPath);
                return newPath;
            };
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private string CutImage(string path, double x = 0, double y = 0)
        {
            var newPath = Path.Combine(_imageTempPath, Guid.NewGuid().ToString() + "." + GetFileSuffixName(path, UploadFileType.Image));

            using (var image = Image.Load(path))
            {
                int linshi_x, linshi_y;

                linshi_x = image.Width;
                linshi_y = (int)(linshi_x / x * y);
                if (image.Height < linshi_y)
                {
                    linshi_y = image.Height;
                    linshi_x = (int)(linshi_y / y * x);
                }


                image.Mutate(x => x
                     //.Resize(linshi_x, linshi_y)
                     .Crop(new Rectangle((image.Width - linshi_x) / 2, (image.Height - linshi_y) / 2, linshi_x, linshi_y)));
                image.Save(newPath);
                return newPath;

            };
        }
        #endregion

        #region 删除文件
        private bool DeleteFiles(string path)
        {
            if (Directory.Exists(path) == false)
            {
                _logger.LogError("要删除的路径不存在");
                return false;
            }
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles();
            try
            {
                foreach (var item in files)
                {
                    System.IO.File.Delete(item.FullName);
                }
                if (dir.GetDirectories().Length != 0)
                {
                    foreach (var item in dir.GetDirectories())
                    {
                        if (!item.ToString().Contains('$') && (!item.ToString().Contains("Boot")))
                        {
                            // Console.WriteLine(item);

                            _ = DeleteFiles(dir.ToString() + "\\" + item.ToString());
                        }
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除文件夹失败");
                return false;

            }



        }
        public void DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除文件失败：{url}", path);
            }
        }

        #endregion


    }
}
