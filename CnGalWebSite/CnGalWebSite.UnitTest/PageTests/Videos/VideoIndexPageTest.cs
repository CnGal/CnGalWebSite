using CnGalWebSite.UnitTest.Base;
using CnGalWebSite.UnitTest.PageTests.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Videos
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class VideoIndexPageTest : BasePageTest
    {
        /// <summary>
        /// 验证页面
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task VerifyPage()
        {
            var page = new VideoIndexPage(Page);
            await page.GotoAsync();
            await page.VerifyPage();
        }
    }
}
