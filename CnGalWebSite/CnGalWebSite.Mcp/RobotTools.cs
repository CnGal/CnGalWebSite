using System.ComponentModel;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.SDK.MainSite.Abstractions;
using ModelContextProtocol.Server;

namespace CnGalWebSite.Mcp;

[McpServerToolType]
public sealed class RobotTools(IRobotQueryService queries)
{
    [McpServerTool(Name = "search_cngal", ReadOnly = true), Description("名称不明确时先搜索 CnGal 公开词条和文章。返回候选名称、类型、ID 和来源；多个候选须列出名称与链接请用户确认，单个部分匹配也不等于精确匹配，不猜测最短名称。确认词条或文章 ID 后使用对应详情工具。")]
    public Task<RobotQueryResult> Search(string text, int current_page = 1, CancellationToken cancellationToken = default) => queries.SearchAsync(text, current_page, cancellationToken);
    [McpServerTool(Name = "get_entry", ReadOnly = true), Description("已知词条 ID 时获取公开词条简介和链接；用户仅给出数字 ID 时默认查询词条。CnGal 的 /entries/ID 或 /entries/index/ID 链接可直接提取 ID 调用，不读取任意外部链接。隐藏或不存在的词条均返回空结果；只要链接时保留返回的来源链接，不额外编造介绍。")]
    public Task<RobotQueryResult> Entry(int id, CancellationToken cancellationToken = default) => queries.GetEntryAsync(id, cancellationToken);
    [McpServerTool(Name = "get_article", ReadOnly = true), Description("已知文章 ID 时获取公开文章简介和链接；CnGal 的 /articles/ID 或 /articles/index/ID 链接可直接提取 ID 调用，不读取任意外部链接。隐藏或不存在的文章均返回空结果。")]
    public Task<RobotQueryResult> Article(long id, CancellationToken cancellationToken = default) => queries.GetArticleAsync(id, cancellationToken);
    [McpServerTool(Name = "get_latest_games", ReadOnly = true), Description("查询近期发布正式版的游戏及来源；体验版使用 get_latest_demo_games，即将发售使用 get_upcoming_games。")]
    public Task<RobotQueryResult> Latest(CancellationToken cancellationToken = default) => queries.GetLatestGamesAsync(cancellationToken);
    [McpServerTool(Name = "get_latest_demo_games", ReadOnly = true), Description("查询近期发布体验版的游戏及来源，不代表正式版已经发布。")]
    public Task<RobotQueryResult> Demo(CancellationToken cancellationToken = default) => queries.GetLatestDemoGamesAsync(cancellationToken);
    [McpServerTool(Name = "get_upcoming_games", ReadOnly = true), Description("查询即将发售的游戏及来源，返回已有的发售日期与简介。")]
    public Task<RobotQueryResult> Upcoming(CancellationToken cancellationToken = default) => queries.GetUpcomingGamesAsync(cancellationToken);
    [McpServerTool(Name = "get_news", ReadOnly = true), Description("查询 CnGal 新闻、最新动态及来源，也是周报或速报请求的查询入口。")]
    public Task<RobotQueryResult> News(CancellationToken cancellationToken = default) => queries.GetNewsAsync(cancellationToken);
    [McpServerTool(Name = "get_random_game", ReadOnly = true), Description("随机推荐一款公开游戏并提供来源；每次调用结果可能不同，空结果表示当前未找到可推荐的公开内容。")]
    public Task<RobotQueryResult> Random(CancellationToken cancellationToken = default) => queries.GetRandomGameAsync(cancellationToken);
    [McpServerTool(Name = "get_role_birthdays", ReadOnly = true), Description("查询公开角色生日及来源。month/day 必须同时提供；都省略时查询上海时区当日。空结果表示当日未找到符合条件的公开角色。")]
    public Task<RobotQueryResult> Birthdays(int? month = null, int? day = null, CancellationToken cancellationToken = default) => queries.GetRoleBirthdaysAsync(month, day, cancellationToken);
    [McpServerTool(Name = "get_recently_updated_games", ReadOnly = true), Description("查询最近编辑更新的游戏词条及来源；更新时间不是发售日期，不把编辑更新当成新作发布。")]
    public Task<RobotQueryResult> Updated(CancellationToken cancellationToken = default) => queries.GetRecentlyUpdatedGamesAsync(cancellationToken);
    [McpServerTool(Name = "get_discount_summary", ReadOnly = true), Description("查询当前 Steam 折扣游戏数量及折扣页链接；数量为零也是有效结果，请求失败不等于零。不提供逐游戏价格或折扣幅度。")]
    public Task<RobotDiscountSummary> Discounts(CancellationToken cancellationToken = default) => queries.GetDiscountSummaryAsync(cancellationToken);
}
