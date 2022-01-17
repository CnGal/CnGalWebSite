using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class UserSpaceCommentManager
    {
        public long Id { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
