using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.Helper.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace CnGalWebSite.DrawingBed.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _tempPath = "";
        private readonly ILogger<FileService> _logger;

        public FileService(IHttpClientFactory clientFactory, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, ILogger<FileService> logger)
        {
            _clientFactory = clientFactory;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _logger = logger;

            _tempPath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "images");
        }

        public async Task<string> TransferDepositFile(string url,double x=0,double y=0)
        {
            string pathSaveFile = null;
            string pathCutFile = null;
            string pathCompressFile = null;
            try
            {

                pathSaveFile = await SaveFileFromUrl(url);
                pathCutFile = CutLocalFile(pathSaveFile, x, y);
                pathCompressFile = CompressFile(pathCutFile);
                return await UploadLocalFileToServer(pathCompressFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "外部链接转存失败：{url}", url);
                return null;
            }
            finally
            {
                DeleteFile(pathSaveFile);
                DeleteFile(pathCutFile);
                DeleteFile(pathCompressFile);
            }

        }

        public async Task<string> UploadFormFile(IFormFile file, double x = 0, double y = 0)
        {
            string pathSaveFile = null;
            string pathCompressFile = null;
            try
            {

                pathSaveFile = SaveFormFile(file);

                var pathCutFile = CutLocalFile(pathSaveFile, x, y);

                pathCompressFile = CompressFile(pathCutFile);
                return await UploadLocalFileToServer(pathCompressFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传文件失败：{file}" ,file.Name);
                return null;
            }
            finally
            {
                DeleteFile(pathSaveFile);
                DeleteFile(pathCompressFile);
            }

        }


        private async Task<string> UploadLocalFileToServer(string filePath)
        {
            using var content = new MultipartFormDataContent();

            content.Add(
                content: new StringContent(GetFileBase64(filePath)),
                name: "source");

            using var client = _clientFactory.CreateClient();

            var url = _configuration["SliotsImageUrl"] + "api/1/upload/?format=txt&key=" + _configuration["SliotsImageAPIToken"];

            var response = await client.PostAsync(url, content);

            var newUploadResults = await response.Content.ReadAsStringAsync();

            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("上传文件到 {url} 失败：{filePath}", _configuration["SliotsImageUrl"], filePath);
            }

            return response.StatusCode == System.Net.HttpStatusCode.OK && string.IsNullOrWhiteSpace(newUploadResults) == false && newUploadResults.Contains("http")
                ? newUploadResults.Replace("http://local.host/", "https://pic.cngal.top/").Replace("pic.cngal.top", "image.cngal.org").Replace("http://image.cngal.org/", "https://image.cngal.org/")
                : null;
        }

        private static string GetFileSuffixName(string path)
        {
            var temp = path.Split('.');
            var Suffix = temp.Length == 1 ? "png" : temp[^1];
            return Suffix.Length > 4 ? "png" : Suffix;
        }

        private string SaveFormFile(IFormFile file)
        {

            var tempName = Guid.NewGuid().ToString() + "." + GetFileSuffixName(file.FileName);
            //保存图片到本地
            var newPath = Path.Combine(_tempPath, tempName);
            SaveFile(file, newPath);

            _logger.LogInformation("保存客户端传输的图片：{file}" ,file.FileName);

            return newPath;
        }

        public async Task<string> SaveFileFromUrl(string url)
        {

            using var client = _clientFactory.CreateClient();

            var Bytes = await client.GetByteArrayAsync(url);


            using Stream stream = new MemoryStream(Bytes);
            IFormFile image = new FormFile(stream, 0, stream.Length, ".png", "测试.png");

            var tempName = Guid.NewGuid().ToString() + "." + GetFileSuffixName(url);

            //保存图片到本地
            var newPath = Path.Combine(_tempPath, tempName);
            SaveFile(image, newPath);

            _logger.LogInformation( "下载远程链接里的图片：{file}" ,url);

            return newPath;
        }

        public string CutLocalFile(string path, double x = 0, double y = 0)
        {
            return x == 0 && y == 0 ? path : CutImage(path, x, y);
        }

        public string CompressFile(string path)
        {
            return new FileInfo(path).Length > 2 * 1024 * 1024 ? GetPicThumbnail(path, 0.7) : path;
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
            byte[] fileBytes;
            fileBytes = stmMemory.ToArray();//文件流Byte
            using var fs = new FileStream(destinationPath, FileMode.OpenOrCreate);
            stmMemory.WriteTo(fs);
        }

        /// <summary>
        /// 按比例缩小图片
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="proportion"></param>
        private string GetPicThumbnail(string path, double proportion)
        {
            var newPath = Path.Combine(_tempPath, Guid.NewGuid().ToString() + "." + GetFileSuffixName(path));
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
            var newPath = Path.Combine(_tempPath, Guid.NewGuid().ToString() + "." + GetFileSuffixName(path));

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
                        if (!item.ToString().Contains("$") && (!item.ToString().Contains("Boot")))
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
        private static void DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch
            {

            }
        }

    }
}
