using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.HistoryData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.DataModel.Models;

namespace CnGalWebSite.APIServer.Application.HistoryData
{
    public class HistoryDataService : IHistoryDataService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly IExamineService _examineService;
        private readonly IAppHelper _appHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly List<OriginalImageToDrawingBedUrl> _images;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, int> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<UserFile, string> _userFileRepository;

        private readonly string oldImageHost = "pic.cngal.top";
        private readonly string oldImageHost2 = "pic.sliots.top";

        public HistoryDataService(IHttpClientFactory clientFactory, IConfiguration configuration, IFileService fileService, IWebHostEnvironment webHostEnvironment, IAppHelper appHelper, IRepository<Periphery, long> peripheryRepository,
            IRepository<UserFile, string> userFileRepository,
        IRepository<Entry, int> entryRepository, IExamineService examineService, IRepository<ApplicationUser, string> userRepository, IRepository<Article, long> articleRepository, IRepository<Examine, int> examineRepository)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _fileService = fileService;
            _webHostEnvironment = webHostEnvironment;
            _appHelper = appHelper;
            _entryRepository = entryRepository;
            _examineService = examineService;
            _userRepository = userRepository;
            _articleRepository = articleRepository;
            _peripheryRepository = peripheryRepository;
            _examineRepository = examineRepository;
            _userFileRepository = userFileRepository;
        }

        public async Task Replace()
        {
            await ReplaceUserFile();
            //await ReplaceArticle();
            //await ReplacePeriphery();
            //await ReplaceUser();
            //await ReplaceExamine();
        }

        public async Task ReplaceUserFile()
        {
            var files = await _userFileRepository.GetAll()
                .Where(s => string.IsNullOrWhiteSpace(s.FileName) == false &&
                ( s.FileName.Contains(oldImageHost)|| s.FileName.Contains(oldImageHost2)))
                .ToListAsync();

            foreach (var file in files)
            {
                file.FileName = ReplaceString(file.FileName);

                await _userFileRepository.UpdateAsync(file);

                Console.WriteLine($"[{files.IndexOf(file) + 1}]替换完成文件图片[Id:{file.Id}]");
            }
            //https://pic.sliots.top
        }

        public async Task ReplaceEntry()
        {
            var entries = await _entryRepository.GetAll().Include(s=>s.Pictures)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false &&
                (s.Pictures.Any() || s.MainPicture.Contains(oldImageHost) || s.Thumbnail.Contains(oldImageHost) || s.BackgroundPicture.Contains(oldImageHost) || s.SmallBackgroundPicture.Contains(oldImageHost) || s.MainPage.Contains(oldImageHost)))
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.MainPicture = ReplaceString(entry.MainPicture);
                entry.Thumbnail = ReplaceString(entry.Thumbnail);
                entry.BackgroundPicture = ReplaceString(entry.BackgroundPicture);
                entry.SmallBackgroundPicture = ReplaceString(entry.SmallBackgroundPicture);
                entry.MainPage = ReplaceString(entry.MainPage);

                foreach (var item in entry.Pictures)
                {
                    item.Url = ReplaceString(item.Url);
                }
                await _entryRepository.UpdateAsync(entry);

                Console.WriteLine($"[{entries.IndexOf(entry) + 1}]替换完成词条图片[Id:{entry.Id}]");
            }
            //https://pic.sliots.top
        }

        public async Task ReplaceArticle()
        {
            var entries = await _articleRepository.GetAll()
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false &&
                (s.MainPicture.Contains(oldImageHost)  || s.BackgroundPicture.Contains(oldImageHost) || s.SmallBackgroundPicture.Contains(oldImageHost) || s.MainPage.Contains(oldImageHost)))
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.MainPicture = ReplaceString(entry.MainPicture);
                entry.BackgroundPicture = ReplaceString(entry.BackgroundPicture);
                entry.SmallBackgroundPicture = ReplaceString(entry.SmallBackgroundPicture);
                entry.MainPage = ReplaceString(entry.MainPage);

                await _articleRepository.UpdateAsync(entry);

                Console.WriteLine($"[{entries.IndexOf(entry) + 1}]替换完成文章图片[Id:{entry.Id}]");
            }
        }

        public async Task ReplacePeriphery()
        {
            var entries = await _peripheryRepository.GetAll().Include(s => s.Pictures)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false &&
                (s.Pictures.Any() || s.MainPicture.Contains(oldImageHost) || s.Thumbnail.Contains(oldImageHost) || s.BackgroundPicture.Contains(oldImageHost) || s.SmallBackgroundPicture.Contains(oldImageHost)))
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.MainPicture = ReplaceString(entry.MainPicture);
                entry.Thumbnail = ReplaceString(entry.Thumbnail);
                entry.BackgroundPicture = ReplaceString(entry.BackgroundPicture);
                entry.SmallBackgroundPicture = ReplaceString(entry.SmallBackgroundPicture);

                foreach (var item in entry.Pictures)
                {
                    item.Url = ReplaceString(item.Url);
                }


                await _peripheryRepository.UpdateAsync(entry);

                Console.WriteLine($"[{entries.IndexOf(entry) + 1}]替换完成周边图片[Id:{entry.Id}]");

            }
        }

        public async Task ReplaceUser()
        {
            var entries = await _userRepository.GetAll()
                 .Where(s=> s.PhotoPath.Contains(oldImageHost) || s.BackgroundImage.Contains(oldImageHost) || s.SBgImage.Contains(oldImageHost) || s.MBgImage.Contains(oldImageHost) || s.MainPageContext.Contains(oldImageHost))
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.PhotoPath = ReplaceString(entry.PhotoPath);
                entry.BackgroundImage = ReplaceString(entry.BackgroundImage);
                entry.SBgImage = ReplaceString(entry.SBgImage);
                entry.MBgImage = ReplaceString(entry.MBgImage);
                entry.MainPageContext = ReplaceString(entry.MainPageContext);

                await _userRepository.UpdateAsync(entry);

                Console.WriteLine($"[{entries.IndexOf(entry) + 1}]替换完成用户图片[Id:{entry.Id}]");

            }
        }

        public async Task ReplaceExamine()
        {
            var entries = await _examineRepository.GetAll()
                .Where(s => s.Context.Contains(oldImageHost))
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.Context = ReplaceString(entry.Context);

                await _examineRepository.UpdateAsync(entry);

                Console.WriteLine($"[{entries.IndexOf(entry) + 1}]替换完成审核图片[Id:{entry.Id}]");

            }
        }

        public string ReplaceString(string str)
        {
            return str?.Replace("pic.sliots.top", "image.cngal.org").Replace("pic.cngal.top", "image.cngal.org")?.Replace("http://image.cngal.org/", "https://image.cngal.org/");
        }
    }
    public class Icemic_Data
    {
        public Metadata Metadata { get; set; }

    }
    public class Metadata
    {
        public int cngalId { get; set; }
        public int bgmId { get; set; }
    }

    public class LevenshteinDistance
    {

        private static readonly LevenshteinDistance _instance = null;
        public static LevenshteinDistance Instance
        {
            get
            {
                if (_instance == null)
                {
                    return new LevenshteinDistance();
                }
                return _instance;
            }
        }


        /// <summary>
        /// 取最小的一位数
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <returns></returns>
        public int LowerOfThree(int first, int second, int third)
        {
            var min = first;
            if (second < min)
            {
                min = second;
            }

            if (third < min)
            {
                min = third;
            }

            return min;
        }

        public int Levenshtein_Distance(string str1, string str2)
        {
            int[,] Matrix;
            var n = str1.Length;
            var m = str2.Length;

            var temp = 0;
            char ch1;
            char ch2;
            var i = 0;
            var j = 0;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {

                return n;
            }
            Matrix = new int[n + 1, m + 1];

            for (i = 0; i <= n; i++)
            {
                //初始化第一列
                Matrix[i, 0] = i;
            }

            for (j = 0; j <= m; j++)
            {
                //初始化第一行
                Matrix[0, j] = j;
            }

            for (i = 1; i <= n; i++)
            {
                ch1 = str1[i - 1];
                for (j = 1; j <= m; j++)
                {
                    ch2 = str2[j - 1];
                    if (ch1.Equals(ch2))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 1;
                    }
                    Matrix[i, j] = LowerOfThree(Matrix[i - 1, j] + 1, Matrix[i, j - 1] + 1, Matrix[i - 1, j - 1] + temp);


                }
            }

            for (i = 0; i <= n; i++)
            {
                for (j = 0; j <= m; j++)
                {
                    // Console.Write(" {0} ", Matrix[i, j]);
                }
                //Console.WriteLine("");
            }
            return Matrix[n, m];

        }

        /// <summary>
        /// 计算字符串相似度
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public decimal LevenshteinDistancePercent(string str1, string str2)
        {
            if (str1 == "" || str2 == "")
            {
                return 0;
            }
            var maxLenth = str1.Length > str2.Length ? str1.Length : str2.Length;
            var val = Levenshtein_Distance(str1, str2);
            return 1 - (decimal)val / maxLenth;
        }
    }
}
