namespace CnGalWebSite.Extensions;

/// <summary>
/// 用户等级的公共规则，供服务端头衔和前端资料卡共用。
/// </summary>
public static class UserLevelHelper
{
    public const int MaxLevel = 9;

    // 下标为当前等级，值为升到下一级所需的累计积分。等级从 0 开始。
    private static readonly int[] NextLevelIntegrals = { 5, 50, 200, 500, 1000, 2000, 3500, 6000, 10000 };

    /// <summary>
    /// 升到下一级所需的累计积分，满级时没有下一级，返回 null。
    /// </summary>
    public static int? GetNextLevelIntegral(int level)
    {
        if (level < 0 || level > MaxLevel)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        return level == MaxLevel ? null : NextLevelIntegrals[level];
    }

    public static int GetUserLevel(int integral)
    {
        for (var level = 0; level < MaxLevel; level++)
        {
            if (integral < NextLevelIntegrals[level])
            {
                return level;
            }
        }

        return MaxLevel;
    }

    /// <summary>
    /// 当前累计积分占下一级累计积分门槛的百分比，满级为 100%。
    /// </summary>
    public static double GetLevelProgress(int integral)
    {
        var level = GetUserLevel(integral);
        return level == MaxLevel ? 100 : Math.Clamp(100.0 * integral / NextLevelIntegrals[level], 0, 100);
    }
}
