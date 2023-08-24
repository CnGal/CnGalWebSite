using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Home
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class HomePageTest:BasePageTest
    {
        /// <summary>
        /// 测试轮播图
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestCarousel()
        {
            var page = new HomePage(Page);
            await page.GotoAsync();
            await page.ClickCarousel();
            await page.SwitchCarousel();
        }

        /// <summary>
        /// 测试随机推荐
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestRandomRecomment()
        {
            var page = new HomePage(Page);
            await page.GotoAsync();
            await page.VerifyRandomRecommentGames();
            await page.SwitchRandomRecommentGames();
            await page.ClickRandomRecommentGames();
        }
    }
}
