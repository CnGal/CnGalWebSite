
using CnGalWebSite.DataModel.ExamineModel.Dismbigs;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Disambigs
{
    public interface IDisambigService
    {
        void UpdateDisambigDataMain(Disambig disambig, DisambigMain examine);

        Task UpdateDisambigDataRelevancesAsync(Disambig disambig, DisambigRelevances examine);

        Task UpdateDisambigDataAsync(Disambig disambig, Examine examine);
    }
}
