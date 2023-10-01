using CnGalWebSite.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.ProjectSite.Models.Base
{
    public class BaseEditModel
    {
        public string Name { get; set; }

        public long Id { get; set; }

        public virtual Result Validate()
        {
            return new Result
            {
                Success = true,
            };
        }
    }
}
