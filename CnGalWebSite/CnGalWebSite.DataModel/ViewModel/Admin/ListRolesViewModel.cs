
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListRolesInforViewModel
    {
    }

    public class ListRolesViewModel
    {
        public List<ListRoleAloneModel> Roles { get; set; }
    }
    public class ListRoleAloneModel
    {
        [Display(Name = "Id")]
        public string Id { get; set; }
        [Display(Name = "角色名")]
        public string Name { get; set; }
    }

    public class RolesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListRoleAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
