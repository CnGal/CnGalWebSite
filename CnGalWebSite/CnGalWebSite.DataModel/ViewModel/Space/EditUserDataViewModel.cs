using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class EditUserDataViewModel
    {
        public string Id { get; set; }

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


        public string PhotoPath { get; set; }

        public string BackgroundPath { get; set; }
        public string BackgroundName { get; set; }

        public string MBgImagePath { get; set; }
        public string MBgImageName { get; set; }

        public string SBgImagePath { get; set; }
        public string SBgImageName { get; set; }


        [Display(Name = "是否开启空间留言")]
        public bool CanComment { get; set; }
        [Display(Name = "是否公开收藏夹")]
        public bool IsShowFavorites { get; set; }

        [Display(Name = "SteamId")]
        public string SteamId { get; set; }

        public string MicrosoftAccountName { get; set; }

        public string GithubAccountName { get; set; }

        public string GiteeAccountName { get; set; }

        public string QQAccountName { get; set; }

        public List<UserEditRankIsShow> Ranks { get; set; }

    }

    public class UserEditRankIsShow
    {
        public string Name { get; set; }

        public bool IsShow { get; set; }

    }
}
