
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel.Dismbigs;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Disambigs
{
    public class DisambigService : IDisambigService
    {

        private readonly IRepository<Disambig, long> _disambigRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;

        public DisambigService(IRepository<Disambig, long> disambigRepository, IRepository<Entry, int> entryRepository,
            IRepository<Article, long> articleRepository)
        {
            _disambigRepository = disambigRepository;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
        }

        public void UpdateDisambigDataMain(Disambig disambig, DisambigMain examine)
        {
            disambig.Name = examine.Name;
            disambig.BriefIntroduction = examine.BriefIntroduction;
            disambig.MainPicture = examine.MainPicture;
            disambig.BackgroundPicture = examine.BackgroundPicture;
            disambig.SmallBackgroundPicture = examine.SmallBackgroundPicture;

        }

        public async Task UpdateDisambigDataRelevancesAsync(Disambig disambig, DisambigRelevances examine)
        {
            foreach (var item in examine.Relevances)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                if (item.Type == DisambigRelevanceType.Entry)
                {
                    foreach (var infor in disambig.Entries)
                    {

                        if (infor.Id == item.EntryId)
                        {
                            //查看是否为删除操作
                            if (item.IsDelete == true)
                            {
                                disambig.Entries.Remove(infor);
                            }
                            else
                            {
                                //查找词条
                                var temp = await _entryRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                                if (temp != null)
                                {
                                    disambig.Entries.Add(temp);
                                }
                            }
                            isAdd = true;
                            break;
                        }
                    }
                }
                else if (item.Type == DisambigRelevanceType.Article)
                {
                    foreach (var infor in disambig.Articles)
                    {

                        if (infor.Id == item.EntryId)
                        {
                            //查看是否为删除操作
                            if (item.IsDelete == true)
                            {
                                disambig.Articles.Remove(infor);
                            }
                            else
                            {
                                //查找词条
                                var temp = await _articleRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                                if (temp != null)
                                {
                                    disambig.Articles.Add(temp);
                                }
                            }
                            isAdd = true;
                            break;
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    if (item.Type == DisambigRelevanceType.Entry)
                    {
                        var temp = await _entryRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                        if (temp != null)
                        {
                            disambig.Entries.Add(temp);
                        }
                    }
                    else if (item.Type == DisambigRelevanceType.Article)
                    {
                        var temp = await _articleRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                        if (temp != null)
                        {
                            disambig.Articles.Add(temp);
                        }
                    }
                }
            }
        }

        public async Task UpdateDisambigDataAsync(Disambig disambig, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.DisambigMain:
                    DisambigMain disambigMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        disambigMain = (DisambigMain)serializer.Deserialize(str, typeof(DisambigMain));
                    }

                    UpdateDisambigDataMain(disambig, disambigMain);
                    break;
                case Operation.DisambigRelevances:
                    DisambigRelevances disambigRelevances = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        disambigRelevances = (DisambigRelevances)serializer.Deserialize(str, typeof(DisambigRelevances));
                    }

                    await UpdateDisambigDataRelevancesAsync(disambig, disambigRelevances);
                    break;

            }
        }
    }
}
