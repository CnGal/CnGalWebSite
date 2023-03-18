using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Comments.Dtos;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;

using CnGalWebSite.DataModel.ExamineModel.Comments;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Comments
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<UserSpaceCommentManager, long> _userSpaceCommentManagerRepository;
        private readonly IAppHelper _appHelper;
        private readonly IRankService _rankService;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IUserService _userService;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Comment>, string, SortOrder, IEnumerable<Comment>>> SortLambdaCache = new();

        public CommentService(IAppHelper appHelper, IRepository<Comment, long> commentRepository, IRepository<UserSpaceCommentManager, long> userSpaceCommentManagerRepository, IRepository<Article, long> articleRepository, IRepository<Video, long> videoRepository,
            IUserService userService,
        IRankService rankService, IRepository<Entry, int> entryRepository, IRepository<Periphery, long> peripheryRepository, IRepository<ApplicationUser, string> userRepository, IRepository<Vote, long> voteRepository, IRepository<Lottery, long> lotteryRepository)
        {
            _commentRepository = commentRepository;
            _rankService = rankService;
            _userSpaceCommentManagerRepository = userSpaceCommentManagerRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _entryRepository = entryRepository;
            _peripheryRepository = peripheryRepository;
            _userRepository = userRepository;
            _voteRepository = voteRepository;
            _lotteryRepository = lotteryRepository;
            _videoRepository = videoRepository;
            _userService= userService;
        }

        public async Task<List<CommentViewModel>> GetComments( CommentType type, string Id, string rankName, string ascriptionUserId,IEnumerable<Comment> examineComments)
        {
            //筛选出符合类型的所有评论

            var query = _commentRepository.GetAll().AsNoTracking()
                .Include(s => s.InverseParentCodeNavigation).Include(s => s.ApplicationUser)
                .Where(s => s.ParentCodeNavigation == null&&s.IsHidden==false);
            long tempId = 0;
            if (type != CommentType.CommentUser)
            {
                tempId = long.Parse(Id);
            }

            switch (type)
            {
                case CommentType.CommentArticle:
                    query = query.Where(s => s.Type == type && s.ArticleId == tempId);
                    examineComments = examineComments.Where(s => s.ParentCodeNavigation != null|| s.ArticleId == tempId);
                    break;
                case CommentType.CommentEntries:
                    query = query.Where(s => s.Type == type && s.EntryId == tempId);
                    examineComments = examineComments.Where(s => s.ParentCodeNavigation != null || s.EntryId == tempId);
                    break;
                case CommentType.CommentPeriphery:
                    query = query.Where(s => s.Type == type && s.PeripheryId == tempId);
                    examineComments = examineComments.Where(s => s.ParentCodeNavigation != null || s.PeripheryId == tempId);
                    break;
                case CommentType.CommentVote:
                    query = query.Where(s => s.Type == type && s.VoteId == tempId);
                    examineComments = examineComments.Where(s => s.ParentCodeNavigation != null || s.VoteId == tempId);
                    break;
                case CommentType.CommentLottery:
                    query = query.Where(s => s.Type == type && s.LotteryId == tempId);
                    examineComments = examineComments.Where(s => s.ParentCodeNavigation != null || s.LotteryId == tempId);
                    break;
                case CommentType.CommentVideo:
                    query = query.Where(s => s.Type == type && s.VideoId == tempId);
                    examineComments = examineComments.Where(s => s.ParentCodeNavigation != null || s.VideoId == tempId);
                    break;
                case CommentType.CommentUser:
                    //
                    var userSpace = await _userSpaceCommentManagerRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == Id);
                    if (userSpace != null)
                    {
                        query = query.Where(s => s.Type == type && s.UserSpaceCommentManagerId == userSpace.Id);
                        examineComments = examineComments.Where(s => s.UserSpaceCommentManagerId == userSpace.Id);
                    }
                    else
                    {
                        return new List<CommentViewModel>();
                    }
                    break;
                default:
                    throw new ArgumentException("未知评论类型");
            }


            //统计查询数据的总条数 不包括子评论
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.OrderBy("Priority desc");
            //将结果转换为List集合 加载到内存中
            var tempQuery = await query.ToListAsync();
            //加载预览评论
            tempQuery.AddRange(examineComments.Where(s => s.ParentCodeNavigation == null));

            var models = new List<CommentViewModel>();
            foreach (var item in tempQuery)
            {
                models.Add(await CombinationDataAsync(item, rankName, ascriptionUserId, examineComments));
            }
            return models;
        }

        /// <summary>
        /// 递归复制数据到视图模型
        /// </summary>
        /// <param name="comment">评论</param>
        /// <param name="rankName">归属者头衔</param>
        /// <param name="ascriptionUserId">归属者Id</param>
        /// <param name="examineComments"></param>
        /// <returns></returns>
        private async Task<CommentViewModel> CombinationDataAsync(Comment comment, string rankName, string ascriptionUserId, IEnumerable<Comment> examineComments)
        {

            var model = new CommentViewModel
            {
                ApplicationUserId = comment.ApplicationUserId,
                EntryId = comment.EntryId,
                ArticleId = comment.ArticleId,
                UserSpaceCommentManagerId = comment.UserSpaceCommentManagerId,
                IsHidden = comment.IsHidden,
                CommentTime = comment.CommentTime,
                Type = comment.Type,
                Id = comment.Id,
                UserName = comment.ApplicationUser.UserName,
                UserImage = _appHelper.GetImagePath(comment.ApplicationUser.PhotoPath, "user.png"),
                InverseParentCodeNavigation = new List<CommentViewModel>()
            };
            //提前将MarkDown语法转为Html
            if (comment.IsHidden == false)
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
                model.Text = Markdig.Markdown.ToHtml(comment.Text ?? "", pipeline);
            }
            else
            {
                model.Text = "该评论被隐藏";
            }
            model.Ranks = await _rankService.GetUserRanks(comment.ApplicationUser);

            if (comment.ApplicationUserId == ascriptionUserId)
            {
                model.Ranks.Add(new RankViewModel
                {
                    Text = rankName,
                    Name = rankName,
                    CSS = "bg-primary"
                });
            }

            //递归初始化子评论
            if (comment.InverseParentCodeNavigation.Any())
            {
                foreach (var item in await _commentRepository.GetAll().AsNoTracking().Include(s => s.InverseParentCodeNavigation).Include(s => s.ApplicationUser).Where(s =>s.IsHidden==false&& comment.InverseParentCodeNavigation.Select(s => s.Id).Contains(s.Id)).ToListAsync())
                {
                    model.InverseParentCodeNavigation.Add(await CombinationDataAsync(item, rankName, ascriptionUserId, examineComments));
                }
            }
        
            //递归初始化预览评论
            foreach (var item in examineComments.Where(s =>s.ParentCodeNavigation!=null&& s.ParentCodeNavigation.Id == comment.Id))
            {
                model.InverseParentCodeNavigation.Add(await CombinationDataAsync(item, rankName, ascriptionUserId, examineComments));
            }

            return model;

        }

        public async Task<QueryData<ListCommentAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListCommentAloneModel searchModel, CommentType type = CommentType.None, string objectId = "")
        {
            IEnumerable<Comment> items = _commentRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Text) == false).AsNoTracking().Include(s => s.ParentCodeNavigation);
            //判断评论列表类型
            long tempId = 0;
            if (type != CommentType.None && type != CommentType.CommentUser)
            {
                tempId = long.Parse(objectId);
            }
            switch (type)
            {
                case CommentType.CommentArticle:
                    items = items.Where(s => s.ArticleId == tempId);
                    break;
                case CommentType.CommentEntries:
                    items = items.Where(s => s.EntryId == tempId);
                    break;
                case CommentType.CommentPeriphery:
                    items = items.Where(s => s.PeripheryId == tempId);
                    break;
                case CommentType.CommentVote:
                    items = items.Where(s => s.VoteId == tempId);
                    break;
                case CommentType.CommentLottery:
                    items = items.Where(s => s.LotteryId == tempId);
                    break;
                case CommentType.CommentVideo:
                    items = items.Where(s => s.VideoId == tempId);
                    break;
                case CommentType.CommentUser:
                    var space = await _userSpaceCommentManagerRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == objectId);
                    if (space == null)
                    {
                        return new QueryData<ListCommentAloneModel>();
                    }
                    items = items.Where(s => s.UserSpaceCommentManagerId == space.Id);
                    break;
            }

            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Text))
            {
                items = items.Where(item => item.Text?.Contains(searchModel.Text, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ApplicationUserId))
            {
                items = items.Where(item => item.ApplicationUserId?.Contains(searchModel.ApplicationUserId, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.EntryId?.ToString()))
            {
                items = items.Where(item => item.EntryId.ToString()?.Contains(searchModel.EntryId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ArticleId?.ToString()))
            {
                items = items.Where(item => item.ArticleId.ToString()?.Contains(searchModel.ArticleId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }
            /*   if (!string.IsNullOrWhiteSpace(searchModel.ParentCodeNavigationId?.ToString()))
               {
                   items = items.Where(item => item.ParentCodeNavigationId.ToString()?.Contains(searchModel.ParentCodeNavigationId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
               }*/

            // 处理 Searchable=true 列与 SeachText 模糊搜索

            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Text?.Contains(options.SearchText) ?? false)
                             || (item.ApplicationUserId?.Contains(options.SearchText) ?? false)
                             || (item.EntryId.ToString()?.Contains(options.SearchText) ?? false)
                             || (item.ArticleId.ToString()?.Contains(options.SearchText) ?? false)
                            /* || (item.ParentCodeNavigationId.ToString()?.Contains(options.SearchText) ?? false)*/);
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCache.GetOrAdd(typeof(Comment), key => LambdaExtensions.GetSortLambda<Comment>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListCommentAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListCommentAloneModel
                {
                    Id = item.Id,
                    Type = item.Type,
                    CommentTime = item.CommentTime,
                    Text = item.Text,
                    ApplicationUserId = item.ApplicationUserId,
                    ParentCodeNavigationId = item.ParentCodeNavigation?.Id,
                    EntryId = item.EntryId,
                    ArticleId = item.ArticleId,
                    UserSpaceCommentManagerId = item.UserSpaceCommentManagerId,
                    IsHidden = item.IsHidden,
                    Priority = item.Priority
                });
            }

            return new QueryData<ListCommentAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }

        public async Task UpdateCommentDataMainAsync(Comment comment, CommentText examine)
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
            Video video = null;
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
                    article = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).FirstOrDefaultAsync(s => s.Id == tempId);
                    if (article == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentEntries:
                    entry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (entry == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentPeriphery:
                    periphery = await _peripheryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (periphery == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentVote:
                    vote = await _voteRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (vote == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentLottery:
                    lottery = await _lotteryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (lottery == null)
                    {
                        return;
                    }
                    break;
                case CommentType.CommentVideo:
                    video = await _videoRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(examine.ObjectId));
                    if (video == null)
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
                    _userRepository.Clear();
                    break;
                case CommentType.ReplyComment:
                    replyComment = await _commentRepository.GetAll().AsNoTracking()
                        .Include(s => s.Article)
                        .Include(s => s.Entry)
                        //.Include(s=>s.ApplicationUser)
                        .Include(s => s.UserSpaceCommentManager)//.ThenInclude(s=>s.ApplicationUser)
                        .Include(s=>s.Lottery)
                        .Include(s=>s.Vote)
                        .Include(s => s.Video)
                        .Include(s=>s.Periphery)
                        .FirstOrDefaultAsync(s => s.Id == tempId);
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
                    comment.Article = article;
                    break;
                case CommentType.CommentEntries:
                    comment.EntryId = (int)tempId;
                    comment.Entry = entry;
                    break;
                case CommentType.CommentPeriphery:
                    comment.PeripheryId = tempId;
                    comment.Periphery = periphery;
                    break;
                case CommentType.CommentVote:
                    comment.VoteId = tempId;
                    comment.Vote = vote;
                    break;
                case CommentType.CommentLottery:
                    comment.LotteryId = tempId;
                    comment.Lottery = lottery;
                    break;
                case CommentType.CommentVideo:
                    comment.VideoId = tempId;
                    comment.Video = video;
                    break;
                case CommentType.CommentUser:
                    comment.UserSpaceCommentManager = userSpace;
                    comment.UserSpaceCommentManagerId = userSpace.Id;
                    break;
                case CommentType.ReplyComment:
                    //同步关联父对象的关联对象
                    comment.Article = replyComment.Article;
                    comment.ArticleId = replyComment.Article?.Id;
                    comment.Entry = replyComment.Entry;
                    comment.EntryId = replyComment.Entry?.Id;
                    comment.UserSpaceCommentManager = replyComment.UserSpaceCommentManager;
                    comment.UserSpaceCommentManagerId = replyComment.UserSpaceCommentManager?.Id;
                    comment.Lottery = replyComment.Lottery;
                    comment.LotteryId = replyComment.Lottery?.Id;
                    comment.Vote = replyComment.Vote;
                    comment.VoteId= replyComment.Vote?.Id;
                    comment.Periphery = replyComment.Periphery;
                    comment.PeripheryId = replyComment.Periphery?.Id;
                    comment.ParentCodeNavigation = replyComment;
                    break;
            }

        }

        public async Task UpdateCommentDataAsync(Comment comment, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.PubulishComment:
                    CommentText commentText = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        commentText = (CommentText)serializer.Deserialize(str, typeof(CommentText));
                    }

                   await UpdateCommentDataMainAsync(comment, commentText);
                    break;
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
                else if (_userService.CheckCurrentUserRole("Admin"))
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


    }


}
