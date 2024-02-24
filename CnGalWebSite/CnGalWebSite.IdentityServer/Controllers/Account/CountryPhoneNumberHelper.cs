using Microsoft.AspNetCore.Mvc.Rendering;

namespace CnGalWebSite.IdentityServer.Models.InputModels.Account
{
    public static class CountryPhoneNumberHelper
    {
        public static List<SelectListItem> Countries { get; } =
        [
            new SelectListItem { Value = "86", Text = "中国大陆 +86" },
            new SelectListItem { Value = "852", Text = "中国澳门 +852" },
            new SelectListItem { Value = "853", Text = "中国澳门 +853" },
            new SelectListItem { Value = "886", Text = "中国台湾 +886"  },
            new SelectListItem { Value = "1", Text = "美国 +1"  },
            new SelectListItem { Value = " 1", Text = "加拿大 +1"  },
            new SelectListItem { Value = "7", Text = "俄罗斯 +7"  },
            new SelectListItem { Value = "31", Text = "荷兰 +31"  },
            new SelectListItem { Value = "33", Text = "法国 +33"  },
            new SelectListItem { Value = "34", Text = "西班牙 +34"  },
            new SelectListItem { Value = "39", Text = "意大利 +39"  },
            new SelectListItem { Value = "44", Text = "英国 +44"  },
            new SelectListItem { Value = "49", Text = "德国 +49"  },
            new SelectListItem { Value = "60", Text = "马来西亚 +60"  },
            new SelectListItem { Value = "61", Text = "澳大利亚 +61"  },
            new SelectListItem { Value = "63", Text = "菲律宾 +63"  },
            new SelectListItem { Value = "65", Text = "新加坡 +65"  },
            new SelectListItem { Value = "66", Text = "泰国 +66"  },
            new SelectListItem { Value = "81", Text = "日本 +81"  },
            new SelectListItem { Value = "82", Text = "韩国 +82"  },
            new SelectListItem { Value = "84", Text = "越南 +84"  },
            new SelectListItem { Value = "91", Text = "印度 +91"  },
            new SelectListItem { Value = "380", Text = "乌克兰 +380"  },
        ];
    }
}
