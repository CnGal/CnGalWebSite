using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class EditUserDataViewModel : BaseEditModel
    {
        public new string Id { get; set; }

        [Required(ErrorMessage = "请输入名字"), MaxLength(20, ErrorMessage = "名字的长度不能超过20个字符")]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        [Display(Name = "个性签名")]
        public string PersonalSignature { get; set; }

        [Display(Name = "生日")]
        public DateTime? Birthday { get; set; }

        [Display(Name = "头像")]
        public string PhotoName { get; set; }

        public string BackgroundName { get; set; }

        public string MBgImageName { get; set; }

        public string SBgImageName { get; set; }


        [Display(Name = "是否开启空间留言")]
        public bool CanComment { get; set; }
        [Display(Name = "是否公开收藏夹")]
        public bool IsShowFavorites { get; set; }
        [Display(Name = "是否公开游玩记录")]
        public bool IsShowGameRecord { get; set; }

        [Display(Name = "SteamID64（纯数字，可用逗号分隔多个 ID）")]
        public List<string> SteamIds { get; set; }



        /// <summary>
        /// 上次修改密码时间
        /// </summary>
        public DateTime? LastChangePasswordTime { get; set; }
        /// <summary>
        /// 绑定的群聊QQ号
        /// </summary>
        public string GroupQQ { get; set; }
        /// <summary>
        /// 登入QQ昵称
        /// </summary>
        public string QQAccountName { get; set; }
        /// <summary>
        /// 认证的词条
        /// </summary>
        public EditUserCertificationModel UserCertificationModel { get; set; }

        public List<UserEditRankIsShow> Ranks { get; set; }

        public bool TryNormalizeSteamIds(out List<string> normalized)
        {
            normalized = new List<string>();
            if (SteamIds == null)
                return false;

            foreach (var item in SteamIds)
            {
                if (item == null || item.Any(char.IsControl))
                    return false;

                foreach (var part in item.Split(new[] { ',', '，', '、' }))
                {
                    var id = part.Trim();
                    if (id.Any(c => c < '0' || c > '9') ||
                        !ulong.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value == 0)
                        return false;
                    normalized.Add(id);
                }
            }
            return true;
        }

        public override Result Validate()
        {

            if (string.IsNullOrWhiteSpace(UserName))
            {
                return new Result { Error = "请输入用户名" };

            }
            if (UserName.Length > 20)
            {
                return new Result { Error = "用户名必须少于20个字符" };
            }
            //处理头衔
            if (Ranks.Any(s => s.IsShow) == false)
            {
                return new Result { Error = "至少展示一个头衔" };
            }
            //处理时间
            if (Birthday != null)
            {
                Birthday = Birthday.Value.AddHours(Birthday.Value.Hour < 12 ? (12 - Birthday.Value.Hour) : 0);
            }

            return new Result { Successful = true };
        }

    }

    public class UserEditRankIsShow
    {
        public string Name { get; set; }

        public bool IsShow { get; set; }

    }


}
