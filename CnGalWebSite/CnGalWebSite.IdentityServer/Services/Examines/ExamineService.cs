using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Extensions;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using CnGalWebSite.IdentityServer.Models.DataModels.Messages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Examines
{
    public class ExamineService:IExamineService
    {
        private readonly IRepository<ApplicationDbContext, Examine, long> _examineRepository;


        public ExamineService(IRepository<ApplicationDbContext, Examine, long> examineRepository)
        {
            _examineRepository = examineRepository;
        }

        public async Task AddExamines(ApplicationUser user,Examine examine)
        {
            //查找是否存在审核
            var cur = await _examineRepository.GetAll()
                .Include(s=>s.ClientExamine)
                .FirstOrDefaultAsync(s => s.ApplicationUser.Id == user.Id && s.IsPassed == null && s.Type == examine.Type && s.ClientId == examine.ClientId);

            if (cur == null)
            {
                examine.ApplicationUserId = user.Id;
                examine.Id = 0;
                cur = await _examineRepository.InsertAsync(examine);
            }

            //同步内容
            cur.ApplyTime = DateTime.UtcNow;
            cur.ClientExamine.SynchronizationProperties(examine.ClientExamine);

            //保存
            await _examineRepository.UpdateAsync(examine);
        }
    }
}
