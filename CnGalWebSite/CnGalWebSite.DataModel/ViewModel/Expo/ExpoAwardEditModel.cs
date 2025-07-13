using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoAwardEditModel
    {
        public long Id { get; set; }

        public ExpoAwardType Type { get; set; }

        public int Count { get; set; }

        public string Image { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// 是否启用，未启用的奖项不参与抽奖
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public List<ExpoPrizeEditModel> Prizes { get; set; } = new List<ExpoPrizeEditModel>();

        public Result Validate()
        {
            if (Type == ExpoAwardType.NoEntry && Count <= 0)
            {
                return new Result { Successful = false, Error = "数量必须大于0" };
            }

            if (Type == ExpoAwardType.Key && (Prizes == null || Prizes.Count == 0))
            {
                return new Result { Successful = false, Error = "激活码类型必须至少有一个奖品" };
            }

            return new Result { Successful = true };
        }
    }

    public class ExpoPrizeEditModel
    {
        public long Id { get; set; }

        public string Content { get; set; }

        public bool Allocated { get; set; }
    }
}
