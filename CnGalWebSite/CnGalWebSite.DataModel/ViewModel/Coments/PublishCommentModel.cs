using CnGalWebSite.DataModel.Model;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Coments
{
    public class PublishCommentModel
    {
        public CommentType Type { get; set; }

        public string ObjectId { get; set; }

        public string Text { get; set; }
    }
}
