using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk;
using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Helper
{
    public class AppHelper : IAppHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<TokenCustom, int> _tokenCustomRepository;
        private readonly IRepository<FileManager, int> _fileManagerRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<HistoryUser, int> _historyUserRepository;
        private readonly IRepository<ErrorCount, long> _errorCountRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<BackUpArchive, long> _backUpArchiveRepository;
        private readonly IRepository<BackUpArchiveDetail, long> _backUpArchiveDetailRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IRepository<SendCount, long> _sendCountRepository;
        private readonly IRepository<Loginkey, long> _loginkeyRepository;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IEmailService _EmailService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public AppHelper(IRepository<TimedTask, int> timedTaskRepository, IRepository<BackUpArchive, long> backUpArchiveRepository, IRepository<SignInDay, long> signInDayRepository, IRepository<Periphery, long> peripheryRepository,
        IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteFolder, long> favoriteFolderRepository, IRepository<Vote, long> voteRepository,
        IRepository<ErrorCount, long> errorCountRepository, IRepository<PlayedGame, long> playedGameRepository, IRepository<HistoryUser, int> historyUserRepository, IRepository<Message, long> messageRepository, IRepository<ApplicationUser, string> userRepository,
            IRepository<Comment, long> commentRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, IRepository<SteamInfor, int> steamInforRepository, IRepository<Loginkey, long> loginkeyRepository,
        IHttpClientFactory clientFactory, IRepository<FileManager, int> fileManagerRepository, IEmailService EmailService, IRepository<TokenCustom, int> tokenCustomRepository,
            IRepository<Article, long> aricleRepository, SignInManager<ApplicationUser> signInManager, IRepository<Entry, int> entryRepository, IRepository<SendCount, long> sendCountRepository,
        IWebHostEnvironment webHostEnvironment, IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IRepository<Lottery, long> lotteryRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _tagRepository = tagRepository;
            _articleRepository = aricleRepository;
            _EmailService = EmailService;
            _webHostEnvironment = webHostEnvironment;
            _fileManagerRepository = fileManagerRepository;
            _tokenCustomRepository = tokenCustomRepository;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _userRepository = userRepository;
            _commentRepository = commentRepository;
            _examineRepository = examineRepository;
            _messageRepository = messageRepository;
            _historyUserRepository = historyUserRepository;
            _errorCountRepository = errorCountRepository;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
            _backUpArchiveRepository = backUpArchiveRepository;
            _userFileRepository = userFileRepository;
            _signInDayRepository = signInDayRepository;
            _favoriteFolderRepository = favoriteFolderRepository;
            _sendCountRepository = sendCountRepository;
            _loginkeyRepository = loginkeyRepository;
            _peripheryRepository = peripheryRepository;
            _voteRepository = voteRepository;
            _lotteryRepository = lotteryRepository;
        }

        public async Task<bool> IsEntryLockedAsync(int entryId, string userId, Operation operation)
        {
            if (await _examineRepository.FirstOrDefaultAsync(s => s.EntryId == entryId && s.ApplicationUserId != userId && s.IsPassed == null && s.Operation == operation) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetImagePath(string image, string defaultStr, bool mediumImage = false)
        {

            if (string.IsNullOrWhiteSpace(image) == true)
            {
                if (string.IsNullOrWhiteSpace(defaultStr) == true)
                {
                    return "";
                }
                else
                {
                    image = "default/" + defaultStr;
                    return "https://app.cngal.org/_content/CnGalWebSite.Shared/images/" + image;
                }

            }
            else
            {
                //判断是否为绝对路径
                if (image.Contains("http://") || image.Contains("https://") || image.Contains("//"))
                {
                    //if(image.Contains("pic.cngal.top"))
                    //{
                    //    return image.Replace(".png", ".md.png").Replace(".jpg", ".md.jpg").Replace(".jpeg", ".md.jpeg").Replace(".gif", ".md.gif");
                    //}

                    if (image.Contains("http://pic.cngal.top"))
                    {
                        return image.Replace("http://pic.cngal.top", "https://pic.cngal.top");
                    }

                    return image;
                }
                else
                {
                    //http://localhost:51313/
                    //121.43.54.210
                    return "https://app.cngal.org/_content/CnGalWebSite.Shared/images/" + image;
                }
            }
        }

        public string GetStringAbbreviation(string str, int length)
        {
            if (str == null)
            {
                return "";
            }
            if (str.Length < length * 2.5)
            {
                return str;
            }
            else
            {
                return string.Concat(str.AsSpan(0, (int)(length * 2.5)), "...");
            }
        }

        public async Task AddUserContributionValueAsync(string userId, long otherId, Operation operation)
        {
            //更新用户积分
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await UpdateUserIntegral(user);
            }
        }

        public async Task<bool> IsArticleLockedAsync(long articleId, string userId, Operation operation)
        {
            if (await _examineRepository.FirstOrDefaultAsync(s => s.ArticleId == articleId && s.ApplicationUserId != userId && s.IsPassed == null && s.Operation == operation) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> CanUserEditArticleAsync(ApplicationUser user, long articleId)
        {
            //读取当前文章的审核信息 获取最后编辑记录和作者
            //判断当前用户是否有权限编辑
            if (await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                return true;
            }
            var sum = await _examineRepository.GetAll().CountAsync(s => s.ArticleId == articleId && s.IsPassed != false && s.ApplicationUserId == user.Id);
            if (sum > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<int> GetShortTokenAsync(string longToken, string userName)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();

            if (longToken == null)
            {
                //按用户名查找验证码
                var tokenCustom = await _tokenCustomRepository.GetAll().OrderBy("Time desc").FirstOrDefaultAsync(s => s.UserName == userName && s.Time != null && ((DateTime)s.Time).AddHours(1) > tempDateTimeNow);
                if (tokenCustom != null)
                {
                    return tokenCustom.Num.Value;
                }
                else
                {
                    return 0;
                }
            }

            var result = 0;
            var random = new Random();
            for (var i = 0; i < 100; i++)
            {
                result = random.Next(100000, 999999);
                var tokenCustom = await _tokenCustomRepository.FirstOrDefaultAsync(s => s.Num != null && s.Num == result);
                if (tokenCustom == null)
                {
                    await _tokenCustomRepository.InsertAsync(new TokenCustom
                    {
                        Num = result,
                        Token = longToken,
                        Time = tempDateTimeNow,
                        UserName = userName
                    });
                    break;
                }
                else
                {
                    if (tokenCustom.Time != null && ((DateTime)tokenCustom.Time).AddHours(1) < tempDateTimeNow)
                    {
                        tokenCustom.Token = longToken;
                        tokenCustom.Num = result;
                        tokenCustom.Time = tempDateTimeNow;
                        tokenCustom.UserName = userName;
                        await _tokenCustomRepository.UpdateAsync(tokenCustom);
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<string> GetLongTokenAsync(string shortToken)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            var token = await _tokenCustomRepository.FirstOrDefaultAsync(s => (s.Num != null && s.Num.ToString() == shortToken) && (s.Time != null && ((DateTime)s.Time).AddHours(1) > tempDateTimeNow));
            if (token != null)
            {
                var temp = token.Token;
                // await _tokenCustomRepository.DeleteAsync(token);
                return temp;
            }
            return null;
        }

        public async Task<string> SendVerificationEmailAsync(string longToken, string email, string userName)
        {
            //检查是否超过上限
            if (await IsExceedMaxSendCount(email, 10, 10))
            {
                return "验证码超过发送次数上限";
            }
            try
            {
                var num = await GetShortTokenAsync(longToken, userName);
                if (num == 0)
                {
                    return null;
                }
                _EmailService.Send(email, "验证你的CnGal资料站账号", "<div	id='contentDiv'	onmouseover='getTop().stopPropagation(event);'	onclick='getTop().preSwapLink(event, 'spam', 'ZC2901-OBne295lonzXhDxipYEzca5');'	style='		position: relative; font-size: 14px; height: auto; padding: 15px 15px 10px 15px; z-index: 1; zoom: 1; line-height: 1.7;	'	class='body'>	<div id='qm_con_body'>		<div			id='mailContentContainer'			class='qmbox qm_con_body_content qqmail_webmail_only'		>			<style>				.qmbox .btn:before,				.qmbox .msg-heart:after,				.qmbox .msg-star:after,				.qmbox .msg-plus:after,				.qmbox .msg-file:before {					speak: none;					font-size: 25px;					font-style: normal;					font-variant: normal;					text-transform: none;					line-height: 1;					position: relative;					-webkit-font-smoothing: antialiased;				}				.qmbox .btn {					text-decoration: none;					border: 0;					font-family: inherit;					font-size: inherit;					color: inherit;					background: 0;					cursor: pointer;					padding: 15px 15px;					display: inline-block;					text-transform: uppercase;					letter-spacing: 1px;					font-weight: 700;					outline: 0;					position: relative;					-webkit-transition: all 0.3s;					-moz-transition: all 0.3s;					transition: all 0.3s;					background: #393;					color: #fff;				}				.qmbox .btn:after {					background: #393;					content: '';					position: absolute;					z-index: -1;					-webkit-transition: all 0.3s;					-moz-transition: all 0.3s;					transition: all 0.3s;				}			</style>			<div				style='					background-position: bottom; background-color: #ececec;					padding: 35px;				'			>				<table					cellpadding='0'					align='center'					style='						width: 600px;						margin: 0px auto;						text-align: left;						position: relative;						border-top-left-radius: 5px;						border-top-right-radius: 5px;						border-bottom-right-radius: 5px;						border-bottom-left-radius: 5px;						font-size: 14px;						font-family: 微软雅黑, 黑体;						line-height: 1.5;						box-shadow: rgb(153, 153, 153) 0px 0px 5px;						border-collapse: collapse;						background-position: initial initial;						background-repeat: initial initial;						background: #fff;					'				>					<tbody>						<tr>							<th								valign='middle'								style='									height: 25px;									line-height: 25px;									padding: 15px 35px;									border-bottom-width: 1px;									border-bottom-style: solid;									border-bottom-color: #D979A2;									background-color: #D979A2;									border-top-left-radius: 5px;									border-top-right-radius: 5px;									border-bottom-right-radius: 0px;									border-bottom-left-radius: 0px;									text-align: center;font-size: x-large;color: white;								'							>CnGal资料站</th>						</tr>						<tr>							<td>								<div									style='										padding: 25px 35px 40px;										background-color: #fff;										max-width: 550px;									'								>									<h2 style='margin: 5px 0px'>										<font color='#333333' style='line-height: 20px'>											<font style='line-height: 22px' size='4'>												亲爱的 " + userName + "</font											>										</font>									</h2>									<p>										非常感谢您的访问，欢迎来到 CnGal资料站，在完成此部分前，我们需要先验证一下您的邮箱哦。<br />							<br />	<h1>验证码：" + num + "</h1><br />			<br />								最后，感谢您的访问，祝您使用愉快！									</p>									<p align='right'>CnGal资料站 管理团队</p>									<div style='width: 550px; margin: 0 auto'>										<div											style='												padding: 10px 10px 0;												border-top: 1px solid #ccc;												color: #747474;												margin-bottom: 20px;												line-height: 1.3em;												font-size: 12px;											'										>											<p>												此为系统邮件，请勿回复<br />												请保管好您的邮箱，避免账号被他人盗用											</p>											<p>												©CnGal.org<br />												<a													href='https://www.cngal.org/'													rel='noopener'													target='_blank'													>https://www.cngal.org/</a												>											</p>										</div>									</div>								</div>							</td>						</tr>					</tbody>				</table>			</div>			<style type='text/css'>				.qmbox style,				.qmbox script,				.qmbox head,				.qmbox link,				.qmbox meta {					display: none !important;				}			</style>		</div>	</div>	<!-- --><style>		#mailContentContainer .txt {			height: auto;		}	</style></div>", true);
                //增加发送次数
                await AddSendCount(email, SendType.Email);
                return null;
            }
            catch (Exception exc)
            {
                return exc.Message;
            }
        }


        public async Task<string> SendVerificationSMSAsync(string longToken, string phone, string userName, SMSType type)
        {
            //检查是否超过上限
            if (await IsExceedMaxSendCount(phone, 10, 10))
            {
                return "验证码超过发送次数上限";
            }
            try
            {
                var num = await GetShortTokenAsync(longToken, userName);
                if (num == 0)
                {
                    return null;
                }
                var client = CreateClient(_configuration["AccessKeyId"], _configuration["AccessKeySecret"]);
                var sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
                {
                    PhoneNumbers = phone,
                    SignName = "cngal",
                    TemplateCode = GetSMSCodeFromSMSType(type),
                    TemplateParam = "{ 'code':'" + num.ToString() + "'}",
                };

                // 复制代码运行请自行打印 API 的返回值
                var result = client.SendSms(sendSmsRequest);
                if (result.Body.Code == "OK")
                {
                    //增加发送次数
                    await AddSendCount(phone, SendType.Phone);
                    return null;
                }
                else
                {
                    return result.Body.Message;
                }

            }
            catch (Exception exc)
            {
                return exc.Message;
            }
        }

        public static string GetSMSCodeFromSMSType(SMSType type)
        {
            return type switch
            {
                SMSType.Register => "SMS_223580709",
                SMSType.ForgetPassword => "SMS_223585679",
                SMSType.ChangeMobilePhoneNumber => "SMS_223585685",
                _ => null,
            };
        }

        /// <summary>
        /// 初始化账号Client
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="accessKeySecret"></param>
        /// <returns></returns>
        public static AlibabaCloud.SDK.Dysmsapi20170525.Client CreateClient(string accessKeyId, string accessKeySecret)
        {
            var config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 您的AccessKey ID
                AccessKeyId = accessKeyId,
                // 您的AccessKey Secret
                AccessKeySecret = accessKeySecret,
            };
            // 访问的域名
            config.Endpoint = "dysmsapi.aliyuncs.com";
            return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
        }

        public async Task<ApplicationUser> GetAPICurrentUserAsync(HttpContext context)
        {
            var test = context.Request.Path;
            var bearer = context.Request.Headers["Authorization"].FirstOrDefault();
            ApplicationUser result = null;
            if (string.IsNullOrWhiteSpace(bearer) || !bearer.Contains("Bearer"))
            {
                try
                {
                    var name = context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Name).Value;
                    //获取当前用户ID
                    result = await _userManager.FindByNameAsync(name);
                }
                catch
                {

                }
                return null;
            }
            else
            {
                var jwt = bearer.Split(' ');
                var tokenObj = new JwtSecurityToken(jwt[1]);

                var claimsIdentity = new ClaimsIdentity(tokenObj.Claims);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                context.User = claimsPrincipal;

                var name = context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Name).Value;

                //获取当前用户ID
                result = await _userManager.FindByNameAsync(name);
            }

            //判断是否被封禁
            if (result != null)
            {
                if (result.UnsealTime == null)
                {
                    return result;
                }
                else
                {
                    if (result.UnsealTime < DateTime.Now.ToCstTime())
                    {
                        result.UnsealTime = null;
                        await _userManager.UpdateAsync(result);
                        return result;
                    }
                }
            }

            return null;

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

        /// <summary>
        /// 写文件到目的路径
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="destinationPath">目的地址</param>
        /// <returns></returns>
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

        public async Task<string> UploadImageNewAsync(FileManager fileManager, IFormFile image, double x = 0, double y = 0, int server = 0, string path = "", string name = "")
        {

            //获取保存路径
            string uploadsFolder;
            string uniqueFileName;
            string filePath;
            string tempFilePath;
            if (path == "")
            {
                uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "images");
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

            }
            else
            {
                uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, path);
                uniqueFileName = name;
            }
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
                File.Copy(filePath, tempFilePath);
            }

            File.Delete(filePath);

            //再压缩图片
            if ((new FileInfo(tempFilePath)).Length > 500 * 1024 && server != 1)
            {
                GetPicThumbnail(tempFilePath, filePath, 0.7);
            }
            else
            {
                File.Copy(tempFilePath, filePath);
            }
            // 删除临时文件
            // File.Delete(tempFilePath);

            //检查是否已经上传过该文件
            var sha1 = ToolHelper.ComputeFileSHA1(filePath);
            if (string.IsNullOrWhiteSpace(sha1) == false)
            {
                //检查数据库中是否有相同的文件
                var userFile = await _userFileRepository.FirstOrDefaultAsync(s => s.Sha1 == sha1);
                if (userFile != null)
                {
                    return userFile.FileName;
                }
            }

            //上传文件到远程服务器
            string UploadResults = null;
            if (server == 0)
            {
                UploadResults = await UploadFileToBserver(filePath);

            }
            else if (server == 1)
            {
                UploadResults = await UploadFileToBserver(filePath);
            }

            if (string.IsNullOrWhiteSpace(UploadResults) == false)
            {
                uniqueFileName = UploadResults;
                // 再填充用户文件管理
                var fileInfo = new FileInfo(filePath);
                var length = fileInfo.Length;

                fileManager.UserFiles.Add(new UserFile
                {
                    FileName = uniqueFileName,
                    UploadTime = DateTime.Now.ToCstTime(),
                    FileSize = length,
                    Sha1 = sha1,
                    UserId = fileManager.ApplicationUserId
                });
                fileManager.UsedSize += length;
                //更新用户文件列表
                await _fileManagerRepository.UpdateAsync(fileManager);

                return uniqueFileName;
            }
            else
            {
                throw new Exception("上传文件到远程服务器失败，请尽快联系管理员");
            }
        }



        private async Task<string> UploadFileToBserver(string filePath)
        {
            using var content = new MultipartFormDataContent();

            content.Add(
                content: new StringContent(GetFileBase64(filePath)),
                name: "source");

            var client = _clientFactory.CreateClient();

            var response = await client.PostAsync("https://pic.cngal.top/api/1/upload/?format=txt&key=" + _configuration["SliotsImageAPIToken"], content);

            var newUploadResults = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK && string.IsNullOrWhiteSpace(newUploadResults) == false && newUploadResults != "Duplicated upload")
            {
                return newUploadResults;
            }
            else
            {
                return null;

            }
        }

        public async Task<bool> DeleteFieAsync(ApplicationUser user, string fileName)
        {
            //获取文件管理
            var fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (fileManager == null)
            {
                return false;
            }
            else
            {
                var userFile = fileManager.UserFiles.FirstOrDefault(s => s.FileName == fileName);
                if (userFile != null)
                {
                    DeleteFile(fileName);
                    fileManager.UserFiles.Remove(userFile);
                    fileManager.UsedSize -= (long)userFile.FileSize;
                    _fileManagerRepository.Update(fileManager);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteFile(string name)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uploadsFolder = Path.Combine(uploadsFolder, name);
                File.Delete(uploadsFolder);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<string> GetTagListStringAsync(Tag tag)
        {
            if (tag == null)
            {
                return null;
            }
            else
            {
                var result = tag.Name;
                //尝试向上索引
                while (tag.ParentCodeNavigation != null)
                {
                    tag = await _tagRepository.GetAll().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Name == tag.ParentCodeNavigation.Name);
                    if (tag != null)
                    {
                        result = tag.Name + "->" + result;
                    }
                    else
                    {
                        break;
                    }
                }
                return result;
            }
        }

        public async Task<bool> IsTagLockedAsync(int tagId, string userId, Operation operation)
        {
            if (await _examineRepository.FirstOrDefaultAsync(s => s.TagId == tagId && s.ApplicationUserId != userId && s.IsPassed == null && s.Operation == operation) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }



        private static string GetFileBase64(string path)
        {
            var fsForRead = new FileStream(path, FileMode.Open);
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
            return null;
        }

        public async Task<string> GetUserJWTokenAsync(ApplicationUser user)
        {
            var roles = await _signInManager.UserManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("image", GetImagePath(user.PhotoPath, "user.png")),
                new Claim("mbgimage", GetImagePath(user.MBgImage, "background.png")),
                new Claim("sbgimage", GetImagePath(user.SBgImage, "CnGal5thMin.png")),
                new Claim("userid",user.Id, "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.ToCstTime().AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtAudience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task ArticleReaderNumUpAsync(long articleId)
        {
            await _articleRepository.GetRangeUpdateTable().Where(s => s.Id == articleId).Set(s => s.ReaderCount, b => b.ReaderCount + 1).ExecuteAsync();

        }

        public async Task EntryReaderNumUpAsync(int entryId)
        {
            await _entryRepository.GetRangeUpdateTable().Where(s => s.Id == entryId).Set(s => s.ReaderCount, b => b.ReaderCount + 1).ExecuteAsync();
        }

        public async Task UpdateFavoritesCountAsync(long[] favoriteFolderIds)
        {
            foreach (var item in favoriteFolderIds)
            {
                var count = await _favoriteFolderRepository.GetAll().Include(s => s.FavoriteObjects).Where(s => s.Id == item).Select(s => s.FavoriteObjects.Count).FirstOrDefaultAsync();
                await _favoriteFolderRepository.GetRangeUpdateTable().Where(s => s.Id == item).Set(s => s.Count, b => count).ExecuteAsync();
            }
        }

        public async Task DeleteComment(long id)
        {
            var comment = await _commentRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == id);
            if (comment == null)
            {
                return;
            }
            //删除关联审核
            await _examineRepository.DeleteAsync(s => s.CommentId == id);
            //遍历删除子评论
            foreach (var item in comment.InverseParentCodeNavigation)
            {
                await DeleteComment(item.Id);
            }
            await _commentRepository.DeleteAsync(comment);
            return;
        }

        public async Task ExaminePublishCommentTextAsync(Comment comment, CommentText examine)
        {

            //复制基础数据
            comment.Text = examine.Text;
            comment.CommentTime = examine.CommentTime;
            comment.Type = examine.Type;
            comment.ApplicationUserId = examine.PubulicUserId;
            comment.Text = examine.Text;

            //查找父对象
            Article article = null;
            Entry entry = null;
            Periphery periphery = null;
            Vote vote = null;
            Lottery lottery = null;
            UserSpaceCommentManager userSpace = null;
            Comment replyComment = null;
            ApplicationUser userTemp = null;
            long tempId = 0;
            if (examine.Type != CommentType.CommentUser)
            {
                tempId = long.Parse(examine.ObjectId);
            }
            //判断当前是否能够编辑
            switch (examine.Type)
            {
                case CommentType.CommentArticle:
                    article = await _articleRepository.GetAll().Include(s => s.CreateUser).FirstOrDefaultAsync(s => s.Id == tempId);
                    if (article == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentEntries:
                    entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (entry == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentPeriphery:
                    periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (periphery == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentVote:
                    vote = await _voteRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (vote == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentLottery:
                    lottery = await _lotteryRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (lottery == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentUser:
                    userTemp = await _userRepository.GetAll().Include(s => s.UserSpaceCommentManager).FirstOrDefaultAsync(s => s.Id == examine.ObjectId);
                    if (userTemp == null)
                    {
                        //判断是不是本人
                        if (examine.PubulicUserId != userTemp?.Id)
                        {
                            return;
                        }
                    }
                    if (userTemp.UserSpaceCommentManager == null)
                    {
                        userTemp.UserSpaceCommentManager = new UserSpaceCommentManager();
                        userTemp = await _userRepository.UpdateAsync(userTemp);
                    }
                    userSpace = userTemp.UserSpaceCommentManager;
                    break;
                case CommentType.ReplyComment:
                    replyComment = await _commentRepository.GetAll().Include(s => s.ApplicationUser).Include(s => s.Article).Include(s => s.Entry).Include(s => s.UserSpaceCommentManager).FirstOrDefaultAsync(s => s.Id == tempId);
                    if (replyComment == null)
                    {
                        return;
                    }
                    break;
                default:
                    return;
            }


            //关联父对象
            if (examine.Type != CommentType.CommentUser)
            {
                tempId = long.Parse(examine.ObjectId);
            }
            switch (comment.Type)
            {
                case CommentType.CommentArticle:
                    comment.ArticleId = tempId;
                    break;
                case CommentType.CommentEntries:
                    comment.EntryId = (int)tempId;
                    break;
                case CommentType.CommentPeriphery:
                    comment.PeripheryId = tempId;
                    break;
                case CommentType.CommentVote:
                    comment.VoteId = tempId;
                    break;
                case CommentType.CommentLottery:
                    comment.LotteryId = tempId;
                    break;
                case CommentType.CommentUser:
                    comment.UserSpaceCommentManager = userSpace;
                    comment.UserSpaceCommentManagerId = userSpace.Id;
                    break;
                case CommentType.ReplyComment:
                    //同步关联父对象的关联对象
                    comment.Article = replyComment.Article;
                    comment.Entry = replyComment.Entry;
                    comment.UserSpaceCommentManager = replyComment.UserSpaceCommentManager;
                    comment.ParentCodeNavigation = replyComment;
                    break;
            }



            //保存
            comment = await _commentRepository.UpdateAsync(comment);
            //获取发表评论的用户
            var user = await _userManager.FindByIdAsync(examine.PubulicUserId);
            //向归属者发送消息
            Message message = null;
            switch (examine.Type)
            {
                case CommentType.CommentArticle:
                    if (examine.PubulicUserId == article.CreateUser.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在你的文章『" + (article.DisplayName ?? article.Name) + "』下回复了你『\n" + examine.Text + "\n』",
                        Link = "articles/index/" + article.Id,
                        LinkTitle = article.Name,
                        Type = MessageType.ArticleReply,
                        ApplicationUser = article.CreateUser,
                        ApplicationUserId = article.CreateUser.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
                case CommentType.CommentUser:
                    if (user.Id == userTemp.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在你的空间下留言『\n" + examine.Text + "\n』",
                        Link = "space/index/" + userTemp.Id,
                        LinkTitle = userTemp.UserName,
                        Type = MessageType.SpaceReply,
                        ApplicationUser = userTemp,
                        ApplicationUserId = userTemp.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
                case CommentType.ReplyComment:
                    if (user.Id == replyComment.ApplicationUser.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在你的评论『" + GetStringAbbreviation(replyComment.Text, 20) + "』下回复了你『\n" + examine.Text + "\n』",
                        Type = MessageType.CommentReply,
                        ApplicationUser = replyComment.ApplicationUser,
                        ApplicationUserId = replyComment.ApplicationUser.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
            }
            if (message != null)
            {
                await _messageRepository.InsertAsync(message);

            } //缓存评论数
            var tempCount = 0;
            switch (comment.Type)
            {
                case CommentType.CommentArticle:
                    tempCount = await _commentRepository.CountAsync(s => s.ArticleId == tempId);
                    await _articleRepository.GetRangeUpdateTable().Where(s => s.Id == tempId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
                case CommentType.CommentEntries:
                    tempCount = await _commentRepository.CountAsync(s => s.EntryId == tempId);
                    await _entryRepository.GetRangeUpdateTable().Where(s => s.Id == tempId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
                case CommentType.CommentPeriphery:
                    tempCount = await _commentRepository.CountAsync(s => s.PeripheryId == tempId);
                    await _peripheryRepository.GetRangeUpdateTable().Where(s => s.Id == tempId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
                case CommentType.CommentVote:
                    tempCount = await _commentRepository.CountAsync(s => s.VoteId == tempId);
                    await _voteRepository.GetRangeUpdateTable().Where(s => s.Id == tempId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
                case CommentType.CommentLottery:
                    tempCount = await _commentRepository.CountAsync(s => s.VoteId == tempId);
                    await _lotteryRepository.GetRangeUpdateTable().Where(s => s.Id == tempId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
            }
        }

        public async Task UniversalCommentExaminedAsync(Comment comment, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    Comment = comment,
                    PassedAdminName = user.UserName,
                    CommentId = comment.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                var examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    PassedTime = null,
                    Comment = comment,
                    CommentId = comment.Id,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user,
                    Note = note
                };
                //添加到审核列表
                await _examineRepository.InsertAsync(examine);
            }
        }

        public async Task<bool> IsUserHavePermissionForCommmentAsync(long commentId, ApplicationUser user)
        {
            var comment = await _commentRepository.GetAll().Include(s => s.Article).Include(s => s.Entry).Include(s => s.UserSpaceCommentManager).Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == commentId);
            if (comment != null)
            {
                //判断评论是否归属该用户
                if (comment.ApplicationUserId == user.Id)
                {

                }
                else if (comment.Article != null && comment.Article.CreateUserId == user.Id)
                {

                }
                else if (await _userManager.IsInRoleAsync(user, "Admin"))
                {

                }
                else if (comment.UserSpaceCommentManager != null && comment.UserSpaceCommentManager.ApplicationUserId == user.Id)
                {

                }
                else
                {
                    //不符合上述条件 返回
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> LoginHistoryUser(string name)
        {
            //查找历史用户 顺序为 登入Id 电子邮箱 昵称
            var historyUser = await _historyUserRepository.FirstOrDefaultAsync(s => s.LoginName == name || s.Email == name || s.UserName == name);
            if (historyUser == null)
            {
                return null;
            }

            //通过后更新关联用户的密码
            var user = await _userManager.FindByNameAsync(historyUser.UserName);

            //获取长令牌
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            //获取短令牌
            await GetShortTokenAsync(token, user.UserName);
            //因为此处已经通过人机验证 直接获取验证码
            var result_2 = await SendVerificationEmailAsync(null, historyUser.Email, user.UserName);
            if (result_2 != null)
            {
                return null;
            }
            //删除历史用户
            // await _historyUserRepository.DeleteAsync(historyUser);
            return user.UserName;
        }

        public async Task<bool> IsExceedMaxErrorCount(string text, int limit, int maxMinutes)
        {
            var timeNow = DateTime.Now.ToCstTime();
            var errors = await _errorCountRepository.GetAll().CountAsync(s => s.Text == text && s.LastUpdateTime.AddMinutes(maxMinutes) > timeNow);
            if (errors < limit)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task AddErrorCount(string text)
        {
            await _errorCountRepository.InsertAsync(new ErrorCount
            {
                LastUpdateTime = DateTime.Now.ToCstTime(),
                Text = text
            });
        }

        public async Task RemoveErrorCount(string text)
        {
            //查找是否存在计数器
            var error = await _errorCountRepository.FirstOrDefaultAsync(s => s.Text == text);
            if (error != null)
            {
                await _errorCountRepository.DeleteAsync(error);
                return;
            }
        }

        public async Task<bool> IsExceedMaxSendCount(string mail, int limit, int maxMinutes)
        {
            var timeNow = DateTime.Now.ToCstTime();
            var errors = await _sendCountRepository.GetAll().CountAsync(s => s.Mail == mail && s.SendTime.AddMinutes(maxMinutes) > timeNow);
            if (errors < limit)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task AddSendCount(string mail, SendType type)
        {
            await _sendCountRepository.InsertAsync(new SendCount
            {
                SendTime = DateTime.Now.ToCstTime(),
                Type = type,
                Mail = mail
            });
        }

        public bool CheckRecaptcha(string challenge, string validate, string seccode)
        {
            try
            {
                GeetestLibResult result = null;
                IDictionary<string, string> paramDict = new Dictionary<string, string> { };
                var gtLib = new GeetestLib(_configuration["GEETEST_ID"], _configuration["GEETEST_KEY"]);
                result = gtLib.SuccessValidate(challenge, validate, seccode, paramDict);
                if (result.GetStatus() == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public BootstrapBlazor.Components.ChartDataSource GetCountLine(Dictionary<string, List<CountLineModel>> data, string xString, string yString, string title)
        {
            var ds = new BootstrapBlazor.Components.ChartDataSource();
            ds.Options.X.Title = xString;
            ds.Options.Y.Title = yString;

            //查找最大的天数
            var maxDay = DateTime.MinValue;
            var minDay = DateTime.MaxValue;
            foreach (var item in data)
            {
                if (item.Value.Count == 0)
                {
                    continue;
                }
                var tempMaxDay = item.Value.Max(s => s.Time);
                if (tempMaxDay > maxDay)
                {
                    maxDay = tempMaxDay;
                }

                var TempMinDay = item.Value.Min(s => s.Time);
                if (TempMinDay < minDay)
                {
                    minDay = TempMinDay;
                }
            }


            var labels = new List<string>();

            for (var i = minDay; i <= maxDay; i = i.AddDays(1))
            {
                labels.Add(i.ToString("MM-dd"));
            }

            ds.Labels = labels;
            ds.Options.Title = title;

            foreach (var dataList in data)
            {
                if (dataList.Value.Count > 0)
                {
                    var temp = new List<object>();
                    var lastTime = dataList.Value.First().Time;

                    //给没有数据的天数添加默认值0
                    for (var i = minDay; i < lastTime; i = i.AddDays(1))
                    {
                        temp.Add(0);
                    }
                    foreach (var item in dataList.Value)
                    {
                        //给没有数据的天数添加默认值0
                        for (var i = lastTime.AddDays(1); i < item.Time; i = i.AddDays(1))
                        {
                            temp.Add(0);
                        }
                        //添加数据
                        temp.Add(item.Count);
                        lastTime = item.Time;
                    }
                    //给没有数据的天数添加默认值0
                    for (var i = lastTime.AddDays(1); i <= maxDay; i = i.AddDays(1))
                    {
                        temp.Add(0);
                    }

                    ds.Data.Add(new BootstrapBlazor.Components.ChartDataset()
                    {
                        Label = dataList.Key,

                        Data = temp
                    });
                }
            }

            return ds;
        }


        public ArticleInforTipViewModel GetArticleInforTipViewModel(Article item)
        {
            return new ArticleInforTipViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Type = item.Type,
                DisplayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.Name : item.DisplayName,
                CreateUserName = item.CreateUser?.UserName,
                MainImage = GetImagePath(item.MainPicture, "certificate.png", true),
                BriefIntroduction = item.BriefIntroduction,
                LastEditTime = item.LastEditTime,
                ReaderCount = item.ReaderCount,
                ThumbsUpCount = item.ThumbsUpCount,
                CommentCount = item.CommentCount,
                Link = item.OriginalLink
            };
        }

        public async Task<EntryInforTipViewModel> GetEntryInforTipViewModel(Entry entry)
        {
            //预处理图片
            if (entry.Type == EntryType.Staff || entry.Type == EntryType.Role)
            {
                entry.MainPicture = GetImagePath(entry.Thumbnail, "user.png", true);
            }
            else
            {
                entry.MainPicture = GetImagePath(entry.MainPicture, "app.png", true);

            }
            var model = new EntryInforTipViewModel
            {
                Id = entry.Id,
                Name = entry.Name,
                Type = entry.Type,
                DisplayName = string.IsNullOrWhiteSpace(entry.DisplayName) ? entry.Name : entry.DisplayName,
                MainImage = entry.MainPicture,
                BriefIntroduction = entry.BriefIntroduction,
                LastEditTime = entry.LastEditTime,
                ReaderCount = entry.ReaderCount,
                CommentCount = entry.CommentCount,
                AddInfors = new List<EntryInforTipAddInforModel>()
            };

            //处理附加信息
            if (entry.Information != null && entry.EntryRelationFromEntryNavigation != null)
            {
                if (entry.Type == EntryType.Role)
                {
                    //查找配音
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息" && item.DisplayName == "声优" && string.IsNullOrWhiteSpace(item.DisplayValue) == false)
                        {
                            var cvs = item.DisplayValue.Replace(",", "、").Replace("，", "、").Split("、").ToList();
                            var cvEntries = await _entryRepository.GetAll().Where(s => cvs.Contains(s.Name)).Select(s => new KeyValuePair<int, string>(s.Id, s.DisplayName)).ToListAsync();

                            if (cvEntries.Count != cvs.Count)
                            {
                                foreach (var temp in cvs)
                                {
                                    if (cvEntries.Select(s => s.Value).Contains(temp) == false)
                                    {
                                        cvEntries.Add(new KeyValuePair<int, string>(-1, temp));
                                    }
                                }
                            }
                            model.AddInfors.Add(new EntryInforTipAddInforModel
                            {
                                Modifier = "配音",
                                Contents = cvEntries.Select(s => new StaffNameModel
                                {
                                    DisplayName = s.Value,
                                    Id = s.Key
                                }).ToList()
                            });
                            break;
                        }
                    }
                    //查找登场游戏
                    var gameNames = new List<StaffNameModel>();
                    foreach (var nav in entry.EntryRelationFromEntryNavigation)
                    {
                        var item = nav.ToEntryNavigation;
                        if (item.Type == EntryType.Game && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            gameNames.Add(new StaffNameModel
                            {

                                DisplayName = item.DisplayName,
                                Id = item.Id
                            });
                            if (gameNames.Count >= 3)
                            {
                                break;
                            }
                        }
                    }
                    if (gameNames.Count > 0)
                    {
                        model.AddInfors.Add(new EntryInforTipAddInforModel
                        {
                            Modifier = "登场游戏",
                            Contents = gameNames
                        });
                    }
                }
                else if (entry.Type == EntryType.Staff)
                {
                    //查找参与作品
                    var gameNames = new List<StaffNameModel>();
                    foreach (var nav in entry.EntryRelationFromEntryNavigation)
                    {
                        var item = nav.ToEntryNavigation;
                        if (item.Type == EntryType.Game && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            gameNames.Add(new StaffNameModel
                            {
                                DisplayName = item.DisplayName,
                                Id = item.Id
                            });
                            if (gameNames.Count >= 3)
                            {
                                break;
                            }
                        }
                    }
                    if (gameNames.Count > 0)
                    {
                        model.AddInfors.Add(new EntryInforTipAddInforModel
                        {
                            Modifier = "参与作品",
                            Contents = gameNames
                        });
                    }
                }
                else if (entry.Type == EntryType.ProductionGroup)
                {
                    //查找参与作品
                    var gameNames = new List<StaffNameModel>();
                    foreach (var nav in entry.EntryRelationFromEntryNavigation)
                    {
                        var item = nav.ToEntryNavigation;
                        if (item.Type == EntryType.Game && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            gameNames.Add(new StaffNameModel
                            {
                                DisplayName = item.DisplayName,
                                Id = item.Id
                            });
                            if (gameNames.Count >= 3)
                            {
                                break;
                            }
                        }
                    }
                    if (gameNames.Count > 0)
                    {
                        model.AddInfors.Add(new EntryInforTipAddInforModel
                        {
                            Modifier = "参与作品",
                            Contents = gameNames
                        });
                    }
                }

            }

            return model;
        }

        public TagInforTipViewModel GetTagInforTipViewModel(Tag item)
        {
            return new TagInforTipViewModel
            {
                Id = item.Id,
                Name = item.Name,
                MainImage = GetImagePath(item.MainPicture, "app.png", true),
                BriefIntroduction = item.BriefIntroduction,
                LastEditTime = item.LastEditTime,
                ReaderCount = item.ReaderCount
            };
        }

        public PeripheryInforTipViewModel GetPeripheryInforTipViewModel(Periphery periphery)
        {
            //预处理图片
            periphery.MainPicture = GetImagePath(periphery.MainPicture, "app.png", true);


            periphery.BriefIntroduction = GetStringAbbreviation(periphery.BriefIntroduction, 50);

            var model = new PeripheryInforTipViewModel
            {
                Id = periphery.Id,
                Name = periphery.DisplayName ?? periphery.Name,
                MainImage = periphery.MainPicture,
                BriefIntroduction = periphery.BriefIntroduction,
                LastEditTime = periphery.LastEditTime,
                ReaderCount = periphery.ReaderCount,
                CommentCount = periphery.CommentCount,
                AddInfors = new List<EntryInforTipAddInforModel>()
            };

            //处理附加信息
            if (periphery.Entries != null)
            {
                var temp = new EntryInforTipAddInforModel
                {
                    Modifier = "关联词条",
                    Contents = new List<StaffNameModel>()
                };
                model.AddInfors.Add(temp); ;
                foreach (var item in periphery.Entries)
                {
                    temp.Contents.Add(new StaffNameModel
                    {
                        DisplayName = item.Entry.DisplayName,
                        Id = item.Entry.Id,
                    });


                }
            }

            return model;
        }


        private static string ConvertJsonString(string str)
        {
            //格式化json字符串
            var serializer = new Newtonsoft.Json.JsonSerializer();
            TextReader tr = new StringReader(str);
            var jtr = new JsonTextReader(tr);
            var obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                var textWriter = new StringWriter();
                var jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        public string GetJsonStringView(string jsonStr)
        {
            if (jsonStr == null)
            {
                return "";
            }
            var resulte = "``` json\n" + ConvertJsonString(jsonStr) + "\n```";
            //使用markdown渲染
            return MarkdownToHtml(resulte);
        }

        public string MarkdownToHtml(string str)
        {
            if (str == null)
            {
                return "";
            }
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            return Markdown.ToHtml(str, pipeline);
        }

        public async Task UpdateUserIntegral(ApplicationUser user)
        {
            //计算积分
            //编辑 词条  标签  消歧义页  评论  发表文章
            //积分  10    8      2         5     100
            //
            var signInDaysIntegral = 5;
            var entryIntegral = 50;

            var tagIntegral = entryIntegral;
            var disambigIntegral = entryIntegral;
            var peripheryIntegral = entryIntegral;
            var commentIntegral = signInDaysIntegral;
            var commentLimited = 5;
            var articleIntegral = entryIntegral;

            try
            {
                //编辑
                var temp_1 = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.IsPassed == true)
                 .Select(n => new { Time = n.ApplyTime.Date, n.EntryId, n.TagId, n.ArticleId, n.CommentId, n.DisambigId, n.PeripheryId })
                 // 分类
                 .ToListAsync();
                // 返回汇总样式
                var temp_2 = temp_1.GroupBy(s => s.Time);
                var integral_1 = temp_2.Select(s => s.Where(s => s.EntryId != null).GroupBy(s => s.EntryId).Count()).Sum() * entryIntegral
                                  + temp_2.Select(s => s.Where(s => s.TagId != null).GroupBy(s => s.TagId).Count()).Sum() * tagIntegral
                                  + temp_2.Select(s => s.Where(s => s.DisambigId != null).GroupBy(s => s.DisambigId).Count()).Sum() * disambigIntegral
                                  + temp_2.Select(s => s.Where(s => s.PeripheryId != null).GroupBy(s => s.PeripheryId).Count()).Sum() * peripheryIntegral
                                  + temp_2.Select(s => s.Where(s => s.CommentId != null).GroupBy(s => s.DisambigId)
                                            .Select(s => s.Count() > commentLimited ? commentLimited : s.Count()).Sum()).Sum() * commentLimited;


                //发表文章
                var integral_2 = await _articleRepository.CountAsync(s => s.CreateUserId == user.Id) * articleIntegral;
                //签到
                var integral_3 = await _signInDayRepository.CountAsync(s => s.ApplicationUserId == user.Id) * signInDaysIntegral;


                user.DisplayIntegral = user.Integral + integral_1 + integral_2 + integral_3;


                //计算贡献值
                user.DisplayContributionValue = user.ContributionValue + await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).SumAsync(s => s.ContributionValue);

                await _userManager.UpdateAsync(user);
            }
            catch (Exception)
            {

            }
        }

        public async Task GetAllSteamImageToGame()
        {
            var entries = await _entryRepository.GetAll().Include(s => s.Information).Where(s => string.IsNullOrWhiteSpace(s.MainPicture) && s.Type == EntryType.Game).ToListAsync();

            foreach (var item in entries)
            {
                //查找steamId
                string steamId = null;
                foreach (var infor in item.Information)
                {
                    if (infor.DisplayName == "Steam平台Id")
                    {
                        if (string.IsNullOrWhiteSpace(infor.DisplayValue) == false)
                        {
                            steamId = infor.DisplayValue;
                            break;
                        }

                    }
                }

                if (string.IsNullOrWhiteSpace(steamId) == false)
                {
                    item.MainPicture = "https://media.st.dl.pinyuncloud.com/steam/apps/" + steamId + "/header.jpg";
                    await _entryRepository.UpdateAsync(item);
                }
            }
        }

        public async Task ChangeAllImagesPath()
        {
            var oldName = "pic.sliots.top";
            var newName = "pic.cngal.top";

            //查找词条
            var entries = await _entryRepository.GetAll().Include(s => s.Pictures).ToListAsync();
            foreach (var item in entries)
            {

                if (string.IsNullOrWhiteSpace(item.SmallBackgroundPicture) == false)
                {
                    item.SmallBackgroundPicture = item.SmallBackgroundPicture.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.BackgroundPicture) == false)
                {
                    item.BackgroundPicture = item.BackgroundPicture.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.MainPage) == false)
                {
                    item.MainPage = item.MainPage.Replace(oldName, newName);
                }

                foreach (var infor in item.Pictures)
                {
                    if (string.IsNullOrWhiteSpace(infor.Url) == false)
                    {
                        infor.Url = infor.Url.Replace(oldName, newName);
                    }
                }

                await _entryRepository.UpdateAsync(item);
            }
            //查找文章
            var articles = await _articleRepository.GetAll().ToListAsync();
            foreach (var item in articles)
            {

                if (string.IsNullOrWhiteSpace(item.SmallBackgroundPicture) == false)
                {
                    item.SmallBackgroundPicture = item.SmallBackgroundPicture.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.BackgroundPicture) == false)
                {
                    item.BackgroundPicture = item.BackgroundPicture.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.MainPage) == false)
                {
                    item.MainPage = item.MainPage.Replace(oldName, newName);
                }

                await _articleRepository.UpdateAsync(item);
            }
            //查找用户
            var users = await _userRepository.GetAll().ToListAsync();
            foreach (var item in users)
            {

                if (string.IsNullOrWhiteSpace(item.SBgImage) == false)
                {
                    item.SBgImage = item.SBgImage.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.MBgImage) == false)
                {
                    item.MBgImage = item.MBgImage.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.BackgroundImage) == false)
                {
                    item.BackgroundImage = item.BackgroundImage.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.MainPageContext) == false)
                {
                    item.MainPageContext = item.MainPageContext.Replace(oldName, newName);
                }

                await _userRepository.UpdateAsync(item);
            }
            //查找所有文件
            var files = await _userFileRepository.GetAll().ToListAsync();
            foreach (var item in files)
            {

                if (string.IsNullOrWhiteSpace(item.FileName) == false)
                {
                    item.FileName = item.FileName.Replace(oldName, newName);
                }


                await _userFileRepository.UpdateAsync(item);
            }
        }

        public async Task<string> SetUserLoginKeyAsync(string userId, bool isOneTime = false)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            var keyStr = Guid.NewGuid().ToString();
            var key = await _loginkeyRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            if (key == null)
            {
                await _loginkeyRepository.InsertAsync(new Loginkey
                {
                    UserId = userId,
                    Key = keyStr,
                    IsOneTime = isOneTime,
                    CreateTime = tempDateTimeNow,
                });
            }
            else
            {
                key.CreateTime = tempDateTimeNow;
                key.Key = keyStr;
                key.IsOneTime = isOneTime;
                await _loginkeyRepository.UpdateAsync(key);
            }
            return keyStr;
        }

        public async Task<string> GetUserFromLoginKeyAsync(string key)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            var result = await _loginkeyRepository.FirstOrDefaultAsync(s => s.Key == key && (s.CreateTime).AddHours(1) > tempDateTimeNow);
            if (result != null)
            {
                if (result.IsOneTime)
                {
                    await _loginkeyRepository.DeleteAsync(result);
                }
                return result.UserId;
            }
            return null;
        }

        public async Task DeleteAllBackupInfor()
        {
            await _backUpArchiveDetailRepository.DeleteRangeAsync(s => true);
            await _backUpArchiveRepository.DeleteRangeAsync(s => true);
        }

        public async Task<string> CheckStringCompliance(string text, string ip)
        {
            try
            {
                //检查是否超过上限
                if (await IsExceedMaxErrorCount(ip, 5, 1))
                {
                    return "请求验证用户名合规次数达到上限";
                }

                //初始化
                var API_KEY = _configuration["BaiduAPIKey"];
                var SECRET_KEY = _configuration["BaiduSecretKey"];

                //获取令牌
                var client = _clientFactory.CreateClient();
                var jsonContent = await client.GetStringAsync("https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id=" + API_KEY + "&client_secret=" + SECRET_KEY + "&");
                var obj = JObject.Parse(jsonContent);
                var access_token = obj["access_token"].ToString();


                var token = access_token;
                var host = "https://aip.baidubce.com/rest/2.0/solution/v1/text_censor/v2/user_defined?access_token=" + token + "&text=" + text;

                jsonContent = await client.GetStringAsync(host);
                obj = JObject.Parse(jsonContent);

                var conclusion = obj["conclusion"].ToString();



                //增加审核次数
                await AddErrorCount(ip);

                if (conclusion == "合规")
                {
                    return null;
                }
                else
                {
                    return "文本存在敏感词";
                }
            }
            catch (Exception)
            {
                return "请求审核文本失败";
            }

        }

        public async Task<List<string>> GetSensitiveWordsInText(string text)
        {
            //读取关键词列表
            var words = new List<string>();
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "SensitiveWords", "1.txt");

            using var fs1 = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var sr1 = new StreamReader(fs1);
            while (sr1.EndOfStream == false)
            {
                words.Add(await sr1.ReadLineAsync());
            }

            path = Path.Combine(_webHostEnvironment.WebRootPath, "SensitiveWords", "2.txt");

            using var fs2 = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var sr2 = new StreamReader(fs2);
            while (sr2.EndOfStream == false)
            {
                words.Add(await sr2.ReadLineAsync());
            }

            path = Path.Combine(_webHostEnvironment.WebRootPath, "SensitiveWords", "3.txt");

            using var fs3 = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var sr3 = new StreamReader(fs3);
            while (sr3.EndOfStream == false)
            {
                words.Add(await sr3.ReadLineAsync());
            }
            //释放
            fs1.Close();
            await fs1.DisposeAsync();
            fs2.Close();
            await fs2.DisposeAsync();
            fs3.Close();
            await fs3.DisposeAsync();

            //删除特殊屏蔽词
            words.RemoveAll(s => s == "se");

            //查找
            return ToolHelper.FindStringListInText(text, words);
        }


    }
}
