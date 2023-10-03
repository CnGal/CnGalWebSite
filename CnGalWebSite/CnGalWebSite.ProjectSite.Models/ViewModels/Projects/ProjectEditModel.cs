using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.Models.Base;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Projects
{
    public class ProjectEditModel : BaseEditModel
    {
        /// <summary>
        /// 企划详情
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// 预算区间
        /// </summary>
        public string BudgetRange { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 预览图
        /// </summary>
        public List<ProjectImageEditModel> Images { get; set; } = new List<ProjectImageEditModel>();

        /// <summary>
        /// 需求职位
        /// </summary>
        public List<ProjectPositionEditModel> Positions { get; set; } = new List<ProjectPositionEditModel>();
        /// <summary>
        /// 创建者
        /// </summary>
        public UserInfoViewModel CreateUser { get; set; } = new UserInfoViewModel();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Success = false, Message = "请填写企划名称" };
            }
            if (string.IsNullOrWhiteSpace(Contact))
            {
                return new Result { Success = false, Message = "请填写联系方式" };
            }
            if (string.IsNullOrWhiteSpace(Description) || Description.Length < 30)
            {
                return new Result { Success = false, Message = "请填写企划详情，并不少于30个字" };
            }

            if (EndTime <= DateTime.Now)
            {
                return new Result { Success = false, Message = "截止日期必须大于当前日期" };
            }

            foreach (var item in Positions)
            {
                var result = item.Validate();
                if (!result.Success)
                {
                    return result;
                }
            }

            return new Result { Success = true };
        }
    }

    public class ProjectImageEditModel : BaseImageEditModel
    {
        public long Id { get; set; }
    }
    public class ProjectPositionEditModel : ProjectPositionViewModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }

        public Result Validate()
        {
            if (BudgetType == DataModels.BudgetType.Afadian)
            {
                BudgetMax = BudgetMin = Percentage = 0;
            }

            else if (BudgetType == DataModels.BudgetType.IntervalAndDivide || BudgetType == DataModels.BudgetType.Interval)
            {
                if (BudgetMax < BudgetMin)
                {
                    return new Result { Success = false, Message = "预算下限必须小于等于上限" };
                }

                if (BudgetMin <= 0)
                {
                    return new Result { Success = false, Message = "预算必须大于0" };
                }
            }
            else if (BudgetType == DataModels.BudgetType.IntervalAndDivide || BudgetType == DataModels.BudgetType.Divide)
            {
                if (Percentage > 100)
                {
                    return new Result { Success = false, Message = "分成比例必须小于100%" };
                }
                if (Percentage <= 0)
                {
                    return new Result { Success = false, Message = "分成比例必须大于0" };
                }
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                return new Result { Success = false, Message = "请填写约稿需求详情" };
            }

            if (PositionType == DataModels.ProjectPositionType.Other && string.IsNullOrWhiteSpace(PositionTypeName))
            {
                return new Result { Success = false, Message = "选择其他类型后请填写类型" };
            }

            if (DeadLine <= DateTime.Now)
            {
                return new Result { Success = false, Message = "截止日期必须大于当前日期" };
            }

            return new Result
            {
                Success = true,
            };
        }
    }
}
