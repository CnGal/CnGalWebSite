using CnGalWebSite.DataModel.ViewModel.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using System.Linq;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.Others;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.DataModel.Models;

namespace CnGalWebSite.APIServer.Application.Charts
{
    public class ChartService:IChartService
    {

        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<UserOnlineInfor, long> _userOnlineInforRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ThumbsUp, long> _thumbsUpRepository;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<BackUpArchiveDetail, long> _backUpArchiveDetailRepository;

        public ChartService(IRepository<ApplicationUser, string> userRepository, IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<SignInDay, long> signInDayRepository,
            IRepository<Examine, long> examineRepository, IRepository<Comment, long> commentRepository,
            IRepository<FavoriteObject, long> favoriteObjectRepository, IRepository<Article, long> articleRepository, IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<Message, long> messageRepository, IRepository<UserFile, int> userFileRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository)
        {
            _userRepository = userRepository;
            _userOnlineInforRepository = userOnlineInforRepository;
            _signInDayRepository = signInDayRepository;
            _examineRepository = examineRepository;
            _commentRepository = commentRepository;
            _favoriteObjectRepository = favoriteObjectRepository;
            _articleRepository = articleRepository;
            _thumbsUpRepository = thumbsUpRepository;
            _messageRepository = messageRepository;
            _userFileRepository = userFileRepository;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
        }

        public async Task<LineChartModel> GetLineChartAsync(LineChartType type, DateTime afterTime, DateTime beforeTime)
        {
            return new LineChartModel
            {
                Type = type,
                BeforeTime = beforeTime,
                AfterTime = afterTime,
                Options = type switch
                {
                    LineChartType.User => await GetUserLineChartAsync(afterTime, beforeTime),
                    LineChartType.Entry => await GetEntryLineChartAsync(afterTime, beforeTime),
                    LineChartType.Article => await GetArticleLineChartAsync(afterTime, beforeTime),
                    LineChartType.Tag => await GetTagLineChartAsync(afterTime, beforeTime),
                    LineChartType.Examine => await GetExamineLineChartAsync(afterTime, beforeTime),
                    LineChartType.Comment => await GetCommentLineChartAsync(afterTime, beforeTime),
                    LineChartType.Message => await GetMessageLineChartAsync(afterTime, beforeTime),
                    LineChartType.File => await GetFileLineChartAsync(afterTime, beforeTime),
                    LineChartType.BackUpArchive => await GetBackUpArchiveLineChartAsync(afterTime, beforeTime),
                    LineChartType.Edit => await GetEditLineChartAsync(afterTime, beforeTime),
                    _ => new EChartsOptionModel()
                }
            };
        }

        public async Task<EChartsOptionModel> GetUserLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var registerCounts = await _userRepository.GetAll().Where(s => s.RegistTime <= beforeTime && s.RegistTime >= afterTime)
               .Select(n => new { Time = n.RegistTime.Date })
               .GroupBy(n => n.Time)
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var onlineCounts = await _userOnlineInforRepository.GetAll().Where(s => s.Date <= beforeTime && s.Date >= afterTime)
             .Select(n => new { Time = n.Date.Date })
             .GroupBy(n => n.Time)
             .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
             .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
             .ToListAsync();


            var SignIns = await _signInDayRepository.GetAll().Where(s => s.Time <= beforeTime && s.Time >= afterTime)
             .Select(n => new { Time = n.Time.Date })
             .GroupBy(n => n.Time)
             .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
             .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
             .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["注册"] = registerCounts, ["在线"] = onlineCounts, ["签到"] = SignIns }, "用户");
        }

      
        /// <summary>
        /// 获取词条图表
        /// </summary>
        /// <returns></returns>
        public async Task<EChartsOptionModel> GetEntryLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();

            //获取数据
            var editCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EstablishMain || s.Operation == Operation.EstablishAddInfor || s.Operation == Operation.EstablishImages || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags || s.Operation == Operation.EstablishMainPage)
                                                                           && s.IsPassed == true && s.ApplyTime <= beforeTime && s.ApplyTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            //获取数据
            var commentCounts = await _commentRepository.GetAll().Where(s => s.Type == CommentType.CommentEntries && s.CommentTime <= beforeTime && s.CommentTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CommentTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            //获取数据
            var favoriteCounts = await _favoriteObjectRepository.GetAll().Where(s => s.Type == FavoriteObjectType.Entry && s.CreateTime <= beforeTime && s.CreateTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CreateTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["编辑"] = editCounts, ["评论"] = commentCounts, ["收藏"] = favoriteCounts }, "词条");
        }

        /// <summary>
        /// 获取文章图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<EChartsOptionModel> GetArticleLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var createCounts = await _articleRepository.GetAll().Where(s => s.CreateTime <= beforeTime && s.CreateTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CreateTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var editCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleRelevanes || s.Operation == Operation.EditArticleMainPage)
                                                                           && s.IsPassed == true && s.ApplyTime <= beforeTime && s.ApplyTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            var thumsupCounts = await _thumbsUpRepository.GetAll().Where(s => s.ThumbsUpTime <= beforeTime && s.ThumbsUpTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ThumbsUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var commentCounts = await _commentRepository.GetAll().Where(s => s.Type == CommentType.CommentArticle && s.CommentTime <= beforeTime && s.CommentTime >= afterTime)
              // 先进行了时间字段变更为String字段，切只保留到天
              // 采用拼接的方式
              .Select(n => new { Time = n.CommentTime.Date })
              // 分类
              .GroupBy(n => n.Time)
              // 返回汇总样式
              .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
              .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
              .ToListAsync();

            var favoriteCounts = await _favoriteObjectRepository.GetAll().Where(s => s.Type == FavoriteObjectType.Article && s.CreateTime <= beforeTime && s.CreateTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CreateTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["发表"] = createCounts, ["编辑"] = editCounts, ["点赞"] = thumsupCounts, ["评论"] = commentCounts, ["收藏"] = favoriteCounts },  "文章");
        }

        /// <summary>
        /// 获取标签图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<EChartsOptionModel> GetTagLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var editCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EditTag || s.Operation == Operation.EditTagMain || s.Operation == Operation.EditTagChildTags || s.Operation == Operation.EditTagChildEntries)
                                                                           && s.IsPassed == true && s.ApplyTime <= beforeTime && s.ApplyTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["编辑"] = editCounts },  "标签");
        }

        /// <summary>
        /// 获取审核图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<EChartsOptionModel> GetExamineLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var applyCounts = await _examineRepository.GetAll().Where(s => s.ApplyTime <= beforeTime && s.ApplyTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var passCounts = await _examineRepository.GetAll().Where(s => s.PassedTime != null && s.PassedTime.Value <= beforeTime && s.PassedTime.Value >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.PassedTime.Value.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["申请"] = applyCounts, ["处理"] = passCounts }, "审核");
        }

        /// <summary>
        /// 获取评论图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<EChartsOptionModel> GetCommentLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var publishCounts = await _commentRepository.GetAll().Where(s => s.CommentTime <= beforeTime && s.CommentTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CommentTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["发表"] = publishCounts }, "评论");
        }


        /// <summary>
        /// 获取评论图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<EChartsOptionModel> GetMessageLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var postCounts = await _messageRepository.GetAll().Where(s => s.PostTime <= beforeTime && s.PostTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.PostTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["发送"] = postCounts },  "消息");
        }

        /// <summary>
        /// 获取文件图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<EChartsOptionModel> GetFileLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var Counts = await _userFileRepository.GetAll().Where(s => s.UploadTime <= beforeTime && s.UploadTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.UploadTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var Spaces = await _userFileRepository.GetAll().Where(s => s.UploadTime <= beforeTime && s.UploadTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.UploadTime.Date, Space = ((double)n.FileSize) / (1024 * 1024) })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Sum(s => s.Space) })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["数目"] = Counts, ["大小 MB"] = Spaces },  "文件");
        }

        /// <summary>
        /// 获取备份图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<EChartsOptionModel> GetBackUpArchiveLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var success = await _backUpArchiveDetailRepository.GetAll().Where(s => s.IsFail == false && s.BackUpTime <= beforeTime && s.BackUpTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var times = await _backUpArchiveDetailRepository.GetAll().Where(s => s.BackUpTime <= beforeTime && s.BackUpTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date, n.TimeUsed })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Average(s => s.TimeUsed) })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var errors = await _backUpArchiveDetailRepository.GetAll().Where(s => s.IsFail == true && s.BackUpTime <= beforeTime && s.BackUpTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["成功"] = success, ["错误"] = errors, ["用时 秒"] = times }, "备份");
        }

        public async Task<EChartsOptionModel> GetEditLineChartAsync(DateTime afterTime, DateTime beforeTime)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();

            //获取数据
            var entryCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EstablishMain || s.Operation == Operation.EstablishAddInfor || s.Operation == Operation.EstablishImages
                                                                            || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags || s.Operation == Operation.EstablishMainPage)
                                                                           && s.IsPassed == true && s.ApplyTime <= beforeTime && s.ApplyTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            //获取数据
            var articleCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleMainPage || s.Operation == Operation.EditArticleRelevanes)
                                                                           && s.IsPassed == true && s.ApplyTime <= beforeTime && s.ApplyTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            //获取数据
            var tagCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EditTag || s.Operation == Operation.EditTagChildEntries || s.Operation == Operation.EditTagChildTags || s.Operation == Operation.EditTagMain)
                                                                           && s.IsPassed == true && s.ApplyTime <= beforeTime && s.ApplyTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            //获取数据
            var peripheryCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EditPeripheryMain || s.Operation == Operation.EditPeripheryImages || s.Operation == Operation.EditPeripheryRelatedEntries)
                                                                           && s.IsPassed == true && s.ApplyTime <= beforeTime && s.ApplyTime >= afterTime)
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new LineChartSingleData { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            return GetCountLine(new Dictionary<string, List<LineChartSingleData>> { ["词条"] = entryCounts, ["文章"] = articleCounts, ["标签"] = tagCounts, ["周边"] = peripheryCounts }, "编辑概览");
        }

        public EChartsOptionModel GetCountLine(Dictionary<string, List<LineChartSingleData>> data,  string title)
        {
            var ds = new EChartsOptionModel();

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
            var format = (maxDay - minDay).TotalDays > 365 ? "yyyy-MM-dd" : "MM-dd";
            for (var i = minDay; i <= maxDay; i = i.AddDays(1))
            {
                labels.Add(i.ToString(format));
            }

            ds.XAxis.Data = labels;
            ds.Title.Text = title;

            foreach (var dataList in data)
            {
                if (dataList.Value.Count > 0)
                {
                    var temp = new List<double>();
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

                    ds.Series.Add(new EChartsOptionSery
                    {
                        Name = dataList.Key,

                        Data = temp
                    });

                    ds.Legend.Data.Add(dataList.Key);
                }
            }

            return ds;
        }

    }
}
