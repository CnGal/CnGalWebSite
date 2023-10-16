using CnGalWebSite.ProjectSite.Models.Base;
using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.Core.Models;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Comments
{
    public class CommentEditModel : BaseEditModel
    {
        public CommentType Type { get; set; }

        public string Text { get; set; }

        public long ObjectId { get; set; }

        public PageType PageType { get; set; }

        public long PageId { get; set; }

        public override Result Validate()
        {
            if(string.IsNullOrWhiteSpace(Text))
            {
                return new Result { Success = false, Message = "留言不能为空" };
            }

            return new Result
            {
                Success = true,
            };
        }
    }
}
