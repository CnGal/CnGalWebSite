using System.Net;
using CnGalWebSite.Extensions;
using CnGalWebSite.MainSite.Shared.Components.Features.Space;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class UserLevelTests
{
    [TestCase(int.MinValue, 0, 5)]
    [TestCase(0, 0, 5)]
    [TestCase(4, 0, 5)]
    [TestCase(5, 1, 50)]
    [TestCase(49, 1, 50)]
    [TestCase(50, 2, 200)]
    [TestCase(199, 2, 200)]
    [TestCase(200, 3, 500)]
    [TestCase(499, 3, 500)]
    [TestCase(500, 4, 1000)]
    [TestCase(999, 4, 1000)]
    [TestCase(1000, 5, 2000)]
    [TestCase(1999, 5, 2000)]
    [TestCase(2000, 6, 3500)]
    [TestCase(3499, 6, 3500)]
    [TestCase(3500, 7, 6000)]
    [TestCase(5990, 7, 6000)]
    [TestCase(5999, 7, 6000)]
    [TestCase(6000, 8, 10000)]
    [TestCase(9999, 8, 10000)]
    [TestCase(10000, 9, null)]
    [TestCase(99999999, 9, null)]
    [TestCase(int.MaxValue, 9, null)]
    public void LevelBoundariesUseCanonicalThresholds(int integral, int level, int? nextIntegral)
    {
        Assert.Multiple(() =>
        {
            Assert.That(UserLevelHelper.GetUserLevel(integral), Is.EqualTo(level));
            Assert.That(UserLevelHelper.GetNextLevelIntegral(level), Is.EqualTo(nextIntegral));
        });
    }

    [TestCase(int.MinValue, 0)]
    [TestCase(0, 0)]
    [TestCase(4, 80)]
    [TestCase(5, 10)]
    [TestCase(5990, 99.83333333333333)]
    [TestCase(6000, 60)]
    [TestCase(10000, 100)]
    [TestCase(int.MaxValue, 100)]
    public void ProgressUsesTotalIntegralAndStopsAtMaxLevel(int integral, double progress)
    {
        Assert.That(UserLevelHelper.GetLevelProgress(integral), Is.EqualTo(progress).Within(0.000001));
    }

    [TestCase(int.MinValue)]
    [TestCase(-1)]
    [TestCase(10)]
    [TestCase(int.MaxValue)]
    public void NextLevelIntegralRejectsInvalidLevels(int level)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UserLevelHelper.GetNextLevelIntegral(level));
    }

    [TestCase(0, 0, "0 / 5", "0")]
    [TestCase(5, 1, "5 / 50", "10")]
    [TestCase(5990, 7, "5990 / 6000", "99.83")]
    [TestCase(6000, 8, "6000 / 10000", "60")]
    [TestCase(10000, 9, "10000", "100")]
    [TestCase(int.MaxValue, 9, "2147483647", "100")]
    [SetCulture("fr-FR")]
    public async Task ProfileRendersSharedLevelAndValidCssInAnyCulture(int integral, int level, string text, string progress)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<SpaceDataOverviewCard>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = CreateModel(integral) }));
            return WebUtility.HtmlDecode(component.ToHtmlString());
        });

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain($"Lv：{level}"));
            Assert.That(html, Does.Contain($">{text}</span>"));
            Assert.That(html, Does.Contain($"style=\"width: {progress}%\""));
            if (level == UserLevelHelper.MaxLevel)
                Assert.That(html, Does.Not.Contain(" / "));
        });
    }

    private static SpaceDetailViewModel CreateModel(int integral) => new()
    {
        Id = "user",
        Name = "User",
        PersonalSignature = "",
        BackgroundImage = "",
        MBgImage = "",
        SBgImage = "",
        PhotoPath = "",
        MainPageHtml = "",
        Integral = integral,
        GCoins = 0,
        ContributionValue = 0,
        EditCount = 1,
        ArticleCount = 2,
        VideoCount = 0,
        SignInDays = 0,
        Birthday = null,
        RegisteTime = default,
        LastOnlineTime = default,
        OnlineTime = 0,
        Ranks = [],
        UserCertification = null,
        IsCurrentUser = false,
        SteamId = "",
        CanComment = false,
        IsShowFavorites = false,
        IsShowGameRecord = false
    };
}
