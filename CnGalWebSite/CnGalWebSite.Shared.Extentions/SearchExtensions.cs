using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Extentions
{
    public static class SearchExtensions
    {

    }
    public class QueryPageOptionsHelper : CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions
    {
        //TODO:全面更换后台管理中的表格组件
        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="cat"></param>
        public static implicit operator QueryPageOptionsHelper(BootstrapBlazor.Components.QueryPageOptions cat)
        {
            return new QueryPageOptionsHelper
            {
                SearchText = cat.SearchText,
                SortName = cat.SortName,
                SortOrder = (CnGalWebSite.DataModel.ViewModel.Search.SortOrder)cat.SortOrder,
                SearchModel = cat.SearchModel,
                PageIndex = cat.PageIndex,
                StartIndex = cat.StartIndex,
                PageItems = cat.PageItems,
                IsPage = cat.IsPage,
            };
        }
    }
}
