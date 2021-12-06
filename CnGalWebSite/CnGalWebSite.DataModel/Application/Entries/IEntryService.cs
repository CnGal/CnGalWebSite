using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Entries.Dtos;
using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.Application.Entries
{
    public interface IEntryService
    {
        Task<PagedResultDto<Entry>> GetPaginatedResult(GetEntryInput input);
    }
}
