using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.ImportModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tag = CnGalWebSite.DataModel.Model.Tag;

namespace CnGalWebSite.APIServer.ExamineX
{
    public interface IExamineService
    {
        Task<PagedResultDto<ExaminedNormalListModel>> GetPaginatedResult(GetExamineInput input, int entryId = 0, string userId = "");

        Task<QueryData<ListExamineAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListExamineAloneModel searchModel);

        /// <summary>
        /// 将审核列表优化成精简模式以减少流量消耗
        /// </summary>
        /// <param name="examines">数据库审核列表模型</param>
        /// <param name="isShowRanks">是否展示头衔</param>
        /// <returns></returns>
        Task<List<ExaminedNormalListModel>> GetExaminesToNormalListAsync(IQueryable<Examine> examines, bool isShowRanks);

        Task<bool> GetExamineView(Models.ExaminedViewModel model, Examine examine);
        /// <summary>
        /// 处理 EstablishMain 审核成功后调用更新数据
        /// </summary>
        /// <param name="entry">关联词条</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEstablishMainAsync(Entry entry, EntryMain examine);
        /// <summary>
        /// 处理 EstablishAddInfor 审核成功后调用更新数据
        /// </summary>
        /// <param name="entry">关联词条</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEstablishAddInforAsync(Entry entry, EntryAddInfor examine);
        /// <summary>
        /// 处理 EstablishImages 审核成功后调用更新数据
        /// </summary>
        /// <param name="entry">关联词条</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEstablishImagesAsync(Entry entry, EntryImages examine);
        /// <summary>
        /// 处理 EstablishRelevances 审核成功后调用更新数据
        /// </summary>
        /// <param name="entry">关联词条</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEstablishRelevancesAsync(Entry entry, EntryRelevances examine);
        /// <summary>
        /// 处理 EstablishTags 审核成功后调用更新数据
        /// </summary>
        /// <param name="entry">关联词条</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEstablishTagsAsync(Entry entry, EntryTags examine);
        /// <summary>
        /// 处理 EstablishMainPage 审核成功后调用更新数据
        /// </summary>
        /// <param name="entry">关联词条</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEstablishMainPageAsync(Entry entry, string examine);
        /// <summary>
        /// 获取当前用户对该词条部分的待审核记录
        /// </summary>
        /// <param name="entryId">词条Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>审核记录</returns>
        Task<Examine> GetUserEntryActiveExamineAsync(int entryId, string userId, Operation operation);
        /// <summary>
        /// 词条 编辑 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="entry">词条</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task UniversalEditExaminedAsync(Entry entry, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 词条 创建 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="entry">词条</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<bool> UniversalEstablishExaminedAsync(Entry entry, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 为批量导入的词条建立审核记录
        /// </summary>
        /// <param name="model">词条</param>
        /// <param name="user">管理员用户</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task AddBatchEtryExaminesAsync(Entry model, ApplicationUser user, string note);

        /// <summary>
        /// 获取当前用户对该 文章 部分的待审核记录
        /// </summary>
        /// <param name="articleId">文章Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>审核记录</returns>
        Task<Examine> GetUserArticleActiveExamineAsync(long articleId, string userId, Operation operation);
        /// <summary>
        /// 处理 EditArticleMain 审核成功后调用更新数据
        /// </summary>
        /// <param name="article">关联文章</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditArticleMainAsync(Article article, ArticleMain examine);
        /// <summary>
        /// 文章 创建 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="article">文章</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<bool> UniversalCreateArticleExaminedAsync(Article article, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 文章 编辑 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="article">文章</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task UniversalEditArticleExaminedAsync(Article article, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 处理 EditArticleRelevanes 审核成功后调用更新数据
        /// </summary>
        /// <param name="article">关联文章</param>
        /// <param name="articleRelecancesModel">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditArticleRelevancesAsync(Article article, ArticleRelevances examine);
        /// <summary>
        /// 处理 EditArticleMainPage 审核成功后调用更新数据
        /// </summary>
        /// <param name="article">关联文章</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditArticleMainPageAsync(Article article, string examine);

        /// <summary>
        /// 为批量导入的文章建立审核记录
        /// </summary>
        /// <param name="model">文章</param>
        /// <param name="user">用户</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task AddBatchArticleExaminesAsync(Article model, ApplicationUser user, string note);
        /// <summary>
        /// 为批量导入的周边建立审核记录
        /// </summary>
        /// <param name="model">周边</param>
        /// <param name="user">用户</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<string> AddBatchPeeripheryExaminesAsync(ImportPeripheryModel model, ApplicationUser user, string note);

        /// <summary>
        /// 获取当前用户 标签 等待审核
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="userId"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task<Examine> GetUserTagActiveExamineAsync(int tagId, string userId, Operation operation);
        /// <summary>
        /// 处理 EditTag 审核成功后调用更新数据
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="tagEdit"></param>
        /// <returns></returns>
        Task ExamineTagAsync(DataModel.Model.Tag tag, TagEdit tagEdit);

        /// <summary>
        /// 标签 编辑 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task UniversalEditTagExaminedAsync(DataModel.Model.Tag tag, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 处理 DisambigMain 审核成功后调用更新数据
        /// </summary>
        /// <param name="disambig">关联消歧义页</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditDisambigMainAsync(Disambig disambig, DisambigMain examine);
        /// <summary>
        /// 消歧义页 创建 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="disambig">消歧义页</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<bool> UniversalCreateDisambigExaminedAsync(Disambig disambig, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 处理 DisambigRelevanes 审核成功后调用更新数据
        /// </summary>
        /// <param name="disambig">关联消歧义页</param>
        /// <param name="disambigRelevances">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditDisambigRelevancesAsync(Disambig disambig, DisambigRelevances disambigRelevances);
        /// <summary>
        /// 消歧义页 编辑 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="disambig">消歧义页</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task UniversalEditDisambigExaminedAsync(Disambig disambig, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 处理 EditUserMain 审核成功后调用更新数据
        /// </summary>
        /// <param name="user">关联用户</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditUserMainAsync(ApplicationUser user, UserMain examine);
        /// <summary>
        /// 处理 UserMainPage 审核成功后调用更新数据
        /// </summary>
        /// <param name="user">关联用户</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditUserMainPageAsync(ApplicationUser user, string examine);
        /// <summary>
        /// 用户信息 编辑 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task UniversalEditUserExaminedAsync(ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 获取当前用户对该 用户信息 部分的待审核记录
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>审核记录</returns>
        Task<Examine> GetUserInforActiveExamineAsync(string userId, Operation operation);

        /// <summary>
        /// 处理 PeripheryMain 审核成功后调用更新数据
        /// </summary>
        /// <param name="periphery">关联周边</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditPeripheryMainAsync(Periphery periphery, PeripheryMain examine);
        /// <summary>
        /// 处理 PeripheryImages 审核成功后调用更新数据
        /// </summary>
        /// <param name="periphery">关联周边</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditPeripheryImagesAsync(Periphery periphery, PeripheryImages examine);
        /// <summary>
        /// 周边 创建 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="periphery">周边</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<bool> UniversalCreatePeripheryExaminedAsync(Periphery periphery, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 周边 编辑 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="periphery">周边</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task UniversalEditPeripheryExaminedAsync(Periphery periphery, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 获取当前用户 周边 等待审核
        /// </summary>
        /// <param name="peripheryId"></param>
        /// <param name="userId"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task<Examine> GetUserPeripheryActiveExamineAsync(long peripheryId, string userId, Operation operation);
        /// <summary>
        /// 处理 PeripheryRelatedEntries 审核成功后调用更新数据
        /// </summary>
        /// <param name="periphery">关联周边</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditPeripheryRelatedEntriesAsync(Periphery periphery, PeripheryRelatedEntries examine);
        /// <summary>
        /// 处理 TagMain 审核成功后调用更新数据
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditTagMainAsync(Tag tag, TagMain examine);
        /// <summary>
        /// 处理 TagChildTags 审核成功后调用更新数据
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditTagChildTagsAsync(Tag tag, TagChildTags examine);
        /// <summary>
        /// 处理 TagChildEntries 审核成功后调用更新数据
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditTagChildEntriesAsync(Tag tag, TagChildEntries examine);
        /// <summary>
        /// 标签 创建 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<bool> UniversalCreateTagExaminedAsync(Tag tag, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 迁移 EditEntryTags 类型审核记录到新版
        /// </summary>
        /// <returns></returns>
        Task MigrationEditEntryTagsExamineRecord();
        Task MigrationEditArticleRelevanceExamineRecord();
        Task MigrationEditEntryRelevanceExamineRecord();


        /// <summary>
        /// 处理 PeripheryRelatedPeripheries 审核成功后调用更新数据
        /// </summary>
        /// <param name="periphery">关联周边</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditPeripheryRelatedPeripheriesAsync(Periphery periphery, PeripheryRelatedPeripheries examine);


    }
}
