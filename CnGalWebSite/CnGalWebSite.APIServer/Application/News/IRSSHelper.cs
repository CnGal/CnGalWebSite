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

        Task<OriginalRSS> CorrectOriginalWeiboInfor(string Text, long id);

        Task<OriginalRSS> GetOriginalWeibo(long id, string keyWord);

        Task<List<OriginalRSS>> GetOriginalBilibili(long id, DateTime fromTime);
    }
}
