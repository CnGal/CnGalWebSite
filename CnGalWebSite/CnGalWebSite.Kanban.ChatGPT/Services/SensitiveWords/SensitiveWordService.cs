using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.EventBus.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.Kanban.ChatGPT.Services.SensitiveWords
{
    public class SensitiveWordService : ISensitiveWordService
    {
        private readonly ILogger<SensitiveWordService> _logger;
        private readonly IEventBusService _eventBusService;


        public SensitiveWordService(ILogger<SensitiveWordService> logger, IEventBusService eventBusService)
        {
            _logger = logger;
            _eventBusService = eventBusService;
        }

        public async Task<List<string>> Check(List<string> texts)
        {
            return (await _eventBusService.CallSensitiveWordsCheck(new EventBus.Models.SensitiveWordsCheckModel
            {
                Texts = texts
            })).Words;
        }

        public async Task<List<string>> Check(string text)
        {
            return (await _eventBusService.CallSensitiveWordsCheck(new EventBus.Models.SensitiveWordsCheckModel
            {
                Texts = [text]
            })).Words;
        }
    }
}
