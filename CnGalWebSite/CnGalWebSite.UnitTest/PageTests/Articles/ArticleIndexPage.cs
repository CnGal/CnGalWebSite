using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.UnitTest.PageTests.Articles
{
    public class ArticleIndexPage:BasePage
    {
        public ArticleIndexPage(IPage page) : base(page, "articles/index/1")
        {

        }

        public async Task VerifyPage()
        {
            await Expect(_page.Locator("h1").First).ToHaveTextAsync("CnGal新版网站架构与开放API功能概述", new() { Timeout=30000});
        }
    }
}
