using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Coments
{
    public class PublishCommentModel
    {
        public CommentType Type { get; set; }

        public string ObjectId { get; set; }

        public string Text { get; set; }
    }
}
