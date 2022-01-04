
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Entries.Dtos;
using CnGalWebSite.DataModel.Model;


namespace CnGalWebSite.DataModel.Application.Entries
{
    public class EntryService : IEntryService
    {

        Task<PagedResultDto<Entry>> IEntryService.GetPaginatedResult(GetEntryInput input)
        {
            throw new NotImplementedException();
        }
    }
}
