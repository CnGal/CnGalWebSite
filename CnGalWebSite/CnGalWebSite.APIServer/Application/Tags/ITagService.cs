
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Tags;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Tags
{
    public interface ITagService
    {
        Task<BootstrapBlazor.Components.QueryData<ListTagAloneModel>> GetPaginatedResult(BootstrapBlazor.Components.QueryPageOptions options, ListTagAloneModel searchModel);

        Task UpdateTagDataAsync(Tag tag, Examine examine);

        Task UpdateTagDataMainAsync(Tag tag, TagMain examine);

        Task UpdateTagDataChildTagsAsync(Tag tag, TagChildTags examine);

        Task UpdateTagDataChildEntriesAsync(Tag tag, TagChildEntries examine);

        Task UpdateTagDataOldAsync(Tag tag, TagEdit examine);

        Task<List<KeyValuePair<string, int>>> GetTagLevelListAsync(Tag tag);

        Task<TagEditState> GetTagEditState(ApplicationUser user, long tagId);
    }
}
