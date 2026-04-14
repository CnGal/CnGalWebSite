using System.Collections.Generic;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Geetest
{
    /// <summary>
    /// 极验配置选项
    /// </summary>
    public class GeetestOptions
    {
        public const string SectionName = "Geetest";

        /// <summary>
        /// 默认凭据，未配置场景时使用
        /// </summary>
        public GeetestCredential Default { get; set; } = new();

        /// <summary>
        /// 按场景配置的凭据
        /// </summary>
        public Dictionary<string, GeetestCredential> Scenarios { get; set; } = new();

        /// <summary>
        /// 根据场景获取凭据，未配置则回落到 Default
        /// </summary>
        public GeetestCredential GetCredential(GeetestScenario scenario)
        {
            var key = scenario.ToString();
            if (Scenarios.TryGetValue(key, out var credential) && !string.IsNullOrEmpty(credential.Id))
            {
                return credential;
            }
            return Default;
        }
    }

    /// <summary>
    /// 极验凭据
    /// </summary>
    public class GeetestCredential
    {
        public string Id { get; set; }
        public string Key { get; set; }
    }
}
