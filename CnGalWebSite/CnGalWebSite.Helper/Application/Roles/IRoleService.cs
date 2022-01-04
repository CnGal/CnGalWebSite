using Microsoft.AspNetCore.Identity;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Roles.Dtos;

namespace CnGalWebSite.DataModel.Application.Roles
{
    public interface IRoleService
    {
        public PagedResultDto<IdentityRole> GetPaginatedResult(GetRoleInput input, List<IdentityRole> examines);

    }
}
