using System.Text.RegularExpressions;
using CnGalWebSite.RobotClientX.Services;

namespace CnGalWebSite.RobotClientX.Extentions
{
    public static class MessageExtension
    {
        private static readonly Regex AtPattern = new(@"\[@(\d+)\]", RegexOptions.Compiled);

        public static string ReplaceAtTags(this string message, long groupNumber, IQQGroupMemberCacheService memberCacheService, long selfQQ, string selfName)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return message;
            }

            return AtPattern.Replace(message, match =>
            {
                if (match.Groups.Count < 2)
                {
                    return match.Value;
                }

                if (!long.TryParse(match.Groups[1].Value, out var qqNumber))
                {
                    return match.Value;
                }
                var nickName = memberCacheService.GetMemberNickName(groupNumber, qqNumber);
                if (qqNumber == selfQQ)
                {
                    nickName = selfName;
                }


                return string.IsNullOrWhiteSpace(nickName) ? match.Value : $" @{nickName} ";
            });
        }
    }
}
