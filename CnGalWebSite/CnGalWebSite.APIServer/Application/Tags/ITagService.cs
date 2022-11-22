using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.ExamineModel.Tags;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Tags;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Tags
{
    public interface ITagService
    {
        Task<BootstrapBlazor.Components.QueryData<ListTagAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListTagAloneModel searchModel);

        Task UpdateTagDataAsync(Tag tag, Examine examine);

        Task UpdateTagDataMainAsync(Tag tag, TagMain_1_0 examine);

        Task UpdateTagDataMainAsync(Tag tag, ExamineMain examine);

        Task UpdateTagDataChildTagsAsync(Tag tag, TagChildTags examine);

        Task UpdateTagDataChildEntriesAsync(Tag tag, TagChildEntries examine);

        Task UpdateTagDataOldAsync(Tag tag, TagEdit examine);

        Task<List<KeyValuePair<string, int>>> GetTagLevelListAsync(Tag tag);

        Task<TagEditState> GetTagEditState(ApplicationUser user, long tagId);

        Task<TagIndexViewModel> GetTagViewModel(Tag tag);

        List<KeyValuePair<object, Operation>> ExaminesCompletion(Tag currentTag, Tag newTag);

        Task<List<int>> GetTagIdsFromNames(List<string> names);

        Task<List<TagIndexViewModel>> ConcompareAndGenerateModel(Tag currentTag, Tag newTag);

        void SetDataFromEditTagMainViewModel(Tag newTag, EditTagMainViewModel model, Tag parentTag);

        void SetDataFromEditTagChildTagsViewModel(Tag newTag, EditTagChildTagsViewModel model, List<Tag> tags);

        void SetDataFromEditTagChildEntriesViewModel(Tag newTag, EditTagChildEntriesViewModel model, List<Entry> entries);
    }
}
