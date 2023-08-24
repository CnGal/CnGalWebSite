using CnGalWebSite.UnitTest.Base;
using CnGalWebSite.UnitTest.PageTests.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Entries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class EntryIndexPageTest:BasePageTest
    {
        /// <summary>
        /// 验证页面
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task VerifyPage()
        {
            var page = new EntryIndexPage(Page);
            await page.GotoAsync();
            await page.VerifyPage();
        }
    }
}
