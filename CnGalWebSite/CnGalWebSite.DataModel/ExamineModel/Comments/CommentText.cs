using CnGalWebSite.DataModel.Model;

using System;
namespace CnGalWebSite.DataModel.ExamineModel.Comments
{
    public class CommentText
    {
        public long Id { get; set; }

        public DateTime CommentTime { get; set; }

        public string Text { get; set; }

        public CommentType Type { get; set; }

        /// <summary>
        /// 发表评论的用户
        /// </summary>
        public string PubulicUserId { get; set; }
        /// <summary>
        /// 归属的对象id 文章 词条 空间 评论
        /// </summary>
        public string ObjectId { get; set; }

    }
}
