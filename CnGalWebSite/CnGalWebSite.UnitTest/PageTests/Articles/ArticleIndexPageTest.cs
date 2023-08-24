using CnGalWebSite.UnitTest.Base;
using CnGalWebSite.UnitTest.PageTests.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Articles
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ArticleIndexPageTest : BasePageTest
    {
        /// <summary>
        /// 验证页面
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task VerifyPage()
        {
            var page = new ArticleIndexPage(Page);
            await page.GotoAsync();
            await page.VerifyPage();
        }
    }
}
