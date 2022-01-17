
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListHistoryUsersInforViewModel
    {
        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListHistoryUsersViewModel
    {
        public List<ListHistoryUserAloneModel> HistoryUsers { get; set; }
    }
    public class ListHistoryUserAloneModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Display(Name = "用户的Id")]
        public string UserIdentity { get; set; }

        [Display(Name = "邮箱")]
        public string LoginName { get; set; }

        [Display(Name = "是否隐藏")]
        public bool UserName { get; set; }
    }

    public class HistoryUsersPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListHistoryUserAloneModel SearchModel { get; set; }

        public string ObjectId { get; set; }
    }
}
