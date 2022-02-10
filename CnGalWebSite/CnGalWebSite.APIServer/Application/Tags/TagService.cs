using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Tags;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Tags
{
    public class TagService : ITagService
    {
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IAppHelper _appHelper;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Tag>, string, BootstrapBlazor.Components.SortOrder, IEnumerable<Tag>>> SortLambdaCacheApplicationUser = new();
        public TagService(IRepository<Tag, int> tagRepository, IRepository<Entry, int> entryRepository, IRepository<Examine, long> examineRepository,
            IAppHelper appHelper)
        {
            _tagRepository = tagRepository;
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _appHelper = appHelper;
        }

        public Task<BootstrapBlazor.Components.QueryData<ListTagAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListTagAloneModel searchModel)
        {
            IEnumerable<Tag> items = _tagRepository.GetAll().AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheApplicationUser.GetOrAdd(typeof(Tag), key => LambdaExtensions.GetSortLambda<Tag>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListTagAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListTagAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    IsHidden = item.IsHidden,
                    LastEditTime = item.LastEditTime
                });
            }

            return Task.FromResult(new BootstrapBlazor.Components.QueryData<ListTagAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public async Task<List<int>> GetTagIdsFromNames(List<string> names)
        {
            //判断关联是否存在
            var entryId = new List<int>();

            foreach (var item in names)
            {
                var infor = await _tagRepository.GetAll().AsNoTracking().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    throw new Exception("标签 " + item + " 不存在");
                }
                else
                {
                    entryId.Add(infor);
                }
            }
            //删除重复数据
            entryId = entryId.Distinct().ToList();

            return entryId;
        }


        public async Task UpdateTagDataOldAsync(Tag tag, TagEdit examine)
        {
            //修改 关联词条
            var childenEntries = tag.Entries;

            foreach (var item in examine.ChildrenEntries)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in childenEntries)
                {

                    if (infor.Name == item.Name)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            childenEntries.Remove(infor);

                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有在当前标签中找到 要添加的词条 将词条添加到标签中
                    var temp = await _entryRepository.FirstOrDefaultAsync(s => s.Name == item.Name);
                    if (temp != null)
                    {
                        childenEntries.Add(temp);
                    }
                }
            }
            //修改 子标签
            var childenTags = tag.InverseParentCodeNavigation;

            foreach (var item in examine.ChildrenTags)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in childenTags)
                {

                    if (infor.Name == item.Name)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            childenTags.Remove(infor);

                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有在当前标签中找到 要添加的子标签 将子标签添加到标签中
                    var temp = await _tagRepository.FirstOrDefaultAsync(s => s.Name == item.Name);
                    if (temp != null)
                    {
                        childenTags.Add(temp);
                    }
                }
            }

            //判断是否要修改父标签
            if ((tag.ParentCodeNavigation != null && tag.ParentCodeNavigation.Name != examine.ParentTag) || (tag.ParentCodeNavigation == null && !string.IsNullOrWhiteSpace(examine.ParentTag)))
            {
                var temp = await _tagRepository.FirstOrDefaultAsync(s => s.Name == examine.ParentTag);
                if (temp != null)
                {
                    tag.ParentCodeNavigation = temp;
                }
            }
            tag.Name = examine.Name;
            tag.LastEditTime = DateTime.Now.ToCstTime();
        }

        public async Task UpdateTagDataMainAsync(Tag tag, ExamineMain examine)
        {
            ToolHelper.ModifyDataAccordingToEditingRecord(tag, examine.Items);
            //更新父标签

            if (examine.Items.Any(s => s.Key == "ParentTagId"))
            {
                int parentTagId = 0;
                if (int.TryParse(examine.Items.FirstOrDefault(s => s.Key == "ParentTagId")?.Value, out parentTagId))
                {
                    //查找Tag
                    var tagNew = await _tagRepository.FirstOrDefaultAsync(s => s.Id == parentTagId);
                    if (tagNew != null)
                    {
                        tag.ParentCodeNavigation = tagNew;
                    }
                }
            }
            else
            {
                tag.ParentCodeNavigation = null;
            }
            //更新最后编辑时间
            tag.LastEditTime = DateTime.Now.ToCstTime();

        }

        public async Task UpdateTagDataMainAsync(Tag tag, TagMain_1_0 examine)
        {
            tag.Name = examine.Name;
            tag.BriefIntroduction = examine.BriefIntroduction;
            tag.MainPicture = examine.MainPicture;
            tag.Thumbnail = examine.Thumbnail;
            tag.BackgroundPicture = examine.BackgroundPicture;
            tag.SmallBackgroundPicture = examine.SmallBackgroundPicture;

            //更新父标签
            if (tag.ParentCodeNavigation?.Id != examine.ParentTagId && examine.ParentTagId > 0)
            {
                //查找Tag
                var tagNew = await _tagRepository.FirstOrDefaultAsync(s => s.Id == examine.ParentTagId);
                if (tagNew != null)
                {
                    tag.ParentCodeNavigation = tagNew;
                }
            }

            //更新最后编辑时间
            tag.LastEditTime = DateTime.Now.ToCstTime();

        }

        public async Task UpdateTagDataChildTagsAsync(Tag tag, TagChildTags examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = tag.InverseParentCodeNavigation;

            foreach (var item in examine.ChildTags)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Id == item.TagId)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                            isAdd = true;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //查找Tag
                    var tagNew = await _tagRepository.FirstOrDefaultAsync(s => s.Id == item.TagId);
                    if (tagNew != null)
                    {
                        relevances.Add(tagNew);
                    }
                }
            }

            //更新最后编辑时间
            tag.LastEditTime = DateTime.Now.ToCstTime();
        }

        public async Task UpdateTagDataChildEntriesAsync(Tag tag, TagChildEntries examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = tag.Entries;

            foreach (var item in examine.ChildEntries)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Id == item.EntryId)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                            isAdd = true;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //查找Tag
                    var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == item.EntryId);
                    if (tag != null)
                    {
                        relevances.Add(entry);
                    }
                }
            }

            //更新最后编辑时间
            tag.LastEditTime = DateTime.Now.ToCstTime();
        }

        public async Task UpdateTagDataAsync(Tag tag, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.EditTag:
                    TagEdit articleMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        articleMain = (TagEdit)serializer.Deserialize(str, typeof(TagEdit));
                    }

                    await UpdateTagDataOldAsync(tag, articleMain);
                    break;
                case Operation.EditTagMain:
                    ExamineMain tagMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        tagMain = (ExamineMain)serializer.Deserialize(str, typeof(ExamineMain));
                    }

                    await UpdateTagDataMainAsync(tag, tagMain);
                    break;
                case Operation.EditTagChildTags:
                    TagChildTags tagChildTags = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        tagChildTags = (TagChildTags)serializer.Deserialize(str, typeof(TagChildTags));
                    }

                    await UpdateTagDataChildTagsAsync(tag, tagChildTags);
                    break;
                case Operation.EditTagChildEntries:
                    TagChildEntries tagChildEntries = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        tagChildEntries = (TagChildEntries)serializer.Deserialize(str, typeof(TagChildEntries));
                    }

                    await UpdateTagDataChildEntriesAsync(tag, tagChildEntries);
                    break;

            }
        }

        public async Task<List<KeyValuePair<string, int>>> GetTagLevelListAsync(Tag tag)
        {
            if (tag == null)
            {
                return new List<KeyValuePair<string, int>>();
            }
            else
            {
                var result = new List<KeyValuePair<string, int>>
                {
                    new KeyValuePair<string, int>(tag.Name, tag.Id)
                };

                //尝试向上索引
                while (tag.ParentCodeNavigation != null)
                {
                    tag = await _tagRepository.GetAll().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Name == tag.ParentCodeNavigation.Name);
                    if (tag != null)
                    {
                        result.Insert(0, new KeyValuePair<string, int>(tag.Name, tag.Id));
                    }
                    else
                    {
                        break;
                    }
                }
                return result;
            }
        }

        public async Task<TagEditState> GetTagEditState(ApplicationUser user, long tagId)
        {
            var model = new TagEditState();
            //获取该词条的各部分编辑状态
            //读取审核信息
            List<Examine> examineQuery = null;
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
                               .Where(s => s.TagId == tagId && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EditTagMain || s.Operation == Operation.EditTagChildTags || s.Operation == Operation.EditTagChildEntries))
                               .Select(s => new Examine
                               {
                                   Operation = s.Operation
                               })
                               .ToListAsync();
            }

            if (user != null)
            {
                if (examineQuery.Any(s => s.Operation == Operation.EditTagMain))
                {
                    model.MainState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditTagChildTags))
                {
                    model.ChildTagsState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditTagChildEntries))
                {
                    model.ChildEntriesState = EditState.Preview;
                }


            }
            //获取各部分状态
            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.TagId == tagId && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditPeripheryMain))
                    {
                        model.MainState = EditState.locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
                if (model.ChildTagsState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EditTagChildTags))
                    {
                        model.ChildTagsState = EditState.locked;
                    }
                    else
                    {
                        model.ChildTagsState = EditState.Normal;
                    }
                }
                if (model.ChildEntriesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditTagChildEntries))
                    {
                        model.ChildEntriesState = EditState.locked;
                    }
                    else
                    {
                        model.ChildEntriesState = EditState.Normal;
                    }
                }

            }

            return model;
        }


        public async Task<TagIndexViewModel> GetTagViewModel(Tag tag)
        {
            var model = new TagIndexViewModel
            {
                Id = tag.Id,
                Name = tag.Name,
                BriefIntroduction = tag.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(tag.MainPicture, "app.png"),
                BackgroundPicture = _appHelper.GetImagePath(tag.BackgroundPicture, ""),
                SmallBackgroundPicture = _appHelper.GetImagePath(tag.SmallBackgroundPicture, ""),
                Thumbnail = _appHelper.GetImagePath(tag.Thumbnail, ""),
                ReaderCount = tag.ReaderCount,
                ParentTag = tag.ParentCodeNavigation == null ? null : _appHelper.GetTagInforTipViewModel(tag.ParentCodeNavigation),
                Taglevels = await GetTagLevelListAsync(tag),
                IsHidden= tag.IsHidden,
            };

          

            foreach (var item in tag.InverseParentCodeNavigation)
            {
                model.ChildrenTags.Add(_appHelper.GetTagInforTipViewModel(item));
            }

          
            foreach (var item in tag.Entries.Where(s => s.IsHidden == false))
            {
                model.ChildrenEntries.Add(await _appHelper.GetEntryInforTipViewModel(item));
            }

            return model;

        }

        public List<KeyValuePair<object, Operation>> ExaminesCompletion(Tag currentTag, Tag newTag)
        {
            var examines = new List<KeyValuePair<object, Operation>>();
            //第一部分 主要信息

            var examineMain = new ExamineMain
            {
                Items = ToolHelper.GetEditingRecordFromContrastData(currentTag, newTag)
            };
            if (currentTag.ParentCodeNavigation?.Id != newTag.ParentCodeNavigation?.Id)
            {
                examineMain.Items.Add(new ExamineMainAlone
                {
                    Key = "ParentTagId",
                    Value = newTag.ParentCodeNavigation?.Id.ToString()
                });
            }

            if (examineMain.Items.Count > 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(examineMain, Operation.EditTagMain));

            }

            //第二部分 子标签
            var tagChildTags = new TagChildTags();

            foreach (var item in currentTag.InverseParentCodeNavigation)
            {
                tagChildTags.ChildTags.Add(new TagChildTagAloneModel
                {
                    TagId = item.Id,
                    Name = item.Name,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in newTag.InverseParentCodeNavigation)
            {
                var temp = tagChildTags.ChildTags.FirstOrDefault(s => s.TagId == item.Id);
                if (temp != null)
                {
                    tagChildTags.ChildTags.Remove(temp);
                }
                else
                {
                    tagChildTags.ChildTags.Add(new TagChildTagAloneModel
                    {
                        TagId = item.Id,
                        Name=item.Name,
                        IsDelete = false
                    });
                }
            }
            if (tagChildTags.ChildTags.Count != 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(tagChildTags, Operation.EditTagChildTags));
            }

            //第三部分 子词条
            var tagChildEntries = new TagChildEntries();


            foreach (var item in currentTag.Entries)
            {
                tagChildEntries.ChildEntries.Add(new TagChildEntryAloneModel
                {
                    EntryId = item.Id,
                    Name = item.Name,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in newTag.Entries)
            {
                var temp = tagChildEntries.ChildEntries.FirstOrDefault(s => s.EntryId == item.Id);
                if (temp != null)
                {
                    tagChildEntries.ChildEntries.Remove(temp);
                }
                else
                {
                    tagChildEntries.ChildEntries.Add(new TagChildEntryAloneModel
                    {
                        EntryId = item.Id,
                        Name = item.Name,
                        IsDelete = false
                    });
                }
            }
            if (tagChildEntries.ChildEntries.Count != 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(tagChildEntries, Operation.EditTagChildEntries));
            }


            return examines;
        }

        public async Task<List<TagIndexViewModel>> ConcompareAndGenerateModel(Tag currentTag, Tag newTag)
        {
            var model = new List<TagIndexViewModel>();

            model.Add(await GetTagViewModel(currentTag));
            model.Add(await GetTagViewModel(newTag));



            return model;
        }

    }
}
