using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Shared
{

    public class QueryResultModel<TModel> where TModel : class
    {
        public IEnumerable<TModel> Items { get; set; } = Enumerable.Empty<TModel>();

        public QueryParameterModel Parameter { get; set; }=new QueryParameterModel();

        public int Total { get; set; }
    }
    public class QueryParameterModel
    {
        public string SearchText { get; set; }

        public int Page { get; set; } = 1;

        public int ItemsPerPage { get; set; } = 10;

        public IEnumerable<bool> SortDesc { get; set; } = new bool[] { true };

        public IEnumerable<bool> GroupDesc { get; set; } = Enumerable.Empty<bool>();

        public bool MustSort { get; set; }

        public bool MultiSort { get; set; }

        public IEnumerable<string> SortBy { get; set; } = new string[] { "Id" };

        public IEnumerable<string> GroupBy { get; set; } = Enumerable.Empty<string>();
    }
}
