namespace CnGalWebSite.DataModel.ImportModel
{
    public class ImportUserModel
    {
        public string PasswordSalt { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }
        /// <summary>
        /// 充当Id 对应导入用户的Id
        /// </summary>
        public string UserIdentity { get; set; }

        public string LoginName { get; set; }
        /// <summary>
        /// 对应导入用户的昵称
        /// </summary>
        public string UserName { get; set; }

        public string MainPageContext { get; set; }

        public string PersonalSignature { get; set; }

        public DateTime? Birthday { get; set; }

        public DateTime RegistTime { get; set; }

        public string PhotoPath { get; set; }

        public string BackgroundImage { get; set; }
        /// <summary>
        /// 积分
        /// </summary>
        public int Integral { get; set; } = 0;
        /// <summary>
        /// 贡献值
        /// </summary>
        public int ContributionValue { get; set; } = 0;
        /// <summary>
        /// 在线时间 单位 秒
        /// </summary>
        public long OnlineTime { get; set; } = 0;
        /// <summary>
        /// 最后在线时间
        /// </summary>
        public DateTime LastOnlineTime { get; set; }
        /// <summary>
        /// 解封时间
        /// </summary>
        public DateTime? UnsealTime { get; set; } = null;
        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool CanComment { get; set; } = true;


    }
}
