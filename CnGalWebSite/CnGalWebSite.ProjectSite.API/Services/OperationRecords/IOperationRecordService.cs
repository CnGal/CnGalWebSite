
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using CnGalWebSite.ProjectSite.Models.DataModels;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.API.Services
{
    public interface IOperationRecordService
    {
        Task AddOperationRecord(OperationRecordType type,long objectId, PageType pageType,long pageId,  ApplicationUser user, DeviceIdentificationModel model);
    }
}
