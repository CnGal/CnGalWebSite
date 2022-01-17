using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Tags;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Tags;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/tags/[action]")]
    public class TagsAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IAppHelper _appHelper;
        private readonly ITagService _tagService;
        private readonly IExamineService _examineService;

        public TagsAPIController(UserManager<ApplicationUser> userManager, IAppHelper appHelper, IRepository<Tag, int> tagRepository, ITagService tagService,
            IRepository<Entry, int> entryRepository, IExamineService examineService, IRepository<Examine, long> examineRepository)
        {
            _userManager = userManager;
            _tagRepository = tagRepository;
            _appHelper = appHelper;
            _tagService = tagService;
            _entryRepository = entryRepository;
            _examineService = examineService;
            _examineRepository = examineRepository;
        }


        [HttpPost]
        public async Task<ActionResult<Result>> CreateTagAsync(CreateTagViewModel model)
        {
            try
            {
                //获取当前用户ID
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                //检查是否超过编辑上限
                if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
                {
                    return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
                }

                //判断名称是否重复
                if (await _tagRepository.FirstOrDefaultAsync(s => s.Name == model.Name) != null)
                {
                    return new Result { Error = "该标签的名称与其他标签重复", Successful = false };
                }
                //判断父标签是否存在
                Tag parentTag = null;

                if (string.IsNullOrWhiteSpace(model.ParentTagName))
                {
                    return new Result { Error = "除四个顶级标签外，其他标签必须包含父标签", Successful = false };
                }
                else
                {
                    parentTag = await _tagRepository.FirstOrDefaultAsync(s => s.Name == model.ParentTagName);
                    if (parentTag == null)
                    {
                        return new Result { Error = "父标签必须真实存在", Successful = false };
                    }
                }
                //预处理 建立子标签关联信息
                //判断关联是否存在
                var tagIds = new List<int>();

                var tagNames = new List<string>();
                tagNames.AddRange(model.Tags.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                foreach (var item in tagNames)
                {
                    var infor = await _tagRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                    if (infor <= 0)
                    {
                        return new Result { Successful = false, Error = "标签 " + item + " 不存在" };
                    }
                    else
                    {
                        tagIds.Add(infor);
                    }
                }
                //删除重复数据
                tagIds = tagIds.Distinct().ToList();


                //预处理 建立子词条关联信息
                //判断关联是否存在
                var entryIds = new List<int>();

                var entryNames = new List<string>();
                entryNames.AddRange(model.Entries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                foreach (var item in entryNames)
                {
                    var infor = await _entryRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                    if (infor <= 0)
                    {
                        return new Result { Successful = false, Error = "词条 " + item + " 不存在" };
                    }
                    else
                    {
                        entryIds.Add(infor);
                    }
                }
                //删除重复数据
                entryIds = entryIds.Distinct().ToList();

                //第一步 处理主要信息

                //新建审核数据对象
                var tagMain = new TagMain
                {
                    Name = model.Name,
                    BriefIntroduction = model.BriefIntroduction,
                    MainPicture = model.MainPicture,
                    BackgroundPicture = model.BackgroundPicture,
                    SmallBackgroundPicture = model.SmallBackgroundPicture,
                    Thumbnail = model.Thumbnail,
                    ParentTagId = parentTag?.Id ?? 0
                };
                //序列化
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, tagMain);
                    resulte = text.ToString();
                }
                //将空标签添加到数据库中 目的是为了获取索引
                var tag = new Tag();

                tag = await _tagRepository.InsertAsync(tag);
                //判断是否是管理员
                if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                {
                    await _examineService.ExamineEditTagMainAsync(tag, tagMain);
                    await _examineService.UniversalCreateTagExaminedAsync(tag, user, true, resulte, Operation.EditTagMain, model.Note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, tag.Id, Operation.EditTagMain);
                }
                else
                {
                    await _examineService.UniversalCreateTagExaminedAsync(tag, user, false, resulte, Operation.EditTagMain, model.Note);
                }

                //第二步 处理子标签
                //创建审核数据模型
                var examinedModel = new TagChildTags();


                //添加新建项目
                foreach (var item in tagIds)
                {
                    examinedModel.ChildTags.Add(new TagChildTagAloneModel
                    {
                        TagId = item,
                        IsDelete = false
                    });

                }

                //判断审核是否为空
                if (examinedModel.ChildTags.Count != 0)
                {

                    //序列化JSON
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                    {
                        await _examineService.ExamineEditTagChildTagsAsync(tag, examinedModel);
                        await _examineService.UniversalCreateTagExaminedAsync(tag, user, true, resulte, Operation.EditTagChildTags, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, tag.Id, Operation.EditTagChildTags);
                    }
                    else
                    {
                        await _examineService.UniversalCreateTagExaminedAsync(tag, user, false, resulte, Operation.EditTagChildTags, model.Note);
                    }
                }


                //第三步 处理子词条 
                //创建审核数据模型
                var tagChildEntries = new TagChildEntries();


                //添加新建项目
                foreach (var item in entryIds)
                {
                    tagChildEntries.ChildEntries.Add(new TagChildEntryAloneModel
                    {
                        EntryId = item,
                        IsDelete = false
                    });

                }

                //判断审核是否为空
                if (tagChildEntries.ChildEntries.Count != 0)
                {
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, tagChildEntries);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                    {
                        await _examineService.ExamineEditTagChildEntriesAsync(tag, tagChildEntries);
                        await _examineService.UniversalCreateTagExaminedAsync(tag, user, true, resulte, Operation.EditTagChildEntries, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, tag.Id, Operation.EditTagChildEntries);
                    }
                    else
                    {
                        await _examineService.UniversalCreateTagExaminedAsync(tag, user, false, resulte, Operation.EditTagChildEntries, model.Note);
                    }
                }

                //序列化JSON


                return new Result { Successful = true, Error = tag.Id.ToString() };
            }
            catch (Exception)
            {
                return new Result { Error = "创建标签的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };
            }

        }


        [AllowAnonymous]
        [HttpGet("{_id}")]
        public async Task<ActionResult<TagIndexViewModel>> GetTagAsync(string _id)
        {

            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取词条 
            Tag tag = null;
            try
            {
                var id = -1;
                id = int.Parse(_id);
                tag = await _tagRepository.GetAll().AsNoTracking()
                    .Include(s => s.InverseParentCodeNavigation)
                    .Include(s => s.ParentCodeNavigation)
                    .Include(s => s.Entries)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch
            {
                tag = await _tagRepository.GetAll().AsNoTracking()
                    .Include(s => s.InverseParentCodeNavigation)
                    .Include(s => s.ParentCodeNavigation)
                    .Include(s => s.Entries)
                    .FirstOrDefaultAsync(x => x.Name == ToolHelper.Base64DecodeName(_id));
            }
            if (tag == null)
            {
                return NotFound();
            }
            //读取审核信息
            List<Examine> examineQuery = null;
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
                               .Where(s => s.TagId == tag.Id && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EditTagMain || s.Operation == Operation.EditTagChildTags || s.Operation == Operation.EditTagChildEntries))
                               .Select(s => new Examine
                               {
                                   Operation = s.Operation,
                                   Context = s.Context
                               })
                               .ToListAsync();
            }

            if (user != null)
            {
                var examine = examineQuery.Find(s => s.Operation == Operation.EditTagMain);
                if (examine != null)
                {
                    await _tagService.UpdateTagDataAsync(tag, examine);
                }
            }
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
                Taglevels = await _tagService.GetTagLevelListAsync(tag)
            };

            //判断当前是否隐藏
            if (tag.IsHidden == true)
            {
                if (user == null || await _userManager.IsInRoleAsync(user, "Admin") != true)
                {
                    return NotFound();
                }
                model.IsHidden = true;
            }
            else
            {
                model.IsHidden = false;
            }
            if (user != null)
            {
                var examine = examineQuery.Find(s => s.Operation == Operation.EditTagMain);
                if (examine != null)
                {
                    model.MainState = EditState.Preview;
                }
            }

            //序列化标签子标签
            //获取用户审核记录
            if (user != null)
            {
                var examine = await _examineService.GetUserTagActiveExamineAsync(tag.Id, user.Id, Operation.EditTagChildTags);
                if (examine != null)
                {
                    model.ChildTagsState = EditState.Preview;
                    await _tagService.UpdateTagDataAsync(tag, examine);
                }
            }
            foreach (var item in tag.InverseParentCodeNavigation)
            {
                model.ChildrenTags.Add(_appHelper.GetTagInforTipViewModel(item));
            }

            //序列化标签子词条
            //获取用户审核记录
            if (user != null)
            {
                var examine = await _examineService.GetUserTagActiveExamineAsync(tag.Id, user.Id, Operation.EditTagChildEntries);
                if (examine != null)
                {
                    model.ChildEntriesState = EditState.Preview;
                    await _tagService.UpdateTagDataAsync(tag, examine);
                }
            }
            foreach (var item in tag.Entries)
            {
                model.ChildrenEntries.Add(await _appHelper.GetEntryInforTipViewModel(item));
            }



            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.TagId == tag.Id && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }

            //获取各部分状态
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditTagMain))
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

            //增加阅读人数
            await _tagRepository.GetRangeUpdateTable().Where(s => s.Id == tag.Id).Set(s => s.ReaderCount, b => b.ReaderCount + 1).ExecuteAsync();

            return model;

        }

        /// <summary>
        /// 获取在词条编辑页面的简单表示 仅显示层级 例如 游戏->按时长分->短篇
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{tagName}")]
        public async Task<ActionResult<GetTagInEntryViewModel>> GetTagInEntryViewAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return NotFound("标签名称不能为空");
            }

            tagName = ToolHelper.Base64DecodeName(tagName);

            var tag = await _tagRepository.GetAll().Where(s => s.IsHidden != true).AsNoTracking().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Name == tagName);
            var result = await _appHelper.GetTagListStringAsync(tag);
            if (result == null)
            {
                return NotFound();
            }
            else
            {
                return new GetTagInEntryViewModel { Result = result };
            }
        }

        /// <summary>
        /// 获取搜索结果 标签数量很少 可以简化
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        [HttpGet("{tagName}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> SearchTagAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return NotFound("标签名称不能为空");
            }

            tagName = ToolHelper.Base64DecodeName(tagName);

            var que = _tagRepository.GetAll().Where(s => s.IsHidden != true).AsNoTracking().Include(s => s.ParentCodeNavigation).Where(s => s.Name.Contains(tagName));

            var result = new List<string>();
            foreach (var item in que)
            {
                result.Add(await _appHelper.GetTagListStringAsync(item));
            }
            return result;

        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EditTagMainViewModel>> EditMainAsync(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取周边
            var tag = await _tagRepository.GetAll().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (tag == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && (s.Operation == Operation.EditTagMain)))
            {
                return NotFound("当前标签该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑");
            }

            //获取审核记录
            var examine = await _examineService.GetUserTagActiveExamineAsync(tag.Id, user.Id, Operation.EditTagMain);
            if (examine != null)
            {
                await _tagService.UpdateTagDataAsync(tag, examine);
            }


            var model = new EditTagMainViewModel
            {
                Id = tag.Id,
                Name = tag.Name,
                BriefIntroduction = tag.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(tag.MainPicture, ""),
                BackgroundPicture = _appHelper.GetImagePath(tag.BackgroundPicture, ""),
                SmallBackgroundPicture = _appHelper.GetImagePath(tag.SmallBackgroundPicture, ""),
                Thumbnail = _appHelper.GetImagePath(tag.Thumbnail, ""),
                ParentTagName = tag.ParentCodeNavigation?.Name ?? ""
            };


            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditMainAsync(EditTagMainViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == model.Id && s.IsPassed == null && (s.Operation == Operation.EditTagMain)))
            {
                return new Result { Error = "当前标签该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }


            //查找当前周边
            var tag = await _tagRepository.GetAll().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (tag == null)
            {
                return new Result { Error = $"无法找到ID为{tag.Id}的标签", Successful = false };
            }
            //判断父标签是否存在
            Tag parentTag = null;
            if (tag.Name != "游戏" && tag.Name != "角色" && tag.Name != "制作组" && tag.Name != "STAFF")
            {
                if (string.IsNullOrWhiteSpace(model.ParentTagName))
                {
                    return new Result { Error = "除四个顶级标签外，其他标签必须包含父标签", Successful = false };
                }
                else
                {
                    parentTag = await _tagRepository.FirstOrDefaultAsync(s => s.Name == model.ParentTagName);
                    if (parentTag == null)
                    {
                        return new Result { Error = "父标签必须真实存在", Successful = false };
                    }
                }
            }

            //判断是否被修改
            if (model.SmallBackgroundPicture != tag.SmallBackgroundPicture || model.Name != tag.Name || model.BriefIntroduction != tag.BriefIntroduction
                || model.MainPicture != tag.MainPicture || model.Thumbnail != tag.Thumbnail || model.BackgroundPicture != tag.BackgroundPicture
                 || tag.ParentCodeNavigation?.Name != parentTag?.Name)
            {
                //添加修改记录
                //新建审核数据对象
                var tagMain = new TagMain
                {
                    Name = model.Name,
                    BriefIntroduction = model.BriefIntroduction,
                    MainPicture = model.MainPicture,
                    BackgroundPicture = model.BackgroundPicture,
                    SmallBackgroundPicture = model.SmallBackgroundPicture,
                    Thumbnail = model.Thumbnail,
                    ParentTagId = parentTag?.Id ?? 0
                };
                //序列化
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, tagMain);
                    resulte = text.ToString();
                }
                //判断是否是管理员
                if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                {
                    await _examineService.ExamineEditTagMainAsync(tag, tagMain);
                    await _examineService.UniversalEditTagExaminedAsync(tag, user, true, resulte, Operation.EditTagMain, model.Note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, tag.Id, Operation.EditPeripheryMain);
                }
                else
                {
                    await _examineService.UniversalEditTagExaminedAsync(tag, user, false, resulte, Operation.EditTagMain, model.Note);
                }
            }

            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditTagChildTagsViewModel>> EditChildTags(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取周边
            var tag = await _tagRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (tag == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && (s.Operation == Operation.EditTagChildTags)))
            {
                return NotFound("当前标签该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑");
            }

            //获取审核记录
            var examine = await _examineService.GetUserTagActiveExamineAsync(tag.Id, user.Id, Operation.EditTagChildTags);
            if (examine != null)
            {
                await _tagService.UpdateTagDataAsync(tag, examine);
            }

            //处理附加信息
            var model = new EditTagChildTagsViewModel
            {
                Id = id,
                Name = tag.Name
            };
            var tags = tag.InverseParentCodeNavigation;

            foreach (var item in tags)
            {
                model.Tags.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });
            }

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditChildTags(EditTagChildTagsViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == model.Id && s.IsPassed == null && (s.Operation == Operation.EditTagChildTags)))
            {
                return new Result { Error = "当前标签该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }


            //查找当前周边
            var tag = await _tagRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (tag == null)
            {
                return new Result { Error = $"无法找到ID为{tag.Id}的标签", Successful = false };
            }

            //预处理 建立词条关联信息
            //判断关联是否存在
            var tagIds = new List<int>();

            var tagNames = new List<string>();
            tagNames.AddRange(model.Tags.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            foreach (var item in tagNames)
            {
                var infor = await _tagRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    return new Result { Successful = false, Error = "标签 " + item + " 不存在" };
                }
                else
                {
                    tagIds.Add(infor);
                }
            }
            //删除重复数据
            tagIds = tagIds.Distinct().ToList();

            //创建审核数据模型
            var examinedModel = new TagChildTags();

            //遍历当前词条数据 打上删除标签
            foreach (var item in tag.InverseParentCodeNavigation)
            {
                examinedModel.ChildTags.Add(new TagChildTagAloneModel
                {
                    TagId = item.Id,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in tagIds.Where(s => examinedModel.ChildTags.Select(s => s.TagId).Contains(s) == false))
            {
                examinedModel.ChildTags.Add(new TagChildTagAloneModel
                {
                    TagId = item,
                    IsDelete = false
                });

            }
            //删除不存在的老项目
            examinedModel.ChildTags.RemoveAll(s => tagIds.Contains(s.TagId) && s.IsDelete);

            //判断审核是否为空
            if (examinedModel.ChildTags.Count == 0)
            {
                return new Result { Successful = true };
            }

            //序列化JSON
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, examinedModel);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEditTagChildTagsAsync(tag, examinedModel);
                await _examineService.UniversalEditTagExaminedAsync(tag, user, true, resulte, Operation.EditTagChildTags, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, tag.Id, Operation.EditTagChildTags);
            }
            else
            {
                await _examineService.UniversalEditTagExaminedAsync(tag, user, false, resulte, Operation.EditTagChildTags, model.Note);
            }
            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditTagChildEntriesViewModel>> EditChildEntries(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取周边
            var tag = await _tagRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (tag == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && (s.Operation == Operation.EditTagChildEntries)))
            {
                return NotFound("当前标签该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑");
            }

            //获取审核记录
            var examine = await _examineService.GetUserTagActiveExamineAsync(tag.Id, user.Id, Operation.EditTagChildEntries);
            if (examine != null)
            {
                await _tagService.UpdateTagDataAsync(tag, examine);
            }

            //处理附加信息
            var model = new EditTagChildEntriesViewModel
            {
                Id = id,
                Name = tag.Name
            };
            var tags = tag.Entries;

            foreach (var item in tags)
            {
                model.Entries.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });
            }

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditChildEntries(EditTagChildEntriesViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == model.Id && s.IsPassed == null && (s.Operation == Operation.EditTagChildEntries)))
            {
                return new Result { Error = "当前标签该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }


            //查找当前周边
            var tag = await _tagRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (tag == null)
            {
                return new Result { Error = $"无法找到ID为{tag.Id}的标签", Successful = false };
            }

            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryIds = new List<int>();

            var entryNames = new List<string>();
            entryNames.AddRange(model.Entries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            foreach (var item in entryNames)
            {
                var infor = await _entryRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    return new Result { Successful = false, Error = "词条 " + item + " 不存在" };
                }
                else
                {
                    entryIds.Add(infor);
                }
            }
            //删除重复数据
            entryIds = entryIds.Distinct().ToList();

            //创建审核数据模型
            var examinedModel = new TagChildEntries();

            //遍历当前词条数据 打上删除标签
            foreach (var item in tag.Entries)
            {
                examinedModel.ChildEntries.Add(new TagChildEntryAloneModel
                {
                    EntryId = item.Id,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in entryIds.Where(s => examinedModel.ChildEntries.Select(s => s.EntryId).Contains(s) == false))
            {
                examinedModel.ChildEntries.Add(new TagChildEntryAloneModel
                {
                    EntryId = item,
                    IsDelete = false
                });

            }
            //删除不存在的老项目
            examinedModel.ChildEntries.RemoveAll(s => entryIds.Contains(s.EntryId) && s.IsDelete);

            //判断审核是否为空
            if (examinedModel.ChildEntries.Count == 0)
            {
                return new Result { Successful = true };
            }

            //序列化JSON
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, examinedModel);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEditTagChildEntriesAsync(tag, examinedModel);
                await _examineService.UniversalEditTagExaminedAsync(tag, user, true, resulte, Operation.EditTagChildEntries, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, tag.Id, Operation.EditTagChildEntries);
            }
            else
            {
                await _examineService.UniversalEditTagExaminedAsync(tag, user, false, resulte, Operation.EditTagChildEntries, model.Note);
            }
            return new Result { Successful = true };

        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(b => b.Name).ToListAsync();

        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListTagAloneModel>>> GetTagListAsync(TagsPagesInfor input)
        {
            var dtos = await _tagService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }
        [HttpGet]
        public async Task<ActionResult<ListTagsInforViewModel>> ListTagsAsync()
        {
            var model = new ListTagsInforViewModel
            {
                All = await _tagRepository.CountAsync(),
                Hiddens = await _tagRepository.CountAsync(s => s.IsHidden == true)
            };

            return model;
        }


        [HttpPost]
        public async Task<ActionResult<Result>> RevokeExamine(RevokeExamineModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //查找审核
            var examine = await _examineRepository.FirstOrDefaultAsync(s => s.TagId == model.Id && s.ApplicationUserId == user.Id && s.Operation == model.ExamineType && s.IsPassed == null);
            if (examine != null)
            {
                await _examineRepository.DeleteAsync(examine);
                //删除以此审核为前置审核的
                await _examineRepository.DeleteAsync(s => s.PrepositionExamineId == examine.Id);
                return new Result { Successful = true };
            }
            else
            {
                return new Result { Successful = false, Error = "找不到目标审核记录" };
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenTagAsync(HiddenTagModel model)
        {
            await _tagRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EditTagInforBindModel>> GetTagEditInforBindModelAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var tag = await _tagRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (tag == null)
            {
                return NotFound("无法找到该标签");
            }
            var model = new EditTagInforBindModel
            {
                Id = id,
                Name = tag.Name
            };

            //获取编辑记录
            model.Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.TagId == id && s.IsPassed == true), true);
            model.Examines = model.Examines.OrderByDescending(s => s.ApplyTime).ToList();
            //获取编辑状态
            model.State = await _tagService.GetTagEditState(user, id);

            return model;
        }
    }
}
