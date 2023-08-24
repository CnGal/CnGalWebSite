using CnGalWebSite.UnitTest.Base;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace CnGalWebSite.UnitTest.PageTests.Home
{
    public class HomePage : BasePage
    {
        private readonly ILocator _carouselCard;
        private readonly ILocator _randomRecommentGamesCard;

        public HomePage(IPage page) : base(page, "home")
        {
            _carouselCard = _page.Locator(".carousels-card");
            _randomRecommentGamesCard = _page.Locator(".published-games-card").First;
        }


        /// <summary>
        /// 点击轮播图
        /// </summary>
        /// <returns></returns>
        public async Task ClickCarousel()
        {
            await _carouselCard.Locator("li").First.ClickAsync();
            var newPage = await _page.RunAndWaitForPopupAsync(async () =>
            {
                await _carouselCard.Locator(".carousel-item.active").GetByRole(AriaRole.Img).ClickAsync();
            });
            await newPage.CloseAsync();
        }

        /// <summary>
        /// 切换轮播图
        /// </summary>
        /// <returns></returns>
        public async Task SwitchCarousel()
        {
            var count = 6;
            string[] images = new string[count];
            //显示按钮
            await _carouselCard.HoverAsync();
            await Task.Delay(1000);
            //获取全部图片
            for (var i = 0; i < count; i++)
            {
                await _carouselCard.Locator($"li:nth-child({i + 1})").ClickAsync();
                await Task.Delay(700);
                images[i] = await _carouselCard.Locator(".carousel-item.active").GetByRole(AriaRole.Img).GetAttributeAsync("src") ?? "";
            }
            //显示按钮
            await _carouselCard.HoverAsync();
            await Task.Delay(1000);
            //回到第一格
            await _carouselCard.Locator("li").First.ClickAsync();
            //依次点击下一张并验证
            for (var i = 0; i < count; i++)
            {
                await Task.Delay(700);
                await Expect(_carouselCard.Locator(".carousel-item.active").GetByRole(AriaRole.Img)).ToHaveAttributeAsync("src", images[i]);
                await _carouselCard.Locator(".carousel-control-next").ClickAsync();
            }
        }

        /// <summary>
        /// 验证是否为随机推荐
        /// </summary>
        /// <returns></returns>
        public async Task VerifyRandomRecommentGames()
        {
            await Expect(_randomRecommentGamesCard.Locator("h2")).ToHaveTextAsync("随机推荐");

        }

        /// <summary>
        /// 换一批随机推荐
        /// </summary>
        /// <returns></returns>
        public async Task SwitchRandomRecommentGames()
        {
            var count = await _randomRecommentGamesCard.Locator(".item").CountAsync() - 1;
            string[] names = new string[count];

            //获取所有名称
            for (var i = 0; i < count; i++)
            {
                names[i] = await _randomRecommentGamesCard.Locator(".item").Nth(i).Locator(".name").TextContentAsync() ?? "";
            }

            //点击换一批
            await _randomRecommentGamesCard.Locator(".more-link").ClickAsync();
            await Task.Delay(1000);
            //再次获取所有名称
            var sameCount = 0;
            for (var i = 0; i < count; i++)
            {
                var name = await _randomRecommentGamesCard.Locator(".item").Nth(i).Locator(".name").TextContentAsync() ?? "";
                if (names.Any(s => s == name))
                {
                    sameCount++;
                }
            }

            //对比概率 是否随机
            Assert.That((sameCount * 100.0 / count), Is.LessThan(50));
        }

        /// <summary>
        /// 点击随机推荐
        /// </summary>
        /// <returns></returns>
        public async Task ClickRandomRecommentGames()
        {
            var card = _randomRecommentGamesCard.Locator(".item").Nth(0);
            var url = await card.GetAttributeAsync("href");
            var name = await card.Locator(".name").TextContentAsync() ?? "";
            await card.ClickAsync();
            await _page.WaitForURLAsync(_baseUrl + url);
            await Expect(_page.Locator("h1").First).ToHaveTextAsync(name, new() { Timeout = 30000 });
        }

     
    }
}
