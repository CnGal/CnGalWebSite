using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Admin.Shared.Services
{
    public interface ISingleDataCacheService<TModel> where TModel : class
    {
        public TModel Date { get; set; }
    }
}
