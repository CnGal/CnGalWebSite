using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.RobotClient.DataRepositories;
using CnGalWebSite.RobotClient.Services.ExternalDatas;
using CnGalWebSite.RobotClient.Services.SensitiveWords;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient.Services.Synchronous
{
    public class SynchronousService:ISynchronousService,IDisposable
    {
        private readonly IRepository<RobotReply> _robotReplyRepository;
        private readonly IRepository<RobotFace> _robotFaceRepository;
        private readonly IRepository<RobotGroup> _robotGroupRepository;
        private readonly IRepository<RobotEvent> _robotEventRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SynchronousService> _logger;
        private readonly IHttpService _httpService;

        System.Timers.Timer t =null;

        public SynchronousService(IRepository<RobotReply> robotReplyRepository, IRepository<RobotFace> robotFaceRepository, IExternalDataService externalDataService, IHttpService httpService, IRepository<RobotGroup> robotGroupRepository, IRepository<RobotEvent> robotEventRepository,
        ILogger<SynchronousService> logger,
        IConfiguration configuration,
            ISensitiveWordService sensitiveWordService)
        {
            _robotReplyRepository = robotReplyRepository;
            _logger = logger;
            _configuration = configuration;
            _robotFaceRepository = robotFaceRepository;
            _httpService = httpService;
            _robotGroupRepository = robotGroupRepository;
            _robotEventRepository = robotEventRepository;

            Init();
        }

        public void Init()
        {
            if (long.TryParse(_configuration["SynchronousIntervalTime"], out long time) == false)
            {
                time =  60;
                _logger.LogError("获取同步间隔时间失败，采用默认值1分钟");

            }
            t = new(time*1000);
            
            t.Start(); //启动计时器
            t.Elapsed += async (s, e) =>
            {
                await RefreshAsync();
            };
        }

        public async Task RefreshAsync()
        {
            _logger.LogInformation("正在同步数据");
            try
            {
                var model = await _httpService.GetAsync<List<RobotFace>>("api/robot/getrobotFaces");

                if (_robotFaceRepository.GetAll().Any())
                {
                    _robotFaceRepository.GetAll().Clear();
                }
                _robotFaceRepository.GetAll().AddRange(model);
                _robotFaceRepository.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "无法获取表情列表");
            }

            try
            {
                var model = await _httpService.GetAsync<List<RobotEvent>>("api/robot/getrobotEvents");
                var name = _configuration["RobotName"] ?? "看板娘";
                foreach (var reply in model)
                {
                    reply.Text = reply.Text.Replace("$(name)", name);
                }

                if(_robotEventRepository.GetAll().Any())
                {
                    _robotEventRepository.GetAll().Clear();
                }
               
                _robotEventRepository.GetAll().AddRange(model);
                _robotEventRepository.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "无法获取事件列表");
            }

            try
            {
                var model = await _httpService.GetAsync<List<RobotGroup>>("api/robot/getrobotGroups");

                if (_robotGroupRepository.GetAll().Any())
                {
                    _robotGroupRepository.GetAll().Clear();
                }

                _robotGroupRepository.GetAll().AddRange(model);
                _robotGroupRepository.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "无法获取QQ群列表");
            }


            try
            {
                var model = await _httpService.GetAsync<List<RobotReply>>("api/robot/getrobotreplies");
                var name = _configuration["RobotName"] ?? "看板娘";
                var qq = _configuration["QQ"] ;
                foreach (var reply in model)
                {
                    reply.Key = reply.Key.Replace("$(name)", name);
                    reply.Value = reply.Value.Replace("$(name)", name);

                    reply.Key = reply.Key.Replace("$(robotqq)", qq);
                    reply.Value = reply.Value.Replace("$(robotqq)", qq);
                }
                if (_robotReplyRepository.GetAll().Any())
                {
                    _robotReplyRepository.GetAll().Clear();
                }
                _robotReplyRepository.GetAll().Clear();
                _robotReplyRepository.GetAll().AddRange(model);
                _robotReplyRepository.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "无法获取自动回复列表");
            }

            _logger.LogInformation( "同步数据完成");
        }

        public void Dispose()
        {
            if (t != null)
            {
                t.Dispose();
                t = null;
            }
        }
    }
}
