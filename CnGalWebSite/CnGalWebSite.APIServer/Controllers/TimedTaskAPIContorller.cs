using CnGalWebSite.APIServer.Application.TimedTasks;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.TimedTasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/timedtasks/[action]")]

    public class TimedTaskAPIContorller : ControllerBase
    {
        private readonly IRepository<TimedTask, int> _timedTaskRepository;
        private readonly ITimedTaskService _timedTaskService;
        private readonly IQueryService _queryService;

        public TimedTaskAPIContorller(ITimedTaskService timedTaskService, IRepository<TimedTask, int> timedTaskRepository, IQueryService queryService)
        {
            _timedTaskRepository = timedTaskRepository;
            _timedTaskService = timedTaskService;
            _queryService = queryService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<TimedTaskOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<TimedTask, int>(_timedTaskRepository.GetAll().AsSingleQuery(), model,
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

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditAsync(TimedTaskEditModel model)
        {
            TimedTask item = null;
            if (model.Id == 0)
            {
                item = await _timedTaskRepository.InsertAsync(new TimedTask
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
                return new Result { Successful = false, Error = "项目不存在" };
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

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> PauseTimedTaskAsync(PauseTimedTaskModel model)
        {
            await _timedTaskRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsPause, b => model.IsPause));

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> DeleteTimedTaskAsync(DeleteTimedTaskModel model)
        {

            await _timedTaskRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteDeleteAsync();
            return new Result { Successful = true };
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
                    return new Result { Successful = false, Error = "无法找到Id：" + item + " 的定时任务" };
                }
            }

            return new Result { Successful = true };
        }

    }
}
