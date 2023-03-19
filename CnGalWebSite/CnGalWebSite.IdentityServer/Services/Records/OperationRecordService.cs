using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Models.DataModels.Messages;
using CnGalWebSite.IdentityServer.Models.DataModels.Records;

namespace CnGalWebSite.IdentityServer.Services.Records
{
    public class OperationRecordService:IOperationRecordService
    {
        private readonly IRepository<OperationRecord, long> _operationRecordRepository;

        public OperationRecordService(IRepository<OperationRecord, long> _operationRecordRepository)
        {

        }
    }
}
