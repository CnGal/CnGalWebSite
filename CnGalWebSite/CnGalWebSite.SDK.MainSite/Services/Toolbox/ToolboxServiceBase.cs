using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.SDK.MainSite.Models.Toolbox;

namespace CnGalWebSite.SDK.MainSite.Services.Toolbox;

public abstract class ToolboxServiceBase
{
    public event Action<ToolboxProgressUpdate>? ProgressUpdate;

    protected void Report(ToolboxTaskBase model, ToolboxOutputLevel level, string message)
    {
        if (level == ToolboxOutputLevel.Error)
        {
            model.Error = message;
        }

        ProgressUpdate?.Invoke(new ToolboxProgressUpdate(level, message));
    }

    protected static List<RelevancesModel> BuildGameRelevances(string title, string context, IEnumerable<string> games)
    {
        var ignored = new HashSet<string>(StringComparer.Ordinal)
        {
            "幻觉",
            "画师",
            "她",
            "人间"
        };

        var result = new List<RelevancesModel>();
        foreach (var game in games.Where(s => !string.IsNullOrWhiteSpace(s) && !ignored.Contains(s)).Distinct())
        {
            if (context.Contains(game, StringComparison.Ordinal) || title.Contains(game, StringComparison.Ordinal))
            {
                result.Add(new RelevancesModel { DisplayName = game });
            }
        }

        return result
            .GroupBy(s => s.DisplayName, StringComparer.Ordinal)
            .Select(s => s.First())
            .ToList();
    }
}
