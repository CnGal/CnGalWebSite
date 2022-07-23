using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.OperationRecords
{
    public interface IOperationRecordService
    {
        Task<QueryData<ListOperationRecordAloneModel>> GetPaginatedResult(DataModel.ViewModel.Search.QueryPageOptions options, ListOperationRecordAloneModel searchModel);

        Task AddOperationRecord(OperationRecordType type, string objectId, ApplicationUser user, DeviceIdentificationModel model, HttpContext context);

        Task<bool> CheckOperationRecord(OperationRecordType type, string objectId, ApplicationUser user, DeviceIdentificationModel model, HttpContext context);

        string GetIp(HttpContext context, string userIp);
    }
}
