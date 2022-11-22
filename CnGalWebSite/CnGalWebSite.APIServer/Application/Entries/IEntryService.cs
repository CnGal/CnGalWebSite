using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Entries.Dtos;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tag = CnGalWebSite.DataModel.Model.Tag;

namespace CnGalWebSite.APIServer.Application.Entries
{
    public interface IEntryService
    {
        Task<PagedResultDto<Entry>> GetPaginatedResult(GetEntryInput input);

        Task<QueryData<ListEntryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListEntryAloneModel searchModel);

        Task<PagedResultDto<EntryInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input);

        void UpdateEntryDataMain(Entry entry, ExamineMain examine);

        void UpdateEntryDataMain(Entry entry, EntryMain_1_0 examine);

        Task UpdateEntryDataAddInforAsync(Entry entry, EntryAddInfor examine);

        void UpdateEntryDataImages(Entry entry, EntryImages examine);

        Task UpdateEntryDataRelevances(Entry entry, EntryRelevances examine);

        Task UpdateEntryDataTagsAsync(Entry entry, EntryTags examine);

        void UpdateEntryDataMainPage(Entry entry, string examine);

        Task UpdateEntryDataAsync(Entry entry, Examine examine);

        void UpdateEntryDataAudio(Entry entry, EntryAudioExamineModel examine);

        Task<List<int>> GetEntryIdsFromNames(List<string> names);

        Task<EntryEditState> GetEntryEditState(ApplicationUser user, int entryId);

        Task<EntryIndexViewModel> GetEntryIndexViewModelAsync(Entry entry);

        List<KeyValuePair<object, Operation>> ExaminesCompletion(Entry currentEntry, Entry newEntry);

        Task<List<EntryIndexViewModel>> ConcompareAndGenerateModel(Entry currentEntry, Entry newEntry);

        EditMainViewModel GetEditMainViewModel(Entry entry);

        EditAddInforViewModel GetEditAddInforViewModel(Entry entry);

        EditImagesViewModel GetEditImagesViewModel(Entry entry);

        EditRelevancesViewModel GetEditRelevancesViewModel(Entry entry);

        EditMainPageViewModel GetEditMainPageViewModel(Entry entry);

        EditEntryTagViewModel GetEditTagsViewModel(Entry entry);

        EditAudioViewModel GetEditAuioViewModel(Entry entry);

        void SetDataFromEditMainViewModel(Entry newEntry, EditMainViewModel model);

        Task SetDataFromEditAddInforViewModelAsync(Entry newEntry, EditAddInforViewModel model);

        void SetDataFromEditImagesViewModel(Entry newEntry, EditImagesViewModel model);

        void SetDataFromEditRelevancesViewModel(Entry newEntry, EditRelevancesViewModel model, List<Entry> entries, List<Article> articles);

        void SetDataFromEditMainPageViewModel(Entry newEntry, EditMainPageViewModel model);

        void SetDataFromEditTagsViewModel(Entry newEntry, EditEntryTagViewModel model, List<Tag> tags);

        void SetDataFromEditAudioViewModel(Entry newEntry, EditAudioViewModel model);
    }
}
