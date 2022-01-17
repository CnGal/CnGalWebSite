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
        private readonly IRepository<ApplicationUser, string> _userRepository;

        public HistoryDataService(IHttpClientFactory clientFactory, IConfiguration configuration, IFileService fileService, IWebHostEnvironment webHostEnvironment, IAppHelper appHelper,
            IRepository<Entry, int> entryRepository, IExamineService examineService, IRepository<ApplicationUser, string> userRepository)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _fileService = fileService;
            _webHostEnvironment = webHostEnvironment;
            _appHelper = appHelper;
            _entryRepository = entryRepository;
            _examineService = examineService;
            _userRepository = userRepository;
            //InitImages();
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
