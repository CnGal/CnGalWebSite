using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace CnGalWebSite.DrawingBed.Controllers
{
    [ApiController]
    [Route("api/files/[action]")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public FileController(IHttpClientFactory clientFactory, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> TransferDepositFile(TransferDepositFileModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient();
                var x = model.X;
                var y = model.Y;

                //client.Timeout = TimeSpan.FromSeconds(10);

                var Bytes = await client.GetByteArrayAsync(model.Url);


                using Stream stream = new MemoryStream(Bytes);
                IFormFile image = new FormFile(stream, 0, stream.Length, "测试.png", "测试.png");
                //获取保存路径
                string uploadsFolder;
                string uniqueFileName;
                string filePath;
                string tempFilePath;

                uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "images");

                //清空文件夹
                DeleteFiles(uploadsFolder);
                DeleteFiles(Path.Combine(_webHostEnvironment.WebRootPath, "temp", "imageprogress"));


                var temp = image.FileName.Split('.');
                var Suffix = "";
                if (temp.Length == 1)
                {
                    Suffix = "png";
                }
                else
                {
                    Suffix = temp[^1];
                }
                uniqueFileName = Guid.NewGuid().ToString() + "." + Suffix;// + "_" + image.FileName;


                filePath = Path.Combine(uploadsFolder, uniqueFileName);
                tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "imageprogress", uniqueFileName);

                //保存图片到本地
                SaveFile(image, filePath);

                //裁剪图片
                if (x != 0 && y != 0)
                {
                    CutImage(filePath, tempFilePath, x, y);
                }
                else
                {
                    System.IO.File.Copy(filePath, tempFilePath);
                }

                System.IO.File.Delete(filePath);

                //再压缩图片
                if ((new FileInfo(tempFilePath)).Length > 500 * 1024)
                {
                    GetPicThumbnail(tempFilePath, filePath, 0.7);
                }
                else
                {
                    System.IO.File.Copy(tempFilePath, filePath);
                }

                //上传文件到远程服务器
                var UploadResults = "";

                UploadResults = await UploadFileToBserver(filePath);

                if (string.IsNullOrWhiteSpace(UploadResults))
                {
                    return new Result { Successful = false, Error = "文件上传失败" };
                }
                else
                {
                    return new Result { Successful = true, Error = UploadResults };
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("> " + DateTime.Now.ToString("G"));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;
                return new Result { Successful = false, Error = ex.Message };
            }
        }


        private async Task<string> UploadFileToBserver(string filePath)
        {
            using var content = new MultipartFormDataContent();

            content.Add(
                content: new StringContent(GetFileBase64(filePath)),
                name: "source");

            using var client = _clientFactory.CreateClient();

            var url = _configuration["SliotsImageUrl"] + "api/1/upload/?format=txt&key=" + _configuration["SliotsImageAPIToken"];

            var response = await client.PostAsync(url, content);

            var newUploadResults = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK && string.IsNullOrWhiteSpace(newUploadResults) == false && newUploadResults != "Duplicated upload")
            {

                return newUploadResults;
            }
            else
            {
                return "";
            }
        }
        private static string GetFileBase64(string path)
        {
            using var fsForRead = new FileStream(path, FileMode.Open);
            string base64Str;
            try
            {
                //读入一个字节
                //读写指针移到距开头10个字节处
                fsForRead.Seek(0, SeekOrigin.Begin);
                var bs = new byte[fsForRead.Length];
                var log = Convert.ToInt32(fsForRead.Length);
                //从文件中读取10个字节放到数组bs中
                fsForRead.Read(bs, 0, log);
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
        private static void GetPicThumbnail(string oldPath, string newPath, double proportion)
        {
            using (var image = Image.Load(oldPath))
            {
                image.Mutate(x => x
                     .Resize((int)(image.Width * proportion), (int)(image.Height * proportion)));
                image.Save(newPath);
            };
        }
        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private static void CutImage(string oldPath, string newPath, double x = 0, double y = 0)
        {
            using (var image = Image.Load(oldPath))
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

            };
        }
        private static bool DeleteFiles(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Console.WriteLine("Path is not Existed!");
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

                            DeleteFiles(dir.ToString() + "\\" + item.ToString());
                        }
                    }
                }


                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Delete Failed!");
                return false;

            }



        }
    }
}
