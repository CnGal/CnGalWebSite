using CnGalWebSite.UnitTest.Base;
using CnGalWebSite.UnitTest.PageTests.Videos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Tags
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    internal class TagIndexPageTest : BasePageTest
    {
        /// <summary>
        /// 验证页面
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task VerifyPage()
        {
            var page = new TagIndexPage(Page);
            await page.GotoAsync();
            await page.VerifyPage();
        }
    }
}
