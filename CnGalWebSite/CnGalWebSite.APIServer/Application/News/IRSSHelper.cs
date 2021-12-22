using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.News
{
    public interface IRSSHelper
    {
        Task<List<OriginalRSS>> GetOriginalWeibo(long id, DateTime fromTime);

        Task<WeiboUserInfor> GetWeiboUserInfor(long id);
    }
}
