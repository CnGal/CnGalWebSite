using System;

namespace CnGalWebSite.DataModel.ImportModel
{
    public class ImportUserModel
    {
        /// <summary>
        /// 充当Id 对应导入用户的Id 即id
        /// </summary>
        public string UserIdentity { get; set; }
        /// <summary>
        /// 加密字符串
        /// </summary>
        public string PasswordSalt { get; set; }
        /// <summary>
        /// 加密后的密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 登入名 第一次登入后删除
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 对应导入用户的昵称 即nickname
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户主页
        /// </summary>
        public string MainPageContext { get; set; } = "### 哇，这里什么都没有呢";
        /// <summary>
        /// 个性签名
        /// </summary>
        public string PersonalSignature { get; set; } = "哇，这里什么都没有呢";
        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime RegistTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 头像 只写文件名 图片手动上传到服务器
        /// </summary>
        public string PhotoPath { get; set; }
        /// <summary>
        /// 用户空间头图 只写文件名 图片手动上传到服务器
        /// </summary>
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
        /// 封禁解封时间 null代表未封禁
        /// </summary>
        public DateTime? UnsealTime { get; set; } = null;
        /// <summary>
        /// 是否可以留言
        /// </summary>
        public bool CanComment { get; set; } = true;
    }
}
