using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Models.Home
{
    public class SearchViewModel
    {
        public SearchViewPageModel pagedResultDto { get; set; } = new SearchViewPageModel();
    }

    public class SearchViewPageModel
    {
        public List<SearchViewPageDataModel> Data { get; set; } = new List<SearchViewPageDataModel>();
    }

    public class SearchViewPageDataModel
    {
        public SearchViewPageDataArticleModel Article { get; set; }=new SearchViewPageDataArticleModel();
    }

    public class SearchViewPageDataArticleModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string CreateUserName { get; set; }

        public string CreateUserId { get; set; }

        public string MainImage { get; set; }

        public string BriefIntroduction { get; set; }

        public DateTime LastEditTime { get; set; }

        public int ReaderCount { get; set; }

        public int ThumbsUpCount { get; set; }

        public int CommentCount { get; set; }

        public string Link { get; set; }
    }

}
