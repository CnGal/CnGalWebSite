using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class Comment : BaseModel
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public CommentType Type { get; set; }

        public long ObjectId { get; set; }

        public PageType PageType { get; set; }

        public long PageId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

    public enum CommentType
    {
        PositionUser,
        Comment
    }

    public enum PageType
    {
        Project
    }
}
