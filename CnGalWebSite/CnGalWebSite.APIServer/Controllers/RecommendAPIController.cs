using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Recommends;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{

    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/recommends/[action]")]
    public class RecommendAPIController : ControllerBase
    {
        private readonly IRepository<Recommend, long> _recommendRepository;
        private readonly IQueryService _queryService;

        public RecommendAPIController(IRepository<Recommend, long> recommendRepository,IQueryService queryService)
        {
            _recommendRepository = recommendRepository;
            _queryService = queryService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<RecommendOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Recommend, string>(_recommendRepository.GetAll().AsSingleQuery().Include(s => s.Entry), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Entry.Name != null && (s.Entry.Name.Contains(model.SearchText) || s.Id.ToString().Contains(model.SearchText))));

            return new QueryResultModel<RecommendOverviewModel>
            {
                Items = await items.Select(s => new RecommendOverviewModel
                {
                    EntryId = s.EntryId,
                    EntryName=s.Entry.Name,
                    IsHidden=s.IsHidden,
                    Reason=s.Reason,
                    Id = s.Id,
                    UpdateTime=s.UpdateTime
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpGet]
        public async Task<ActionResult<RecommendEditModel>> Edit(long id)
        {
            var item = await _recommendRepository.GetAll().FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("找不到推荐");
            }

            var model = new RecommendEditModel
            {
                EntryId = item.EntryId,
                IsHidden = item.IsHidden,
                Reason = item.Reason,
                Id = item.Id,
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> Edit(RecommendEditModel model)
        {
            var item = await _recommendRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.Id);

            if (item == null)
            {
                item = new Recommend();

                item = await _recommendRepository.InsertAsync(item);
            }



            item.EntryId = model.EntryId;
            item.IsHidden = model.IsHidden;
            item.Reason = model.Reason;
            item.UpdateTime = DateTime.Now.ToCstTime();

            await _recommendRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }
    }
}
