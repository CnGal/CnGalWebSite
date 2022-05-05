using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.Helper.Extensions
{
    public static class SearchTypeExtension
    {
        public static T ToOriginalType<T>(this SearchType model) where T : Enum
        {

            return model switch
            {
                SearchType.Game => (T)Enum.ToObject(typeof(T), 0),
                SearchType.Role => (T)Enum.ToObject(typeof(T), 1),
                SearchType.ProductionGroup => (T)Enum.ToObject(typeof(T), 2),
                SearchType.Staff => (T)Enum.ToObject(typeof(T), 3),

                SearchType.Tought => (T)Enum.ToObject(typeof(T), 0),
                SearchType.Strategy => (T)Enum.ToObject(typeof(T), 1),
                SearchType.Interview => (T)Enum.ToObject(typeof(T), 2),
                SearchType.News => (T)Enum.ToObject(typeof(T), 3),
                SearchType.Evaluation => (T)Enum.ToObject(typeof(T), 4),
                SearchType.PeripheralArticle => (T)Enum.ToObject(typeof(T), 5),
                SearchType.Notice => (T)Enum.ToObject(typeof(T), 6),
                SearchType.OtherArticle => (T)Enum.ToObject(typeof(T), 7),
                SearchType.Fan => (T)Enum.ToObject(typeof(T), 8),

                SearchType.SetorAlbumEtc => (T)Enum.ToObject(typeof(T), 0),
                SearchType.Ost => (T)Enum.ToObject(typeof(T), 1),
                SearchType.Set => (T)Enum.ToObject(typeof(T), 2),
                SearchType.OtherPeriphery => (T)Enum.ToObject(typeof(T), 3),
                _ => throw new NotImplementedException()
            };
        }

        public static List<int> ToTypeList(this SearchType model)
        {

            return model switch
            {
                SearchType.Entry => Enumerable.Range(1, 4).ToList(),
                SearchType.Article => Enumerable.Range(6, 9).ToList(),
                SearchType.Periphery => Enumerable.Range(16, 4).ToList(),
                SearchType.Tag => Enumerable.Range(20, 1).ToList(),
                _ => throw new NotImplementedException()
            };
        }

        public static SearchType ToSearchType(this ArticleType model)
        {
            return model switch
            {
                ArticleType.Tought => SearchType.Tought,
                ArticleType.Strategy => SearchType.Strategy,
                ArticleType.Interview => SearchType.Interview,
                ArticleType.News => SearchType.News,
                ArticleType.Evaluation => SearchType.Evaluation,
                ArticleType.Peripheral => SearchType.PeripheralArticle,
                ArticleType.Notice => SearchType.Notice,
                ArticleType.None => SearchType.OtherArticle,
                ArticleType.Fan => SearchType.Fan,
                _ => throw new NotImplementedException()
            };
        }

        public static SearchType ToSearchType(this EntryType model)
        {
            return model switch
            {
                EntryType.Game => SearchType.Game,
                EntryType.Role => SearchType.Role,
                EntryType.ProductionGroup => SearchType.ProductionGroup,
                EntryType.Staff => SearchType.Staff,
                _ => throw new NotImplementedException()
            };
        }

        public static SearchType ToSearchType(this PeripheryType model)
        {
            return model switch
            {
                PeripheryType.SetorAlbumEtc => SearchType.SetorAlbumEtc,
                PeripheryType.Ost => SearchType.Ost,
                PeripheryType.Set => SearchType.Set,
                PeripheryType.None => SearchType.OtherPeriphery,
                _ => throw new NotImplementedException()
            };
        }
    }


}
