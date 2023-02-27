using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using System;

namespace CnGalWebSite.Shared.AppComponent.Normal.Edits
{
    public class EditButtonLineModel
    {

        public DateTime? LastEditTime { get; set; }

        public int EditCount { get; set; }

        public EditState State { get; set; }

        public Operation Operation { get; set; }

        public long Id { get; set; }

        public string Title => Operation switch
        {
            Operation.EstablishMain => "主要信息",
            Operation.EstablishMainPage => "介绍",
            Operation.EstablishRelevances => "关联信息",
            Operation.EstablishTags => "标签",
            Operation.EstablishAddInfor => "附加信息",
            Operation.EstablishImages => "相册",
            Operation.EstablishAudio => "音频",
            Operation.EstablishWebsite => "样式补充信息",
            Operation.EditPeripheryMain => "主要信息",
            Operation.EditPeripheryImages => "相册",
            Operation.EditPeripheryRelatedEntries => "关联词条",
            Operation.EditPeripheryRelatedPeripheries => "关联周边",
            Operation.EditTagMain => "主要信息",
            Operation.EditTagChildTags => "子标签",
            Operation.EditTagChildEntries => "子词条",
            Operation.EditArticleMain => "主要信息",
            Operation.EditArticleMainPage => "主页",
            Operation.EditArticleRelevanes => "关联信息",
            Operation.EditVideoMain => "主要信息",
            Operation.EditVideoMainPage => "主页",
            Operation.EditVideoRelevanes => "关联信息",
            Operation.EditVideoImages => "预览图",
            _ => "",
        };
        public string Link => Operation switch
        {
            Operation.EstablishMain => "entries/editmain/" + Id,
            Operation.EstablishMainPage => "entries/editmainpage/" + Id,
            Operation.EstablishRelevances => "entries/editrelevances/" + Id,
            Operation.EstablishTags => "entries/edittags/" + Id,
            Operation.EstablishAddInfor => "entries/editaddinfor/" + Id,
            Operation.EstablishImages => "entries/editimages/" + Id,
            Operation.EstablishAudio => "entries/editaudio/" + Id,
            Operation.EstablishWebsite => "entries/editwebsite/" + Id,
            Operation.EditPeripheryMain => "peripheries/editmain/" + Id,
            Operation.EditPeripheryImages => "peripheries/editimages/" + Id,
            Operation.EditPeripheryRelatedEntries => "peripheries/EditRelatedEntries/" + Id,
            Operation.EditPeripheryRelatedPeripheries => "peripheries/EditRelatedPeripheries/" + Id,
            Operation.EditTagMain => "tags/editmain/" + Id,
            Operation.EditTagChildTags => "tags/editchildtags/" + Id,
            Operation.EditTagChildEntries => "tags/editchildentries/" + Id,
            Operation.EditArticleMain => "articles/editmain/" + Id,
            Operation.EditArticleMainPage => "articles/editmainpage/" + Id,
            Operation.EditArticleRelevanes => "articles/editrelevances/" + Id,
            Operation.EditVideoMain => "videos/editmain/" + Id,
            Operation.EditVideoMainPage => "videos/editmainpage/" + Id,
            Operation.EditVideoRelevanes => "videos/editrelevances/" + Id,
            Operation.EditVideoImages => "videos/editimages/" + Id,
            _ => "",
        };
        public string Color => State switch
        {
            EditState.None => "bg-success",
            EditState.Preview => "bg-warning",
            EditState.Normal => "bg-primary",
            EditState.Locked => "bg-dark",
            _ => "",
        };
        public string Describe => Operation switch
        {
            Operation.EstablishMain => "主要信息包括：词条的名称，简介，主图......",
            Operation.EstablishMainPage => "介绍包括：游戏的剧情、人物的性格......等其他部分遗漏的信息，可以参考萌娘百科中关于作品和角色的条目，我们鼓励这种编辑形式",
            Operation.EstablishRelevances => "关联信息包括：相关的游戏，制作组、Staff、角色、文章、外部链接......请尽可能地填写此部分，我们会根据这一部分生成网站的信息图谱",
            Operation.EstablishTags => "填写标签有助于内容分类，方便其他用户查找",
            Operation.EstablishAddInfor => "不同类别的词条附加信息各不相同，比如游戏词条包括SteamId、发行时间、STAFF......这一部分会作为词条的基本信息展示",
            Operation.EstablishImages => "游戏CG，宣传图，角色立绘，制作组活动图，纪念图，周边展示图......都可以上传到相册里，当然不能涉及到剧透、侵权、违法等内容",
            Operation.EditPeripheryMain => "主要信息包括：周边的名称，简介，作者，材质，主图......",
            Operation.EditPeripheryImages => "周边相关图片都可以上传到相册里，但是仍在售卖的设定集，画册请只上传宣传图",
            Operation.EditPeripheryRelatedEntries => "关联词条包括这个周边相关的游戏，角色，STAFF，制作组......我们将词条相关周边以合集方式展示",
            Operation.EditPeripheryRelatedPeripheries => "关联周边主要用于套装，例如豪华版里包括钥匙扣，设定集......",
            Operation.EditTagMain => "主要信息包括：标签的名称，简介，主图......",
            Operation.EditTagChildTags => "标签拥有层级关系，可以为标签添加子标签",
            Operation.EditTagChildEntries => "这里可以批量为标签添加关联词条，前往词条编辑页面可以为词条批量添加标签",
            _ => "",
        };
    }
}
