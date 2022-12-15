using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.Helper.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.Application.Search.Dtos
{
    public class SearchInputModel : PagedSortedAndFilterInput
    {
        public SearchInputModel()
        {
            Sorting = "";
        }

        /// <summary>
        /// 类型过滤 对自身‘或’的关系 对其他条件‘且’的关系
        /// </summary>
        public List<SearchType> Types { get; set; } = new List<SearchType>();
        /// <summary> 
        /// 时间过滤
        /// </summary>
        public List<SearchTimeModel> Times { get; set; } = new List<SearchTimeModel>();

        public string ToQueryParameterString()
        {
            var sb = new StringBuilder();
            var temp = new List<string>();

            if (string.IsNullOrWhiteSpace(FilterText) == false)
            {
                temp.Add("Text=" + FilterText);
            }
            if (string.IsNullOrWhiteSpace(Sorting) == false)
            {
                temp.Add("Sort=" + Sorting);
            }
            if (CurrentPage != 1)
            {
                temp.Add("Page=" + CurrentPage);
            }

            if (Types.Count != 0)
            {
                foreach (var item in Types)
                {
                    temp.Add("Types=" + item.ToString());
                }
            }
            if (Times.Count != 0)
            {
                foreach (var item in Times)
                {
                    temp.Add("Times=" + item.ToString());
                }
            }

            if (temp.Count == 0)
            {
                return "";
            }
            else
            {
                sb.Append('?');
                foreach (var item in temp)
                {
                    sb.Append(item);
                    if (temp.IndexOf(item) != temp.Count - 1)
                    {
                        sb.Append('&');
                    }
                }
            }

            return sb.ToString();
        }

        public static List<SearchType> GetEntryTypes()
        {
            return new List<SearchType>
            {
                 SearchType.Entry,
                 SearchType.Game,
                 SearchType.Role,
                 SearchType.ProductionGroup,
                 SearchType.Staff,
            };
        }

        public static List<SearchType> GetArticleTypes()
        {
            return new List<SearchType>
            {
                 SearchType.Article,
                 SearchType.Tought,
                 SearchType.Strategy,
                 SearchType.Interview,
                 SearchType.News,
                 SearchType.Evaluation,
                 SearchType.PeripheralArticle,
                 SearchType.Notice,
                 SearchType.OtherArticle,
                 SearchType.Fan,
            };
        }


        public static List<SearchType> GetPeripheryTypes()
        {
            return new List<SearchType>
            {
                 SearchType.Periphery,
                 SearchType.SetorAlbumEtc,
                 SearchType.Ost,
                 SearchType.Set,
                 SearchType.ActivationCode,
                 SearchType.ColoredPaper,
                 SearchType.Badge,
                 SearchType.Postcard,
                 SearchType.HangPainting,
                 SearchType.Keychain,
                 SearchType.Bookmark,
                 SearchType.OtherPeriphery,
            };
        }

        public static List<SearchType> GetTagTypes()
        {
            return new List<SearchType>
            {
                 SearchType.Tag
            };
        }

        public static List<SearchType> GetVideoTypes()
        {
            return new List<SearchType>
            {
                 SearchType.Video
            };
        }


        public static List<SearchTimeModel> GetTimes()
        {
            return new List<SearchTimeModel>
            {
               new SearchTimeModel
               {
                   AfterTime=DateTime.ParseExact("2018-01-01","yyyy-MM-dd",null),
                   BeforeTime=DateTime.ParseExact("2019-01-01","yyyy-MM-dd",null),
                   Note="2018"
               },
                new SearchTimeModel
               {
                   AfterTime=DateTime.ParseExact("2019-01-01","yyyy-MM-dd",null),
                   BeforeTime=DateTime.ParseExact("2020-01-01","yyyy-MM-dd",null),
                   Note="2019"
               },
               new SearchTimeModel
               {
                   AfterTime=DateTime.ParseExact("2020-01-01","yyyy-MM-dd",null),
                   BeforeTime=DateTime.ParseExact("2021-01-01","yyyy-MM-dd",null),
                   Note="2020"
               },
               new SearchTimeModel
               {
                   AfterTime=DateTime.ParseExact("2021-01-01","yyyy-MM-dd",null),
                   BeforeTime=DateTime.ParseExact("2022-01-01","yyyy-MM-dd",null),
                   Note="2021"
               },
               new SearchTimeModel
               {
                   AfterTime=DateTime.ParseExact("2022-01-01","yyyy-MM-dd",null),
                   BeforeTime=DateTime.ParseExact("2023-01-01","yyyy-MM-dd",null),
                   Note="2022"
               },
            };
        }


        public static SearchInputModel Parse(string[] Types, string[] Times, string Text, string Sort, int Page)
        {
            var model = new SearchInputModel();
            if (TryParse(Types, Times, Text, Sort, Page, model))
            {
                return model;
            }
            else
            {
                throw new ArgumentException();
            }
        }
        public static bool TryParse(string[] Types, string[] Times, string Text, string Sort, int Page, SearchInputModel model)
        {
            try
            {
                //解析查询字符串
                if (Types != null && Types.Length != 0)
                {
                    model.Types.Clear();
                    foreach (var item in Types)
                    {
                        try
                        {
                            model.Types.Add((SearchType)Enum.Parse(typeof(SearchType), item, true));
                        }
                        catch
                        {

                        }
                    }
                }
                //解析时间
                if (Times!=null&& Times.Length != 0)
                {
                    model.Times.Clear();
                    foreach (var item in Times)
                    {
                        try
                        {
                            model.Times.Add(SearchTimeModel.Parse(item));
                        }
                        catch
                        {

                        }
                    }
                }
                model.Sorting = Sort ?? "";
                model.CurrentPage = Page == 0 ? 1 : Page;
                model.FilterText = Text ?? "";

                return true;

            }
            catch
            {
                return false;
            }

        }
    }

    public class SearchTimeModel
    {
        public DateTime AfterTime { get; set; }

        public DateTime BeforeTime { get; set; }

        public string Note { get; set; }

        public bool IsSame(SearchTimeModel model)
        {
            if (AfterTime == model.AfterTime && BeforeTime == model.BeforeTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static SearchTimeModel Parse(string Text)
        {
            var model = new SearchTimeModel();
            if (TryParse(Text, model))
            {
                return model;
            }
            else
            {
                throw new ArgumentException();
            }
        }
        public static bool TryParse(string Text, SearchTimeModel model)
        {
            try
            {
                var times = Text.Split('-');
                model.AfterTime = times[0].TransTime();
                model.BeforeTime = times[1].TransTime();

                return true;

            }
            catch
            {
                return false;
            }

        }

        public override string ToString()
        {
            return $"{AfterTime.ToUnixTimeMilliseconds()}-{BeforeTime.ToUnixTimeMilliseconds()}";
        }

    }

    public enum SearchType
    {
        [Display(Name = "词条")]
        Entry,
        [Display(Name = "游戏")]
        Game,
        [Display(Name = "角色")]
        Role,
        [Display(Name = "制作组")]
        ProductionGroup,
        [Display(Name = "STAFF")]
        Staff,

        [Display(Name = "文章")]
        Article,
        [Display(Name = "感想")]
        Tought,
        [Display(Name = "攻略")]
        Strategy,
        [Display(Name = "访谈")]
        Interview,
        [Display(Name = "动态")]
        News,
        [Display(Name = "评测")]
        Evaluation,
        [Display(Name = "周边文章")]
        PeripheralArticle,
        [Display(Name = "公告")]
        Notice,
        [Display(Name = "杂谈")]
        OtherArticle,
        [Display(Name = "二创")]
        Fan,

        [Display(Name = "周边")]
        Periphery,
        [Display(Name = "设定集或画册等")]
        SetorAlbumEtc,
        [Display(Name = "原声集")]
        Ost,
        [Display(Name = "套装")]
        Set,
        [Display(Name = "其他周边")]
        OtherPeriphery,

        [Display(Name = "标签")]
        Tag,

        [Display(Name = "视频")]
        Video,

        [Display(Name = "激活码")]
        ActivationCode,
        [Display(Name = "色纸")]
        ColoredPaper,
        [Display(Name = "徽章")]
        Badge,
        [Display(Name = "明信片")]
        Postcard,
        [Display(Name = "挂画")]
        HangPainting,
        [Display(Name = "挂件")]
        Keychain,
        [Display(Name = "书签")]
        Bookmark,
        [Display(Name = "海报")]
        Posters,
        [Display(Name = "光盘")]
        CD,
    }

}
