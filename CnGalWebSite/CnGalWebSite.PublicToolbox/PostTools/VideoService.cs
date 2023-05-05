using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.PostTools;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Helper.Helper;
using CnGalWebSite.PublicToolbox.DataRepositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ReverseMarkdown.Converters;
using System.Net.Http.Json;
using static System.Net.Mime.MediaTypeNames;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public class VideoService : IVideoService
    {
        private readonly IHttpService _httpService;
        private readonly IImageService _imageService;
        private readonly IRepository<RepostVideoModel> _outlinkVideoRepository;
        private readonly ILogger<VideoService> _logger;

        public event Action<KeyValuePair<OutputLevel, string>> ProgressUpdate;

        public VideoService(IHttpService httpService, IRepository<RepostVideoModel> outlinkVideoRepository, IImageService imageService, ILogger<VideoService> logger)
        {
            _httpService = httpService;
            _outlinkVideoRepository = outlinkVideoRepository;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task ProcVideo(RepostVideoModel model, IEnumerable<string> videos, IEnumerable<string> games)
        {
            OnProgressUpdate(model, OutputLevel.Infor, $"开始处理链接 {model.Url}");

            var history = _outlinkVideoRepository.GetAll().FirstOrDefault(s => s.Url == model.Url);
            if (history != null && history.PostTime != null)
            {
                OnProgressUpdate(model, OutputLevel.Dager, $"-> 链接 {model.Url} 已于 {history.PostTime} 提交审核[Id:{history.Id}]");
                return;

            }
            if (videos.Any(s => s == model.Title))
            {
                OnProgressUpdate(model, OutputLevel.Dager, $"与现有视频《{model.Title}》重名，请自定义标题");
                return;
            }
            CreateVideoViewModel createModel = null;
            try
            {
                OnProgressUpdate(model, OutputLevel.Infor, $"获取视频内容");
                createModel = await GetVideoContext(model, games);
                if(createModel == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取链接内容失败 {Link}", model.Url);
                OnProgressUpdate(model, OutputLevel.Dager, "获取文章失败，请检查链接格式");
                return;
            }

            await _outlinkVideoRepository.InsertAsync(model);




            OnProgressUpdate(model, OutputLevel.Infor, $"提交审核");

            model.Id = await Submit(createModel);
            if (model.Id == 0)
            {
                _logger.LogError("提交视频审核失败 {Link}", model.Url);
                OnProgressUpdate(model, OutputLevel.Dager, "提交视频审核失败");
                return;
            }

            model.PostTime = DateTime.Now.ToCstTime();
            await _outlinkVideoRepository.SaveAsync();

            model.CompleteTaskCount++;
            OnProgressUpdate(model, OutputLevel.Infor, $"成功处理 {model.Url}");

        }

        public async Task<CreateVideoViewModel> GetVideoContext(RepostVideoModel item, IEnumerable<string> games)
        {
            CreateVideoViewModel model = new CreateVideoViewModel();
            //获取Id
            model.Relevances.BilibiliId = item.Url.Split('?')?.FirstOrDefault()?.Split('/')?.LastOrDefault(s => string.IsNullOrWhiteSpace(s) == false);
            if (string.IsNullOrWhiteSpace(model.Relevances.BilibiliId))
            {
                _logger.LogError("生成视频失败 {Link}", item.Url);
                OnProgressUpdate(item, OutputLevel.Dager, "获取视频Id失败，请检查链接");
            }


            OnProgressUpdate(item, OutputLevel.Infor, $"获取视频内容");
            //获取视频内容
            var json = await(await _httpService.GetClientAsync()).GetStringAsync(ToolHelper.WebApiPath + "api/thirdparties/GetBilibiliVideoInfor?id=" + model.Relevances.BilibiliId);
            var data = JObject.Parse(json);

            if (data["code"].ToObject<int>() != 0)
            {
                _logger.LogError("生成视频失败 {Link}", item.Url);
                OnProgressUpdate(item, OutputLevel.Dager, "获取视频信息失败，原因：" + data["message"].ToString());
                return null;
            }

            item.CompleteTaskCount++;
            OnProgressUpdate(item, OutputLevel.Infor, $"处理视频信息");

            model.Main.Type = string.IsNullOrWhiteSpace(item.Type) ? data["data"]["View"]["tname"].ToString() : item.Type;
            model.Main.Copyright = data["data"]["View"]["copyright"].ToObject<int>() == 1 ? CopyrightType.Original : CopyrightType.Transshipment;
            model.Main.MainPicture = data["data"]["View"]["pic"].ToString();
            model.Main.Name = model.Main.DisplayName = data["data"]["View"]["title"].ToString().Replace("\n","");
            model.Main.PubishTime =ToolHelper.GetDateTimeFrom1970Ticks(data["data"]["View"]["pubdate"].ToObject<long>());
            model.MainPage.Context = model.Main.BriefIntroduction = (data["data"]["View"]["desc"].ToString() ?? "").Replace(model.Main.Name,"");
            model.Main.Duration = new TimeSpan(0, 0, data["data"]["View"]["duration"].ToObject<int>());
            model.Main.IsInteractive = data["data"]["View"]["rights"]["is_stein_gate"].ToObject<bool>();
            model.Main.IsCreatedByCurrentUser = item.IsCreatedByCurrentUser;
            model.Main.OriginalAuthor = data["data"]["View"]["owner"]["name"].ToString();

            item.CompleteTaskCount++;
            OnProgressUpdate(item, OutputLevel.Infor, $"获取快照拼图");

            try
            {
                //获取视频快照
                var images = await _httpService.GetAsync<List<string>>(ToolHelper.WebApiPath + "api/thirdparties/GetBilibiliVideoImageImposition?id=" + model.Relevances.BilibiliId);

                item.TotalTaskCount += images.Count;


                OnProgressUpdate(item, OutputLevel.Infor, $"转存快照拼图");

               // model.MainPage.Context += "\n";

                foreach (var infor in images)
                {
                    //转存图片
                    OnProgressUpdate(item, OutputLevel.Infor, $"获取图片 {infor}");
                    //model.MainPage.Context += $"\n![]({await _imageService.GetImage(infor)})";
                    model.Images.Images.Add(new DataModel.ViewModel.EditImageAloneModel
                    {
                        Image= await _imageService.GetImage(infor)
                    });
                    item.CompleteTaskCount++;
                }

            }
            catch
            {

            }
            

           
            item.CompleteTaskCount++;
            OnProgressUpdate(item, OutputLevel.Infor, $"转存主图");

            //裁剪主图
            if (string.IsNullOrWhiteSpace(model.Main.MainPicture) == false)
            {
                model.Main.MainPicture = await _imageService.GetImage(model.Main.MainPicture, 460, 215);
            }

            //查找关联词条
            var entries = games.ToList();
            entries.RemoveAll(s => s == "幻觉" || s == "画师" || s == "她" || s == "人间");

            foreach (var infor in entries)
            {
                if (model.MainPage.Context.Contains(infor))
                {
                    model.Relevances.Games.Add(new DataModel.ViewModel.RelevancesModel
                    {
                        DisplayName = infor,
                    });
                }

            }
            foreach (var infor in entries)
            {

                if (model.Main.Name.Contains(infor))
                {
                    model.Relevances.Games.Add(new DataModel.ViewModel.RelevancesModel
                    {
                        DisplayName = infor
                    });
                }
            }

            item.CompleteTaskCount++;

            return model;
        }

        private async Task<long> Submit(CreateVideoViewModel model)
        {
            try
            {
                var result = await _httpService.PostAsync<CreateVideoViewModel, Result>(ToolHelper.WebApiPath + "api/videos/create", model);
                //判断结果
                if (result.Successful == false)
                {
                    throw new Exception(result.Error);
                }
                _logger.LogInformation($"已提交{model.Main.Type}《{model.Main.Name}》审核[Id:{result.Error}]");
                return long.Parse(result.Error);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交视频审核失败");
                return 0;
            }
        }

        protected void OnProgressUpdate(RepostVideoModel model, OutputLevel level, string infor)
        {
            if (level == OutputLevel.Dager)
            {
                model.Error = infor;
            }
            ProgressUpdate?.Invoke(new KeyValuePair<OutputLevel, string>(level, infor));
        }



    }
}
