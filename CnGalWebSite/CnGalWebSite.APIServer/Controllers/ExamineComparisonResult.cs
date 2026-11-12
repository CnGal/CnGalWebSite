namespace CnGalWebSite.APIServer.Controllers
{
    using System;
    using System.Threading.Tasks;
    using CnGalWebSite.APIServer.ExamineX;
    using CnGalWebSite.DataModel.ViewModel.Admin;
    using Microsoft.AspNetCore.Mvc;

    internal static class ExamineComparisonResult
    {
        public static async Task<ActionResult<EditRecordComparison>> ExecuteAsync(
            IExamineService examineService,
            long contrastId,
            long currentId,
            ExaminedNormalListModelType type)
        {
            try
            {
                return await examineService.GetEditRecordComparison(contrastId, currentId, type);
            }
            catch (ArgumentException error)
            {
                return new BadRequestObjectResult(error.Message);
            }
        }
    }
}
