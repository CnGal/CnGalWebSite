using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.TimedTask.DataReositories;
using CnGalWebSite.TimedTask.Models.DataModels;
using CnGalWebSite.TimedTask.Models.ViewModels;
using CnGalWebSite.TimedTask.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.TimedTask.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/timedtasks/[action]")]
    [ApiController]
    public class TimedTaskController : ControllerBase
    {
        private readonly IRepository<TimedTaskModel, int> _timedTaskRepository;
        private readonly IQueryService _queryService;
        private readonly ITimedTaskService _timedTaskService;

        public TimedTaskController( IRepository<TimedTaskModel, int> timedTaskRepository, IQueryService queryService, ITimedTaskService timedTaskService)
        {
            _timedTaskRepository = timedTaskRepository;
            _queryService = queryService;
            _timedTaskService = timedTaskService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<TimedTaskOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<TimedTaskModel, int>(_timedTaskRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<TimedTaskOverviewModel>
            {
                Items = await items.Select(s => new TimedTaskOverviewModel
                {
                    Id = s.Id,
                    Type = s.Type,
                    ExecuteType = s.ExecuteType,
                    IntervalTime = s.IntervalTime,
                    EveryTime = s.EveryTime,
                    LastExecutedTime = s.LastExecutedTime,
                    Parameter = s.Parameter,
                    IsPause = s.IsPause,
                    IsRuning = s.IsRuning,
                    IsLastFail = s.IsLastFail
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<TimedTaskEditModel>> EditAsync(long id)
        {
            var item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new TimedTaskEditModel
            {
                Id = item.Id,
                Name = item.Name,
                Type = item.Type,
                ExecuteType = item.ExecuteType,
                IntervalTime = item.IntervalTime,
                EveryTime = item.EveryTime,
                LastExecutedTime = item.LastExecutedTime,
                Parameter = item.Parameter,
                IsPause = item.IsPause,
                IsRuning = item.IsRuning,
                IsLastFail = item.IsLastFail
            };
            if(model.EveryTime==null)
            {
                model.EveryTime = DateTime.Now;
            }
            if (model.EveryTime.Value.Year==1)
            {
                model.EveryTime= model.EveryTime.Value.AddYears(2022);
            }

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditAsync(TimedTaskEditModel model)
        {
            TimedTaskModel item = null;
            if (model.Id == 0)
            {
                item = await _timedTaskRepository.InsertAsync(new TimedTaskModel
                {
                    Name = model.Name,
                    Type = model.Type,
                    ExecuteType = model.ExecuteType,
                    IntervalTime = model.IntervalTime,
                    EveryTime = model.EveryTime,
                    LastExecutedTime = model.LastExecutedTime,
                    Parameter = model.Parameter,
                    IsPause = model.IsPause,
                    IsRuning = model.IsRuning,
                    IsLastFail = model.IsLastFail
                });
                model.Id = item.Id;
                _timedTaskRepository.Clear();
            }

            item = await _timedTaskRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.Id);


            if (item == null)
            {
                return new Result { Success = false, Message = "项目不存在" };
            }

            item.Name = model.Name;
            item.Type = model.Type;
            item.ExecuteType = model.ExecuteType;
            item.IntervalTime = model.IntervalTime;
            item.EveryTime = model.EveryTime;
            item.LastExecutedTime = model.LastExecutedTime;
            item.Parameter = model.Parameter;
            item.IsPause = model.IsPause;
            item.IsRuning = model.IsRuning;
            item.IsLastFail = model.IsLastFail;

            await _timedTaskRepository.UpdateAsync(item);

            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> PauseTimedTaskAsync(PauseTimedTaskModel model)
        {
            await _timedTaskRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.IsPause, b => model.IsPause));

            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> DeleteTimedTaskAsync(DeleteTimedTaskModel model)
        {

            await _timedTaskRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteDeleteAsync();
            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> RunTimedTaskAsync(RunTimedTaskModel model)
        {
            foreach (var item in model.Ids)
            {
                var timedTask = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item);
                if (timedTask != null)
                {
                    await _timedTaskService.RunTimedTask(timedTask);
                }
                else
                {
                    return new Result { Success = false, Message = "无法找到Id：" + item + " 的定时任务" };
                }
            }

            return new Result { Success = true };
        }
    }
}
