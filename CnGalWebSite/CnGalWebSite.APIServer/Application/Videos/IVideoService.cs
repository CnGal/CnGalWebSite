using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.ExamineModel.Videos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Videos;

namespace CnGalWebSite.APIServer.Application.Videos
{
    public interface IVideoService
    {
        Task<List<long>> GetIdsFromNames(List<string> names);

        void UpdateMain(Video video, ExamineMain examine);

        Task UpdateRelevances(Video video, VideoRelevances examine);

        void UpdateMainPage(Video video, string examine);

        void UpdateImages(Video video, VideoImages examine);

        Task UpdateData(Video video, Examine examine);

        VideoViewModel GetViewModel(Video video);

        List<KeyValuePair<object, Operation>> ExaminesCompletion(Video currentVideo, Video newVideo);

        List<VideoViewModel> ConcompareAndGenerateModel(Video currentVideo, Video newVideo);

        Task<VideoEditState> GetEditState(ApplicationUser user, long id);

        void SetDataFromEditMain(Video item, EditVideoMainViewModel model);

        void SetDataFromEditMainPage(Video item, EditVideoMainPageViewModel model);

        void SetDataFromEditRelevances(Video item, EditVideoRelevancesViewModel model, List<Entry> entries, List<Article> articles, List<Video> videos);

        void SetDataFromEditImages(Video item, EditVideoImagesViewModel model);

        EditVideoMainViewModel GetEditMain(Video item);

        EditVideoImagesViewModel GetEditImages(Video item);

        EditVideoRelevancesViewModel GetEditRelevances(Video item);

        EditVideoMainPageViewModel GetEditMainPage(Video item);

        ExaminePreDataModel GetExamineViewMain(Video item);

        ExaminePreDataModel GetExamineViewRelevances(Video item);

        ExaminePreDataModel GetExamineViewImages(Video item);
    }
}
