using Aliyun.OSS;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.Helper.Helper;
using FFmpeg.NET;
using MediaInfo;
using OneOf.Types;
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
        private readonly HttpClient _httpClient;
        private readonly string _imageTempPath = "";
        private readonly string _audioTempPath = "";
        private readonly string _fileTempPath = "";
        private readonly ILogger<FileService> _logger;

        public FileService(HttpClient httpClient, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, ILogger<FileService> logger)
        {
            _httpClient = httpClient;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _logger = logger;

            _imageTempPath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "images");
            _fileTempPath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "files");
            _audioTempPath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "audio");

            Directory.CreateDirectory(_imageTempPath);
            Directory.CreateDirectory(_fileTempPath);
            Directory.CreateDirectory(_audioTempPath);
        }

        public async Task<UploadResult> TransferDepositFile(string url,double x=0,double y=0, UploadFileType type= UploadFileType.Image)
        {
            string pathSaveFile = null;
            string pathCutFile = null;
            string pathCompressFile = null;
            CutFileResult result = new CutFileResult();
            try
            {

                pathSaveFile = await SaveFileFromUrl(url,type);
                if (type == UploadFileType.Image)
                {
                    pathCutFile = CutLocalImage(pathSaveFile, x, y);
                    pathCompressFile = CompressImage(pathCutFile);
                    result.FileSize = (new FileInfo(pathCompressFile)).Length;
                }
                else
                {
                    result =await CutAudioAsync(pathSaveFile);
                    pathCompressFile = result.FileURL;
                }


                var sha1 = GetSHA1(pathCompressFile);
                var uploadedFile = await CheckSameFileFromServer(sha1);
                if (string.IsNullOrWhiteSpace(uploadedFile))
                {
                    if (url.Contains("image.cngal.org")&&x==0&&y==0)
                    {
                        //原链接已经在图床中，直接返回
                        uploadedFile = url;
                    }
                    else
                    {
                        uploadedFile = await UploadLocalFileToServer(pathCompressFile, type);
                    }

                }

                return new UploadResult
                {
                    Uploaded = true,
                    Sha1 = sha1,
                    Url = uploadedFile,
                    FileSize = result.FileSize,
                    OriginalUrl=url,
                    Duration = result.Duration
                };
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

        public async Task<UploadResult> UploadFormFile(IFormFile file, double x = 0, double y = 0, UploadFileType type = UploadFileType.Image)
        {
            string pathSaveFile = null;
            string pathCompressFile = null;
            string pathCutFile = null;
            CutFileResult result = new CutFileResult();
            try
            {

                pathSaveFile = SaveFormFile(file, type);

                if (type == UploadFileType.Image)
                {
                    pathCutFile = CutLocalImage(pathSaveFile, x, y);
                    pathCompressFile = CompressImage(pathCutFile);
                }
                else
                {
                    result =await CutAudioAsync(pathSaveFile);
                    pathCompressFile = result.FileURL;
                }

                var sha1 = GetSHA1(pathCompressFile);
                var uploadedFile = await CheckSameFileFromServer(sha1);
                if (string.IsNullOrWhiteSpace(uploadedFile))
                {
                    uploadedFile = await UploadLocalFileToServer(pathCompressFile, type);
                }

                return new UploadResult
                {
                    Uploaded = true,
                    Sha1 = sha1,
                    FileName = file.FileName,
                    Url = uploadedFile,
                    FileSize = (new FileInfo(pathCompressFile)).Length,
                    Duration = result.Duration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传文件失败：{file}" ,file.Name);
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
        private async Task<string> UploadLocalFileToServer(string filePath, UploadFileType type)
        {
            return type switch
            {
                UploadFileType.Audio => UploadToAudioServer(filePath),
                UploadFileType.Image => await UploadToImageServer(filePath),
                _ => null

            };
        }

        private string UploadToAudioServer(string filePath)
        {
            // yourEndpoint填写Bucket所在地域对应的Endpoint。以华东1（杭州）为例，Endpoint填写为https://oss-cn-hangzhou.aliyuncs.com
            var endpoint = _configuration["OSSEndpoint"];
            // 阿里云账号AccessKey拥有所有API的访问权限，风险很高。强烈建议您创建并使用RAM用户进行API访问或日常运维，请登录RAM控制台创建RAM用户
            var accessKeyId = _configuration["OSSAccessKeyId"];
            var accessKeySecret = _configuration["OSSAccessKeySecret"];
            // yourBucketName填写Bucket名称
            var bucketName = _configuration["OSSBucketName"];

            // 创建OSSClient实例
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);

            // 填写Object完整路径，完整路径中不能包含Bucket名称，例如exampledir/exampleobject.txt
            var objectName = $"audio/upload/{DateTime.Now.ToCstTime():yyyyMMdd}/{DateTime.Now.ToCstTime().ToBinary()}.mp3";
            try
            {
                // 上传文件
                var result = client.PutObject(bucketName, objectName, filePath);
                var url = _configuration["AudioUrl"] + objectName;
                _logger.LogInformation("成功上传音频到OSS：{url}", url);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传音频到OSS失败：{filePath}", filePath);
                throw;
            }

        }

        private async Task<string> UploadToImageServer(string filePath)
        {
            using var content = new MultipartFormDataContent();

            content.Add(
                content: new StringContent(GetFileBase64(filePath)),
                name: "source");

            var url = _configuration["SliotsImageUrl"] + "api/1/upload/?format=txt&key=" + _configuration["SliotsImageAPIToken"];

            var response = await _httpClient.PostAsync(url, content);

            var newUploadResults = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrWhiteSpace(newUploadResults) || newUploadResults.Contains("http") == false)
            {
                _logger.LogError("上传文件到图床失败：{filePath}", filePath);
                throw new Exception("图床内部传输错误");
            }


            var result = newUploadResults.Replace("local.host", "image.cngal.org").Replace("http://", "https://");
            _logger.LogInformation("成功上传图片到图床：{url}", result);
            return result;

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

        private string SaveFormFile(IFormFile file, UploadFileType type)
        {

            var tempName = Guid.NewGuid().ToString() + "." + GetFileSuffixName(file.FileName, type);
            //保存图片到本地
            var newPath = Path.Combine(type switch { UploadFileType.Image => _imageTempPath, UploadFileType.Audio => _audioTempPath, _ => _fileTempPath }, tempName);
            SaveFile(file, newPath);

            _logger.LogInformation("保存客户端传输的文件：{file}" ,file.FileName);

            return newPath;
        }

        public async Task<string> SaveFileFromUrl(string url, UploadFileType type)
        {
            var Bytes = await _httpClient.GetByteArrayAsync(url);


            using Stream stream = new MemoryStream(Bytes);
            IFormFile image = new FormFile(stream, 0, stream.Length, ".png", "测试.png");

            var tempName = Guid.NewGuid().ToString() + "." + GetFileSuffixName(url,type);

            //保存图片到本地
            var newPath = Path.Combine(type switch { UploadFileType.Image => _imageTempPath, UploadFileType.Audio => _audioTempPath, _ => _fileTempPath }, tempName);
            SaveFile(image, newPath);

            _logger.LogInformation( "下载远程链接里的图片：{file}" ,url);

            return newPath;
        }


        private static string GetFileBase64(string path)
        {
            using var fsForRead = new FileStream(path, FileMode.Open);
            string base64Str;
            try
            {
                //读入一个字节
                //读写指针移到距开头10个字节处
                _ = fsForRead.Seek(0, SeekOrigin.Begin);
                var bs = new byte[fsForRead.Length];
                var log = Convert.ToInt32(fsForRead.Length);
                //从文件中读取10个字节放到数组bs中
                _ = fsForRead.Read(bs, 0, log);
                base64Str = Convert.ToBase64String(bs);

                return base64Str;
            }
            catch
            {
            }
            finally
            {
                fsForRead.Close();
            }
            return "";
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

        /// <summary>
        /// 检测是否存在相同的文件 存在则返回
        /// </summary>
        /// <returns></returns>
        private async Task<string> CheckSameFileFromServer(string sha1)
        {
            var result = await _httpClient.GetFromJsonAsync<Result>(ToolHelper.WebApiPath + "api/files/GetSameFile?sha1=" + sha1);
            if(result.Successful&&string.IsNullOrWhiteSpace(result.Error)==false)
            {
                return result.Error;
            }
            else
            {
                return null;
            }
        }

        private static void SaveFile(IFormFile sourceFile, string destinationPath)
        {
            using var stmMemory = new MemoryStream();
            using var stream = sourceFile.OpenReadStream();
            var buffer = new byte[stream.Length];
            int i;
            //将字节逐个放入到Byte中
            while ((i = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                stmMemory.Write(buffer, 0, i);
            }

            using var fs = new FileStream(destinationPath, FileMode.OpenOrCreate);
            stmMemory.WriteTo(fs);
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
            var inputFile = new InputFile(path);
            var outputFile = new OutputFile(Path.Combine(_audioTempPath, Guid.NewGuid().ToString() + ".mp3"));

            var ffmpeg = new Engine(_configuration["FFmpegPath"]);

            var options = new ConversionOptions();
            options.AudioBitRate = 127000;
            options.ExtraArguments = " -codec:a libmp3lame -qscale:a 5 ";

            var output = await ffmpeg.ConvertAsync(inputFile, outputFile, options, default);

            var mediaFile = new MediaInfoWrapper(outputFile.FileInfo.FullName,_logger);

            _logger.LogInformation("成功裁剪音频：{url}", output.FileInfo.FullName);

            return new CutFileResult
            {
                FileURL = output.FileInfo.FullName,
                Duration = new TimeSpan(0,0,0,0,mediaFile.Duration)
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
            var newPath = Path.Combine(_imageTempPath, Guid.NewGuid().ToString() + "." + GetFileSuffixName(path,  UploadFileType.Image));
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
                     .Crop(new Rectangle(0, 0, linshi_x, linshi_y)));
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
        private void DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "删除文件失败：{url}", path);
            }
        }

        #endregion


    }
}
