using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.Model
{
    public class SearchCache
    {
        public string Id { get; set; }

        [Key]
        public long SearchCacheId { get; set; }

        public long OriginalId { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string AnotherName { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainPage { get; set; }

        public int Type { get; set; }

        public int OriginalType { get; set; }

        public long LastEditTime { get; set; }

        public long CreateTime { get; set; }

        public long PubulishTime { get; set; }

        public int ReaderCount { get; set; }

        public void Copy(Entry model)
        {
            Id = SearchCacheId.ToString();
            AnotherName = model.AnotherName??"";
            DisplayName = model.DisplayName ?? "";
            LastEditTime = model.LastEditTime.ToBinary();
            Name = model.Name ?? "";
            Type = 0;
            OriginalType =(int) model.Type;
            OriginalId = model.Id;
            PubulishTime = model.PubulishTime?.ToBinary() ?? 0;
            ReaderCount = model.ReaderCount;
            BriefIntroduction = model.BriefIntroduction ?? "";
            MainPage = model.MainPage ?? "";
        }

        public void Copy(Article model)
        {
            Id = SearchCacheId.ToString();
            AnotherName = "";
            DisplayName = model.DisplayName ?? "";
            LastEditTime = model.LastEditTime.ToBinary();
            Name = model.Name ?? "";
            Type = 1;
            OriginalType = (int)model.Type;
            OriginalId = model.Id;
            ReaderCount = model.ReaderCount;
            BriefIntroduction = model.BriefIntroduction ?? "";
            MainPage = model.MainPage ?? "";
            CreateTime = model.CreateTime.ToBinary();
        }

        public void Copy(Periphery model)
        {
            Id = SearchCacheId.ToString();
            AnotherName = "";
            DisplayName = model.DisplayName ?? "";
            LastEditTime = model.LastEditTime.ToBinary();
            Name = model.Name ?? "";
            Type = 2;
            OriginalType = (int)model.Type;
            OriginalId = model.Id;
            ReaderCount = model.ReaderCount;
            BriefIntroduction = model.BriefIntroduction ?? "";
            MainPage = "";
        }

        public void Copy(Tag model)
        {
            Id = SearchCacheId.ToString();
            AnotherName =  "";
            DisplayName =  "";
            LastEditTime = model.LastEditTime.ToBinary();
            Name = model.Name ?? "";
            Type = 3;
            OriginalId = model.Id;
            ReaderCount = model.ReaderCount;
            BriefIntroduction = model.BriefIntroduction ?? "";
            MainPage ="";
        }


    }
}
