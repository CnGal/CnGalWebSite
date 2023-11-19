using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Almanacs
{
    public class AlmanacEditModel : BaseEditModel
    {
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        public List<AlmanacEntryEditModel> Entries { get; set; } = new List<AlmanacEntryEditModel>() { };

        public List<AlmanacArticleEditModel> Articles { get; set; } = new List<AlmanacArticleEditModel> { };
    }

    public class AlmanacEntryEditModel
    {
        public long Id { get; set; }

        public int EntryId { get; set; }
        public string EntryName { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        public Result Validate(List<AlmanacEntryEditModel> items)
        {
            if (items.Where(s => s.Id != Id).Any(s => s.EntryName == EntryName))
            {
                return new Result { Error = $"已存在“{EntryName}”" };
            }
            if (string.IsNullOrWhiteSpace(EntryName))
            {
                return new Result { Error = $"名称不能为空" };
            }
            return new Result { Successful = true };

        }
    }

    public class AlmanacArticleEditModel
    {
        public long Id { get; set; }

        public long ArticleId { get; set; }
        public string ArticleName { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        public Result Validate(List<AlmanacArticleEditModel> items)
        {
            if (items.Where(s => s.Id != Id).Any(s => s.ArticleName == ArticleName))
            {
                return new Result { Error = $"已存在“{ArticleName}”" };
            }

            if (string.IsNullOrWhiteSpace(ArticleName))
            {
                return new Result { Error = $"名称不能为空" };
            }

            return new Result { Successful = true };

        }
    }
}
