using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Videos
{
    public class VideoIndexPage : BasePage
    {
        public VideoIndexPage(IPage page) : base(page, "videos/index/1")
        {

        }

        public async Task VerifyPage()
        {
            await Expect(_page.Locator("h1").First).ToHaveTextAsync("全新CnGal资料站宣传CM——中文美少女游戏资料站", new() { Timeout = 30000 });
        }
    }
}
