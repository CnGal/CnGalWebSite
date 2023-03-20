using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Admin.Shared.Services
{
    public class SingleDataCacheService<TModel> : ISingleDataCacheService<TModel> where TModel : class,new()
    {
        public TModel Date { get; set; }=new TModel();
    }
}
