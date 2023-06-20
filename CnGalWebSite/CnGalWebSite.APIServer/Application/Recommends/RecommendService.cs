using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.APIServer.Application.Recommends
{
    public class RecommendService : IRecommendService
    {
        private readonly IRepository<Recommend, long> _recommendRepository;
        private readonly IRepository<Entry, long> _entryRepository;
        private readonly IRepository<StoreInfo, long> _storeInfoRepository;

        public RecommendService(IRepository<Recommend, long> recommendRepository, IRepository<Entry, long> entryRepository, IRepository<StoreInfo, long> storeInfoRepository)
        {
            _recommendRepository = recommendRepository;
            _entryRepository = entryRepository;
            _storeInfoRepository = storeInfoRepository;
        }

        public async Task Update()
        {
            //清空理由
            await _recommendRepository.GetAll().ExecuteUpdateAsync(s => s.SetProperty(a => a.Reason, b => RecommendReason.None));

            //更新理由
            await UpdateClassics();
            await UpdateNewHistoryLow();
            await UpdateUnpopularMasterpiece();
            await UpdateHighPraiseRate();
            await UpdatePopular();

            //删除没有理由的项目
            await _recommendRepository.GetAll().Where(s => s.Reason == RecommendReason.None).ExecuteDeleteAsync();
        }

        //经典作品
        public async Task UpdateClassics()
        {
            var gameIds = await _storeInfoRepository.GetAll().AsNoTracking()
                .Include(s=>s.Entry)
                .Where(s =>s.EntryId!=null &&  s.Entry.IsHidden == false&&s.EstimationOwnersMax >= 200000)
                .Select(s => s.EntryId.Value)
                .ToListAsync();

            //更新
            await UpdateRecomendItems(RecommendReason.Classics, gameIds);
        }

        //新史低
        public async Task UpdateNewHistoryLow()
        {
            var gameIds = await _storeInfoRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .Where(s => s.EntryId != null && s.Entry.IsHidden == false && s.CutLowest != null && s.CutLowest > 0 && s.CutNow > s.CutLowest)
                .Select(s => s.EntryId.Value)
                .ToListAsync();

            //更新
            await UpdateRecomendItems(RecommendReason.NewHistoryLow, gameIds);
        }

        //冷门佳作
        public async Task UpdateUnpopularMasterpiece()
        {
            var now = DateTime.Now.ToCstTime();
            var gameIds = await _storeInfoRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .Where(s => s.EntryId != null && s.Entry.IsHidden == false && s.Entry.PubulishTime != null && s.Entry.PubulishTime.Value.AddMonths(1) < now && s.RecommendationRate >= 90 && s.EstimationOwnersMin < 10000)
                .Select(s => s.EntryId.Value)
                .ToListAsync();

            //更新
            await UpdateRecomendItems(RecommendReason.UnpopularMasterpiece, gameIds);
        }

        //好评率高
        public async Task UpdateHighPraiseRate()
        {
            var now = DateTime.Now.ToCstTime();
            var gameIds = await _storeInfoRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .Where(s => s.EntryId != null && s.Entry.IsHidden == false && s.Entry.PubulishTime != null && s.Entry.PubulishTime.Value.AddMonths(1) < now && s.RecommendationRate >= 90 && s.EstimationOwnersMin >= 100000)
                .Select(s => s.EntryId.Value)
                .ToListAsync();

            //更新
            await UpdateRecomendItems(RecommendReason.HighPraiseRate, gameIds);
        }

        //本站热门
        public async Task UpdatePopular()
        {
            var now = DateTime.Now.ToCstTime();
            var gameIds = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden == false&&s.Type==EntryType.Game)
                .OrderByDescending(s => s.ReaderCount)
                .Take(10)
                .Select(s => s.Id)
                .ToListAsync();

            //更新
            await UpdateRecomendItems(RecommendReason.Popular, gameIds);
        }

        //更新推荐列表
        public async Task UpdateRecomendItems(RecommendReason reason,IEnumerable<int> gameIds)
        {
            var existsIds = await _recommendRepository.GetAll().AsNoTracking().Select(s => s.EntryId).ToListAsync();
            var now = DateTime.Now.ToCstTime();

            foreach (var item in gameIds.Distinct().Where(s => existsIds.Contains(s) == false))
            {
                await _recommendRepository.InsertAsync(new Recommend
                {
                    EntryId = item,
                    Reason = reason,
                    UpdateTime = now
                });
            }

            await _recommendRepository.GetAll().Where(s => gameIds.Contains(s.EntryId) && s.Reason > reason).ExecuteUpdateAsync(s => s.SetProperty(a => a.UpdateTime, b => now));
            await _recommendRepository.GetAll().Where(s => gameIds.Contains(s.EntryId) && s.Reason > reason).ExecuteUpdateAsync(s => s.SetProperty(a => a.Reason, b => reason));
        }
    }
}
