using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Tags;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.ThematicPages;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;


namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/thirdparties/[action]")]
    public class ThirdPartyAPIController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ThirdPartyAPIController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.DefaultRequestHeaders.Add("Cookie", _configuration["BilibiliCookie"]);
        }

        /// <summary>
        /// 获取B站视频信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<string>> GetBilibiliVideoInfor([FromQuery]string id)
        {
            return await _httpClient.GetStringAsync($"http://api.bilibili.com/x/web-interface/view/detail?{(id[0] == 'B' || id[0] == 'b' ? $"bvid={id}" : $"aid={id.Replace("av","")}")}");
        }

        /// <summary>
        /// 获取B站视频预览图拼版
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<string>>> GetBilibiliVideoImageImposition([FromQuery] string id)
        {
            var json = await _httpClient.GetStringAsync($"http://api.bilibili.com/pvideo?aid={(id[0] == 'B' || id[0] == 'b' ? id.ToBilibiliAid() : id.Replace("av", ""))}");
            var result = JObject.Parse(json);

            if (result["code"].ToObject<int>()==0)
            {
                return result["data"]["image"].ToArray().Select(s => s.ToString()).ToList();
            }
            else
            {
                return BadRequest(result["message"].ToString());
            }
        }
    }
}
