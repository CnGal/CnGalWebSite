using CnGalWebSite.IdentityServer.Models.DataModels.Records;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Records
{
    public interface IOperationRecordService
    {
        Task AddOperationRecordAsync(string userId, OperationRecordType type);
    }
}
