using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Roles.Dtos;
using Microsoft.AspNetCore.Identity;

namespace CnGalWebSite.DataModel.Application.Roles
{
    public class RoleService : IRoleService
    {
        public PagedResultDto<IdentityRole> GetPaginatedResult(GetRoleInput input, List<IdentityRole> examines)
        {
            IEnumerable<IdentityRole> query = examines;

            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                query = query.Where(s => s.Name.Contains(input.FilterText));
            }
            //统计查询数据的总条数
            var count = examines.Count;
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<IdentityRole> models = null;
            if (count != 0)
            {
                models = query.ToList();
            }
            else
            {
                models = new List<IdentityRole>();
            }


            var dtos = new PagedResultDto<IdentityRole>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };

            return dtos;
        }
    }
}
