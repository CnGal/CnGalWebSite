using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Examines.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Linq.Dynamic.Core;

namespace CnGalWebSite.DataModel.Application.Examines
{
    public class ExamineService : IExamineService
    {
        public enum GetExaminePagedType
        {
            Entry,
            User
        }

        public PagedResultDto<ExaminedNormalListModel> GetPaginatedResult(GetExamineInput input, List<ExaminedNormalListModel> examines, GetExaminePagedType type)
        {
            var query = examines.AsQueryable();
            if (type == GetExaminePagedType.Entry)
            {
                query = query.Where(s => s.IsPassed == true);
            }
            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "待审核":
                        query = query.Where(s => s.IsPassed == null);
                        break;
                    case "已通过":
                        query = query.Where(s => s.IsPassed == true);
                        break;
                    case "未通过":
                        query = query.Where(s => s.IsPassed == false);
                        break;

                }
            }
            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                //尝试将查询翻译成操作
                var operation = Operation.None;
                switch (input.FilterText)
                {
                    case "修改用户主页":
                        operation = Operation.UserMainPage;
                        break;
                    case "编辑词条主要信息":
                        operation = Operation.EstablishMain;
                        break;
                    case "编辑词条附加信息":
                        operation = Operation.EstablishAddInfor;
                        break;
                    case "编辑词条主页":
                        operation = Operation.EstablishMainPage;
                        break;
                    case "编辑词条图片":
                        operation = Operation.EstablishImages;
                        break;
                    case "编辑词条相关链接":
                        operation = Operation.EstablishRelevances;
                        break;
                    case "编辑词条标签":
                        operation = Operation.EstablishTags;
                        break;
                    case "编辑文章主要信息":
                        operation = Operation.EditArticleMain;
                        break;
                    case "编辑文章关联词条":
                        operation = Operation.EditArticleRelevanes;
                        break;
                    case "编辑文章内容":
                        operation = Operation.EditArticleMainPage;
                        break;
                }
                query = query.Where(s => s.UserName.Contains(input.FilterText)
                  || s.Operation == operation);
            }
            //统计查询数据的总条数
            var count = examines.Count;
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<ExaminedNormalListModel> models = null;
            if (count != 0)
            {
                models = query.ToList();
            }
            else
            {
                models = new List<ExaminedNormalListModel>();
            }


            var dtos = new PagedResultDto<ExaminedNormalListModel>
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
