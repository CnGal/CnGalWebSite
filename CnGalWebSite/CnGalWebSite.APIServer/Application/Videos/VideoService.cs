using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.ExamineModel.Videos;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Helper.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Web;

namespace CnGalWebSite.APIServer.Application.Videos
{



    public class VideoService : IVideoService
    {
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IAppHelper _appHelper;
        private readonly ILogger<VideoService> _logger;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Video>, string, BootstrapBlazor.Components.SortOrder, IEnumerable<Video>>> SortLambdaCacheArticle = new();

        public VideoService(IAppHelper appHelper, ILogger<VideoService> logger, IRepository<Video, long> videoRepository, IRepository<Examine, long> examineRepository, IRepository<Entry, int> entryRepository, IRepository<Article, long> articleRepository)
        {
            _appHelper = appHelper;
            _logger = logger;
            _videoRepository = videoRepository;
            _examineRepository = examineRepository;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
        }

        public Task<QueryData<ListVideoAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListVideoAloneModel searchModel)
        {
            IEnumerable<Video> items = _videoRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction?.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OriginalAuthor))
            {
                items = items.Where(item => item.OriginalAuthor?.Contains(searchModel.OriginalAuthor, StringComparison.OrdinalIgnoreCase) ?? false);
            }
           
            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }

            // 处理 Searchable=true 列与 SeachText 模糊搜索

            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)
                             || (item.BriefIntroduction?.Contains(options.SearchText) ?? false)
                             || (item.OriginalAuthor?.Contains(options.SearchText) ?? false));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheArticle.GetOrAdd(typeof(Video), key => LambdaExtensions.GetSortLambda<Video>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListVideoAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListVideoAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    IsHidden = item.IsHidden,
                    BriefIntroduction = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20),
                    Priority = item.Priority,
                    Type = item.Type??"视频",
                    CreateTime = item.CreateTime,
                    LastEditTime = item.LastEditTime,
                    ReaderCount = item.ReaderCount,
                    OriginalAuthor = item.OriginalAuthor,
                    PubishTime = item.PubishTime,
                    CanComment = item.CanComment
                    //ThumbsUpCount=item.ThumbsUps.Count()
                });
            }

            return Task.FromResult(new QueryData<ListVideoAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }


        public async Task<List<long>> GetIdsFromNames(List<string> names)
        {
            //判断关联是否存在
            var entryId = new List<long>();

            foreach (var item in names)
            {
                var infor = await _videoRepository.GetAll().AsNoTracking().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    throw new Exception("视频 " + item + " 不存在");
                }
                else
                {
                    entryId.Add(infor);
                }
            }
            //删除重复数据
            entryId = entryId.Distinct().ToList();

            return entryId;
        }

        /// <summary>
        /// 更新主要信息
        /// </summary>
        /// <param name="video"></param>
        /// <param name="examine"></param>
        public void UpdateMain(Video video, ExamineMain examine)
        {
            ToolHelper.ModifyDataAccordingToEditingRecord(video, examine.Items);

            //更新最后编辑时间
            video.LastEditTime = DateTime.Now.ToCstTime();
        }

        /// <summary>
        /// 更新关联信息
        /// </summary>
        /// <param name="video"></param>
        /// <param name="examine"></param>
        /// <returns></returns>
        public async Task UpdateRelevances(Video video, VideoRelevances examine)
        {
            UpdateOutlinks(video, examine);
            await UpdateRelatedVideosAsync(video, examine);
            await UpdateRelatedEntries(video, examine);
            await UpdateRelatedArticles(video, examine);
        }

        public void UpdateOutlinks(Video video, VideoRelevances examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = video.Outlinks;

            foreach (var item in examine.Relevances.Where(s => s.Type == RelevancesType.Outlink))
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Name == item.DisplayName)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                            isAdd = true;
                            break;
                        }
                        else
                        {
                            infor.BriefIntroduction = item.DisplayValue;
                            infor.Name = item.DisplayName;
                            infor.Link = item.Link;
                            isAdd = true;
                            break;
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new Outlink
                    {
                        Name = item.DisplayName,
                        BriefIntroduction = item.DisplayValue,
                        Link = item.Link
                    };
                    relevances.Add(temp);
                }
            }

            //更新最后编辑时间
            video.LastEditTime = DateTime.Now.ToCstTime();

        }

        public async Task UpdateRelatedVideosAsync(Video video, VideoRelevances examine)
        {

            //序列化相关性列表 From
            //先读取周边信息
            var relevances = video.VideoRelationFromVideoNavigation;

            foreach (var item in examine.Relevances.Where(s => s.Type == RelevancesType.Video))
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.ToVideo.ToString() == item.DisplayName)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //查找文章
                    var videoNew = await _videoRepository.FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                    if (videoNew != null)
                    {
                        relevances.Add(new VideoRelation
                        {
                            FromVideo = video.Id,
                            FromVideoNavigation = video,
                            ToVideo = videoNew.Id,
                            ToVideoNavigation = videoNew
                        });
                    }
                }
            }
            video.VideoRelationFromVideoNavigation = relevances;



            //更新最后编辑时间
            video.LastEditTime = DateTime.Now.ToCstTime();
        }

        public async Task UpdateRelatedEntries(Video video, VideoRelevances examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = video.Entries;

            foreach (var item in examine.Relevances.Where(s => s.Type == RelevancesType.Entry))
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Id.ToString() == item.DisplayName)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有找到关键词 则新建关键词
                    var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                    if (video != null)
                    {
                        relevances.Add(entry);
                    }

                }
            }

            //更新最后编辑时间
            video.LastEditTime = DateTime.Now.ToCstTime();
        }

        public async Task UpdateRelatedArticles(Video video, VideoRelevances examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = video.Articles;

            foreach (var item in examine.Relevances.Where(s => s.Type == RelevancesType.Article))
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Id.ToString() == item.DisplayName)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有找到关键词 则新建关键词
                    var entry = await _articleRepository.FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                    if (video != null)
                    {
                        relevances.Add(entry);
                    }

                }
            }

            //更新最后编辑时间
            video.LastEditTime = DateTime.Now.ToCstTime();
        }

        /// <summary>
        /// 更新主页
        /// </summary>
        /// <param name="video"></param>
        /// <param name="examine"></param>
        public void UpdateMainPage(Video video, string examine)
        {
            video.MainPage = examine;
            //如果主图为空 则提取主图

            //更新最后编辑时间
            video.LastEditTime = DateTime.Now.ToCstTime();

        }

        /// <summary>
        /// 更新相册
        /// </summary>
        /// <param name="video"></param>
        /// <param name="examine"></param>
        public void UpdateImages(Video video, VideoImages examine)
        {
            //序列化图片列表
            //先读取词条信息
            var pictures = video.Pictures;

            foreach (var item in examine.Images)
            {
                var isAdd = false;
                foreach (var pic in pictures)
                {
                    if (pic.Url == item.Url)
                    {
                        if (item.IsDelete == true)
                        {
                            pictures.Remove(pic);

                        }
                        else
                        {
                            pic.Modifier = item.Modifier;
                            pic.Note = item.Note;
                            pic.Priority = item.Priority;
                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    pictures.Add(new EntryPicture
                    {
                        Url = item.Url,
                        Note = item.Note,
                        Modifier = item.Modifier,
                        Priority = item.Priority
                    });
                }
            }

            //更新最后编辑时间
            video.LastEditTime = DateTime.Now.ToCstTime();

        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="video"></param>
        /// <param name="examine"></param>
        /// <returns></returns>
        public async Task UpdateData(Video video, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.EditVideoMain:
                    ExamineMain examineMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        examineMain = (ExamineMain)serializer.Deserialize(str, typeof(ExamineMain));
                    }

                    UpdateMain(video, examineMain);
                    break;
                case Operation.EditVideoRelevanes:
                    VideoRelevances videoRelevances = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        videoRelevances = (VideoRelevances)serializer.Deserialize(str, typeof(VideoRelevances));
                    }

                    await UpdateRelevances(video, videoRelevances);
                    break;
                case Operation.EditVideoImages:
                    VideoImages videoImages = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        videoImages = (VideoImages)serializer.Deserialize(str, typeof(VideoImages));
                    }

                     UpdateImages(video, videoImages);
                    break;
                case Operation.EditVideoMainPage:
                    UpdateMainPage(video, examine.Context);
                    break;
            }
        }

        /// <summary>
        /// 获取视图模型
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        public VideoViewModel GetViewModel(Video video)
        {
            //建立视图模型
            var model = new VideoViewModel
            {
                Id = video.Id,
                Name = video.Name,
                DisplayName = video.DisplayName,
                Type = video.Type ?? "视频",
                MainPage = video.MainPage,
                PubishTime = video.PubishTime,
                OriginalAuthor = video.OriginalAuthor,
                CreateTime = video.CreateTime,
                ReaderCount = video.ReaderCount,
                LastEditTime = video.LastEditTime,
                CanComment = video.CanComment,
                BriefIntroduction = video.BriefIntroduction,
                IsHidden = video.IsHidden,
                Duration = video.Duration,
                CommentCount = video.CommentCount,
                Copyright = video.Copyright,
                IsCreatedByCurrentUser = video.IsCreatedByCurrentUser,
                IsInteractive = video.IsInteractive,
            };

            //初始化图片
            model.MainPicture = _appHelper.GetImagePath(video.MainPicture, "app.png");
            model.BackgroundPicture = _appHelper.GetImagePath(video.BackgroundPicture, "");
            model.SmallBackgroundPicture = _appHelper.GetImagePath(video.SmallBackgroundPicture, "");

            //初始化主页Html代码

            model.MainPage = _appHelper.MarkdownToHtml(video.MainPage);

            //图片
            var pictures = new List<EntryPicture>();
            foreach (var item in video.Pictures.OrderByDescending(s => s.Priority))
            {
                pictures.Add(new EntryPicture
                {
                    Url = item.Url,
                    Note = item.Note,
                    Modifier = item.Modifier
                });
            }

            //根据分类来重新排列图片
            var picturesViewModels = new List<PicturesViewModel>
            {
                new PicturesViewModel
                {
                    Modifier=null,
                    Pictures=new List<PicturesAloneViewModel>()
                }
            };

            foreach (var item in pictures)
            {
                var isAdd = false;
                foreach (var infor in picturesViewModels)
                {
                    if (infor.Modifier == item.Modifier)
                    {
                        infor.Pictures.Add(new PicturesAloneViewModel
                        {
                            Note = item.Note,
                            Url = _appHelper.GetImagePath(item.Url, "")
                        });
                        isAdd = true;
                        break;
                    }
                }

                if (isAdd == false)
                {
                    picturesViewModels.Add(new PicturesViewModel
                    {
                        Modifier = item.Modifier,
                        Pictures = new List<PicturesAloneViewModel> {
                            new PicturesAloneViewModel
                            {
                                Note=item.Note,
                                Url=_appHelper.GetImagePath(item.Url, "")
                            }
                        }
                    });
                }
            }

            //如果所有图片都有分组 则删除默认空分组
            if (picturesViewModels[0].Pictures.Count == 0)
            {
                picturesViewModels.RemoveAt(0);
            }

            model.Pictures = picturesViewModels;

            //读取词条信息
            var relevances = new List<RelevancesViewModel>();

            foreach (var item in video.Entries.Where(s => s.IsHidden == false))
            {
                model.RelatedEntries.Add(_appHelper.GetEntryInforTipViewModel(item));
            }
            foreach (var item in video.Articles.Where(s => s.IsHidden == false))
            {
                model.RelatedArticles.Add(_appHelper.GetArticleInforTipViewModel(item));
            }
            foreach (var item in video.VideoRelationFromVideoNavigation.Where(s => s.ToVideoNavigation.IsHidden == false).Select(s => s.ToVideoNavigation))
            {
                model.RelatedVideos.Add(_appHelper.GetVideoInforTipViewModel(item));
            }
            foreach (var item in video.Outlinks)
            {
                model.RelatedOutlinks.Add(new RelevancesKeyValueModel
                {
                    DisplayName = item.Name,
                    DisplayValue = item.BriefIntroduction,
                    Link = item.Link,
                });
            }

            return model;
        }

        /// <summary>
        /// 比较新旧模型数据差异并生成审核记录
        /// </summary>
        /// <param name="currentVideo"></param>
        /// <param name="newVideo"></param>
        /// <returns></returns>
        public List<KeyValuePair<object, Operation>> ExaminesCompletion(Video currentVideo, Video newVideo)
        {
            var examines = new List<KeyValuePair<object, Operation>>();
            //第一部分 主要信息

            //添加修改记录
            //新建审核数据对象
            var examineMain = new ExamineMain
            {
                Items = ToolHelper.GetEditingRecordFromContrastData(currentVideo, newVideo)

            };
            if (examineMain.Items.Count > 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(examineMain, Operation.EditVideoMain));

            }



            //第二部分 关联信息
            //创建审核数据模型
            var videoRelevances = new VideoRelevances();

            //处理关联视频

            //遍历当前词条数据 打上删除标签
            foreach (var item in currentVideo.VideoRelationFromVideoNavigation.Select(s => s.ToVideoNavigation))
            {
                videoRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                {
                    DisplayName = item.Id.ToString(),
                    DisplayValue = item.Name,
                    Type = RelevancesType.Video,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in newVideo.VideoRelationFromVideoNavigation)
            {
                var temp = videoRelevances.Relevances.FirstOrDefault(s => s.Type == RelevancesType.Video && s.DisplayName == item.ToVideo.ToString());
                if (temp != null)
                {
                    videoRelevances.Relevances.Remove(temp);
                }
                else
                {
                    videoRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                    {
                        DisplayName = item.ToVideo.ToString(),
                        DisplayValue = item.ToVideoNavigation.Name,
                        Type = RelevancesType.Video,
                        IsDelete = false
                    });
                }
            }

            //处理关联文章
            //遍历当前文章数据 打上删除标签
            foreach (var item in currentVideo.Articles)
            {
                videoRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                {
                    DisplayName = item.Id.ToString(),
                    DisplayValue = item.Name,
                    Type = RelevancesType.Article,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in newVideo.Articles)
            {
                var temp = videoRelevances.Relevances.FirstOrDefault(s => s.Type == RelevancesType.Article && s.DisplayName == item.Id.ToString());
                if (temp != null)
                {
                    videoRelevances.Relevances.Remove(temp);
                }
                else
                {
                    videoRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                    {
                        DisplayName = item.Id.ToString(),
                        DisplayValue = item.Name,
                        Type = RelevancesType.Article,
                        IsDelete = false
                    });
                }
            }

            //处理关联词条
            //遍历当前文章数据 打上删除标签
            foreach (var item in currentVideo.Entries)
            {
                videoRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                {
                    DisplayName = item.Id.ToString(),
                    DisplayValue = item.Name,
                    Type = RelevancesType.Entry,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in newVideo.Entries)
            {
                var temp = videoRelevances.Relevances.FirstOrDefault(s => s.Type == RelevancesType.Entry && s.DisplayName == item.Id.ToString());
                if (temp != null)
                {
                    videoRelevances.Relevances.Remove(temp);
                }
                else
                {
                    videoRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                    {
                        DisplayName = item.Id.ToString(),
                        DisplayValue = item.Name,
                        Type = RelevancesType.Entry,
                        IsDelete = false
                    });
                }
            }

            //处理外部链接

            //遍历当前词条外部链接 打上删除标签
            foreach (var item in currentVideo.Outlinks)
            {
                videoRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                {
                    DisplayName = item.Name,
                    DisplayValue = item.BriefIntroduction,
                    IsDelete = true,
                    Type = RelevancesType.Outlink,
                    Link = item.Link
                });
            }


            //循环查找外部链接是否相同
            foreach (var infor in newVideo.Outlinks)
            {
                var isSame = false;
                foreach (var item in videoRelevances.Relevances.Where(s => s.Type == RelevancesType.Outlink))
                {
                    if (item.DisplayName == infor.Name)
                    {
                        if (item.DisplayValue != infor.BriefIntroduction || item.Link != infor.Link)
                        {
                            item.DisplayValue = infor.BriefIntroduction;
                            item.IsDelete = false;
                            item.Link = infor.Link;
                        }
                        else
                        {
                            videoRelevances.Relevances.Remove(item);
                            isSame = true;
                        }
                        break;

                    }
                }
                if (isSame == false)
                {
                    videoRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                    {
                        DisplayName = infor.Name,
                        DisplayValue = infor.BriefIntroduction,
                        Link = infor.Link,
                        Type = RelevancesType.Outlink,
                        IsDelete = false
                    });
                }
            }

            if (videoRelevances.Relevances.Count != 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(videoRelevances, Operation.EditVideoRelevanes));
            }

            //第三部分 图片
            var videoImages = new VideoImages();
            //先把 当前词条中的图片 都 打上删除标签
            foreach (var item in currentVideo.Pictures)
            {
                videoImages.Images.Add(new EditRecordImage
                {
                    Url = item.Url,
                    Note = item.Note,
                    Priority = item.Priority,
                    Modifier = item.Modifier,
                    IsDelete = true
                });
            }
            //再对比当前
            foreach (var infor in newVideo.Pictures.ToList().Purge())
            {
                var isSame = false;
                foreach (var item in videoImages.Images)
                {
                    if (item.Url == infor.Url)
                    {
                        if (item.Note != infor.Note || item.Modifier != infor.Modifier || item.Priority != infor.Priority)
                        {
                            item.Modifier = infor.Modifier;
                            item.IsDelete = false;
                            item.Note = infor.Note;
                            item.Priority = infor.Priority;
                        }
                        else
                        {
                            videoImages.Images.Remove(item);
                        }
                        isSame = true;
                        break;

                    }
                }
                if (isSame == false)
                {
                    videoImages.Images.Add(new EditRecordImage
                    {
                        Url = infor.Url,
                        Modifier = infor.Modifier,
                        Priority = infor.Priority,
                        Note = infor.Note,
                        IsDelete = false
                    });
                }
            }

            if (videoImages.Images.Count != 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(videoImages, Operation.EditVideoImages));

            }

            //第四部分 主页
            if (newVideo.MainPage != currentVideo.MainPage)
            {
                //序列化
                var resulte = newVideo.MainPage;
                examines.Add(new KeyValuePair<object, Operation>(resulte, Operation.EditVideoMainPage));
            }
            return examines;
        }

        /// <summary>
        /// 生成对比视图
        /// </summary>
        /// <param name="currentVideo"></param>
        /// <param name="newVideo"></param>
        /// <returns></returns>
        public List<VideoViewModel> ConcompareAndGenerateModel(Video currentVideo, Video newVideo)
        {
            var model = new List<VideoViewModel>
            {
                GetViewModel(currentVideo),
                GetViewModel(newVideo)
            };

            return model;
        }

        /// <summary>
        /// 获取编辑状态
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<VideoEditState> GetEditState(ApplicationUser user, long id)
        {
            var model = new VideoEditState();
            //获取该词条的各部分编辑状态
            //读取审核信息
            List<Examine> examineQuery = null;
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
                               .Where(s => s.VideoId == id && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EditVideoImages || s.Operation == Operation.EditVideoMain || s.Operation == Operation.EditVideoMainPage || s.Operation == Operation.EditVideoRelevanes))
                               .Select(s => new Examine
                               {
                                   Operation = s.Operation
                               })
                               .ToListAsync();
            }

            if (user != null)
            {
                if (examineQuery.Any(s => s.Operation == Operation.EditVideoMain))
                {
                    model.MainState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditVideoMainPage))
                {
                    model.MainPageState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditVideoRelevanes))
                {
                    model.RelevancesState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditVideoImages))
                {
                    model.ImagesState = EditState.Preview;
                }
            }
            //获取各部分状态
            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.VideoId == id && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditVideoMain))
                    {
                        model.MainState = EditState.Locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
                if (model.MainPageState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EditVideoMainPage))
                    {
                        model.MainPageState = EditState.Locked;
                    }
                    else
                    {
                        model.MainPageState = EditState.Normal;
                    }
                }
                if (model.RelevancesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditVideoRelevanes))
                    {
                        model.RelevancesState = EditState.Locked;
                    }
                    else
                    {
                        model.RelevancesState = EditState.Normal;
                    }
                }
                if (model.ImagesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditVideoImages))
                    {
                        model.ImagesState = EditState.Locked;
                    }
                    else
                    {
                        model.ImagesState = EditState.Normal;
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// 从编辑模型 复制到 数据模型 主要信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="model"></param>
        public void SetDataFromEditMain(Video item, EditVideoMainViewModel model)
        {
            item.Name = model.Name;
            item.BriefIntroduction = model.BriefIntroduction;
            item.MainPicture = model.MainPicture;
            item.BackgroundPicture = model.BackgroundPicture;
            item.Type = model.Type;
            item.OriginalAuthor = model.OriginalAuthor;
            item.PubishTime = model.PubishTime;
            item.DisplayName = model.DisplayName;
            item.SmallBackgroundPicture = model.SmallBackgroundPicture;
            item.Duration = model.Duration;
            item.IsCreatedByCurrentUser = model.IsCreatedByCurrentUser;
            item.IsInteractive = model.IsInteractive;
            item.Copyright = model.Copyright;
        }

        /// <summary>
        /// 从编辑模型 复制到 数据模型 主页
        /// </summary>
        /// <param name="item"></param>
        /// <param name="model"></param>
        public void SetDataFromEditMainPage(Video item, EditVideoMainPageViewModel model)
        {
            item.MainPage = model.Context;
        }

        /// <summary>
        /// 从编辑模型 复制到 数据模型 关联信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="model"></param>
        /// <param name="entries"></param>
        /// <param name="articles"></param>
        /// <param name="videos"></param>
        public void SetDataFromEditRelevances(Video item, EditVideoRelevancesViewModel model, List<Entry> entries, List<Article> articles, List<Video> videos)
        {
            //加载在关联信息中的网站
            if (string.IsNullOrWhiteSpace(model.BilibiliId) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "bilibili",
                    Link = "https://www.bilibili.com/video/" + HttpUtility.UrlDecode(model.BilibiliId)
                });
            }

            item.Outlinks.Clear();
            item.Entries = entries;
            item.Articles = articles;
            item.VideoRelationFromVideoNavigation = videos.Select(s => new VideoRelation
            {
                ToVideo = s.Id,
                ToVideoNavigation = s
            }).ToList();

            foreach (var infor in model.others)
            {
                item.Outlinks.Add(new Outlink
                {
                    Name = infor.DisplayName,
                    BriefIntroduction = infor.DisPlayValue,
                    Link = infor.Link,
                });
            }

        }

        /// <summary>
        /// 从编辑模型 复制到 数据模型 图片
        /// </summary>
        /// <param name="item"></param>
        /// <param name="model"></param>
        public void SetDataFromEditImages(Video item, EditVideoImagesViewModel model)
        {
            //再遍历视图模型中的图片 对应修改
            item.Pictures.Clear();

            foreach (var infor in model.Images)
            {

                item.Pictures.Add(new EntryPicture
                {
                    Url = infor.Image,
                    Modifier = infor.Modifier,
                    Note = infor.Note,
                    Priority = infor.Priority,
                });

            }
        }

        /// <summary>
        /// 从 数据模型 生成 编辑模型 主要信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public EditVideoMainViewModel GetEditMain(Video item)
        {
            var model = new EditVideoMainViewModel
            {
                Name = item.Name,
                BriefIntroduction = item.BriefIntroduction,
                MainPicture = item.MainPicture,
                BackgroundPicture = item.BackgroundPicture,
                Type = item.Type,
                OriginalAuthor = item.OriginalAuthor,
                PubishTime = item.PubishTime,
                DisplayName = item.DisplayName,
                SmallBackgroundPicture = item.SmallBackgroundPicture,
                Duration = item.Duration,
                IsCreatedByCurrentUser = item.IsCreatedByCurrentUser,
                IsInteractive = item.IsInteractive,
                Copyright = item.Copyright,

                Id = item.Id
            };

            return model;
        }

        /// <summary>
        /// 从 数据模型 生成 编辑模型 相册
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public EditVideoImagesViewModel GetEditImages(Video item)
        {
            //根据类别生成首个视图模型
            var model = new EditVideoImagesViewModel
            {
                Name = item.Name,
                Id = item.Id,
            };
            //处理图片
            var Images = new List<EditImageAloneModel>();
            foreach (var infor in item.Pictures)
            {
                Images.Add(new EditImageAloneModel
                {
                    Image = _appHelper.GetImagePath(infor.Url, ""),
                    Modifier = infor.Modifier,
                    Note = infor.Note,
                    Priority = infor.Priority,
                });
            }

            model.Images = Images;
            return model;
        }

        /// <summary>
        /// 从 数据模型 生成 编辑模型 关联信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public EditVideoRelevancesViewModel GetEditRelevances(Video item)
        {
            var model = new EditVideoRelevancesViewModel
            {
                Id = item.Id,
                Name = item.Name,
            };
            //处理附加信息
            var roles = new List<RelevancesModel>();
            var staffs = new List<RelevancesModel>();
            var articles = new List<RelevancesModel>();
            var groups = new List<RelevancesModel>();
            var games = new List<RelevancesModel>();
            var videos = new List<RelevancesModel>();
            var news = new List<RelevancesModel>();
            var others = new List<RelevancesModel>();

            foreach (var infor in item.Entries)
            {
                switch (infor.Type)
                {
                    case EntryType.Role:
                        roles.Add(new RelevancesModel
                        {
                            DisplayName = infor.Name
                        });
                        break;
                    case EntryType.Staff:
                        staffs.Add(new RelevancesModel
                        {
                            DisplayName = infor.Name
                        });
                        break;
                    case EntryType.ProductionGroup:
                        groups.Add(new RelevancesModel
                        {
                            DisplayName = infor.Name
                        });
                        break;
                    case EntryType.Game:
                        games.Add(new RelevancesModel
                        {
                            DisplayName = infor.Name
                        });
                        break;
                }
            }
            foreach (var infor in item.Articles)
            {
                switch (infor.Type)
                {
                    case ArticleType.News:
                        news.Add(new RelevancesModel
                        {
                            DisplayName = infor.Name
                        });
                        break;
                    default:
                        articles.Add(new RelevancesModel
                        {
                            DisplayName = infor.Name
                        });
                        break;
                }
            }
            foreach (var infor in item.Outlinks)
            {
                if (infor.Link.Contains("www.bilibili.com"))
                {
                    model.BilibiliId = HttpUtility.UrlDecode(infor.Link.Replace("https://www.bilibili.com/video/", "").Split('/').FirstOrDefault());
                }
                else
                {
                    others.Add(new RelevancesModel
                    {
                        DisplayName = infor.Name,
                        DisPlayValue = infor.BriefIntroduction,
                        Link = infor.Link
                    });
                }

            }

            foreach(var infor in item.VideoRelationFromVideoNavigation.Select(s=>s.ToVideoNavigation))
            {
                videos.Add(new RelevancesModel
                {
                    DisplayName = infor.Name
                });
            }

            model.Roles = roles;
            model.staffs = staffs;
            model.articles = articles;
            model.Groups = groups;
            model.Games = games;
            model.others = others;
            model.news = news;
            model.videos = videos;

            return model;
        }

        /// <summary>
        /// 从 数据模型 生成 编辑模型 主页
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public EditVideoMainPageViewModel GetEditMainPage(Video item)
        {

            var model = new EditVideoMainPageViewModel
            {
                Context = item.MainPage,
                Id = item.Id,
                Name = item.Name
            };

            return model;

        }

        /// <summary>
        /// 获取 审核预览视图 主要信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ExaminePreDataModel GetExamineViewMain(Video item)
        {
            var model = new ExaminePreDataModel();

            if (string.IsNullOrWhiteSpace(item.MainPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "主图",
                    Url = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                });
            }

            if (string.IsNullOrWhiteSpace(item.BackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "大背景图",
                    Url = _appHelper.GetImagePath(item.BackgroundPicture, "app.png"),
                });
            }
            if (string.IsNullOrWhiteSpace(item.SmallBackgroundPicture) == false)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Note = "小背景图",
                    Url = _appHelper.GetImagePath(item.SmallBackgroundPicture, "app.png"),
                });
            }

            var texts = new List<KeyValueModel>();

            if (string.IsNullOrWhiteSpace(item.Name) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "唯一名称",
                    DisplayValue = item.Name,
                });
            }
            if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "显示名称",
                    DisplayValue = item.DisplayName,
                });
            }

            if (string.IsNullOrWhiteSpace(item.BriefIntroduction) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "简介",
                    DisplayValue = item.BriefIntroduction,
                });
            }
            texts.Add(new KeyValueModel
            {
                DisplayName = "类型",
                DisplayValue = item.Type,
            });
            texts.Add(new KeyValueModel
            {
                DisplayName = "版权",
                DisplayValue = item.Copyright.GetDisplayName(),
            });
            texts.Add(new KeyValueModel
            {
                DisplayName = "互动视频",
                DisplayValue = item.IsInteractive?"是":"否",
            });
            texts.Add(new KeyValueModel
            {
                DisplayName = "本人创作",
                DisplayValue = item.IsCreatedByCurrentUser ? "是" : "否",
            });
            texts.Add(new KeyValueModel
            {
                DisplayName = "时长",
                DisplayValue = item.Duration.ToString()
            });


            if (string.IsNullOrWhiteSpace(item.OriginalAuthor) == false)
            {
                texts.Add(new KeyValueModel
                {
                    DisplayName = "作者",
                    DisplayValue = item.OriginalAuthor,
                });
            }

            texts.Add(new KeyValueModel
            {
                DisplayName = "发布时间",
                DisplayValue = item.PubishTime.ToString("G"),
            });

            model.Texts.Add(new InformationsModel
            {
                Modifier = "主要信息",
                Informations = texts
            });

            return model;
        }

        /// <summary>
        /// 获取 审核预览视图 关联信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ExaminePreDataModel GetExamineViewRelevances(Video item)
        {
            var model = new ExaminePreDataModel();

            foreach (var infor in item.Entries)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    entry = _appHelper.GetEntryInforTipViewModel(infor)
                });

            }

            foreach (var infor in item.Articles)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    article = _appHelper.GetArticleInforTipViewModel(infor)
                });

            }

            foreach (var infor in item.VideoRelationFromVideoNavigation)
            {
                model.Relevances.Add(new DataModel.ViewModel.Search.SearchAloneModel
                {
                    video = _appHelper.GetVideoInforTipViewModel(infor.ToVideoNavigation)
                });

            }

            foreach (var infor in item.Outlinks)
            {
                model.Outlinks.Add(new RelevancesKeyValueModel
                {
                    DisplayName = infor.Name,
                    DisplayValue = infor.BriefIntroduction,
                    Link = infor.Link
                });

            }


            return model;
        }

        /// <summary>
        /// 获取 审核预览视图 相册
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ExaminePreDataModel GetExamineViewImages(Video item)
        {
            var model = new ExaminePreDataModel();
            foreach (var infor in item.Pictures)
            {
                model.Pictures.Add(new PicturesAloneViewModel
                {
                    Url = infor.Url,
                    Note = infor.Note,
                    Priority = infor.Priority,
                });
            }

            return model;
        }

    }
}
