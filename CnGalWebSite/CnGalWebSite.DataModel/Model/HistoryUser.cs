using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class HistoryUser
    {
        public int Id { get; set; }

        public string PasswordSalt { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }
        /// <summary>
        /// 充当Id 对应导入用户的Id
        /// </summary>
        public string UserIdentity { get; set; }
        /// <summary>
        /// 只是用来查询 用户登入后会废除这个字段
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 对应导入用户的昵称
        /// </summary>
        public string UserName { get; set; }
    }
}
