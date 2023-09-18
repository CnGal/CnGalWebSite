
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.OperationRecords
{
    public interface IOperationRecordService
    {
        Task AddOperationRecord(OperationRecordType type, string objectId, ApplicationUser user, DeviceIdentificationModel model, HttpContext context);

        Task<bool> CheckOperationRecord(OperationRecordType type, string objectId, ApplicationUser user, DeviceIdentificationModel model, HttpContext context);

        string GetIp(HttpContext context, string userIp);

        Task CopyOperationRecord(OperationRecordType fromType, string fromObjectId, OperationRecordType toType, string toObjectId, ApplicationUser user);

        Task<bool> CheckOperationRecord(OperationRecordType type, string objectId, ApplicationUser user);

    }
}
