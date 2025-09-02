using System.Collections.Generic;

namespace CnGalWebSite.Kanban.ChatGPT.Models.DeepSeek
{
    public class DeepSeekBalanceResponse
    {
        public List<BalanceInfo>? Balance_infos { get; set; }
    }

    public class BalanceInfo
    {
        public string? Currency { get; set; }
        public string? Total_balance { get; set; }
        public string? Granted_balance { get; set; }
        public string? Topped_up_balance { get; set; }
    }
}
