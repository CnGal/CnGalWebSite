using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class DiscountPageHelper
    {
        public List<SteamInforTipViewModel> Model = new List<SteamInforTipViewModel>();
        public IEnumerable<SteamInforTipViewModel> Items = new List<SteamInforTipViewModel>();

        public List<int> PurchasedGames=new List<int>();

        public int TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 12;

        public int TotalPages => ((Items.Count()-1) / MaxCount) + 1;

        public int CurrentPage { get; set; } = 1;

        private PurchasedSteamType purchasedType;
        public PurchasedSteamType PurchasedType
        {
            get => purchasedType;
            set
            {
                purchasedType = value;
                SetItems();
            }
        }

        private ScreenSteamType screenType;
        public ScreenSteamType ScreenType
        {
            get => screenType;
            set
            {
                screenType = value;
                SetItems();
            }
        }
        private SteamSortType orderType;
        public SteamSortType OrderType
        {
            get => orderType;
            set
            {
                orderType = value;
                SetItems();
            }
        }

        private SteamSortType thenOrderType;
        public SteamSortType ThenOrderType
        {
            get => thenOrderType;
            set
            {
                thenOrderType = value;
                SetItems();
            }
        }

        public SteamDisplayType SteamDisplayType { get; set; }

        public bool ShowAdvancedOptions { get; set; }

        private bool showNoDiscountGames;
        public bool ShowNoDiscountGames
        {
            get => showNoDiscountGames;
            set
            {
                showNoDiscountGames = value;
                SetItems();
            }
        }

        public const double MaxOriginalPriceLimit= 55;

        private double maxOriginalPrice = MaxOriginalPriceLimit;
        public double MaxOriginalPrice
        {
            get => maxOriginalPrice;
            set
            {
                maxOriginalPrice = value;
                SetItems();
            }
        }

        public bool IsInit => Model.Count > 0;

        public DiscountPageHelper()
        {
            ThenOrderType = SteamSortType.RecommendationRate;
        }

        public void Init(List<SteamInforTipViewModel> model)
        {
            Model = model;
            SetItems();
        }

        public void SetItems()
        {
            //显示未打折的游戏
            if(ShowNoDiscountGames)
            {
                Items = Model;
            }
            else
            {
                Items = Model.Where(s => s.CutNow > 0);
            }

            //筛选价格区间
            if (maxOriginalPrice < MaxOriginalPriceLimit && maxOriginalPrice >= 0)
            {
                Items = Items.Where(s => s.OriginalPrice <= maxOriginalPrice*100);
            }


            switch (ScreenType)
            {
                case ScreenSteamType.All:
                    //Items = Items;
                    break;
                case ScreenSteamType.NewHistoryLow:
                    Items = Items.Where(s => s.CutNow > s.CutLowest && s.CutLowest > 0);
                    break;
                case ScreenSteamType.FlatHistoryLow:
                    Items = Items.Where(s => s.CutNow == s.CutLowest && s.CutLowest > 0);
                    break;
            };

            switch (PurchasedType)
            {
                case PurchasedSteamType.All:
                    //Items = Items;
                    break;
                case PurchasedSteamType.Purchased:
                    Items = Items.Where(s => PurchasedGames.Contains(s.Id));
                    break;
                case PurchasedSteamType.UnPurchased:
                    Items = Items.Where(s => PurchasedGames.Contains(s.Id)==false);
                    break;
            };


            switch (OrderType)
            {

                case SteamSortType.EvaluationCount:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.EvaluationCount);
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.RecommendationRate);
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.PublishTime);
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.CutNow);
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.PriceNow);
                            break;
                    }
                    break;
                case SteamSortType.RecommendationRate:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.EvaluationCount);
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.RecommendationRate);
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.PublishTime);
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.CutNow);
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.PriceNow);
                            break;

                    }
                    break;
                case SteamSortType.PublishTime:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.EvaluationCount);
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.RecommendationRate);
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.PublishTime);
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.CutNow);
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.PriceNow);
                            break;

                    }
                    break;
                case SteamSortType.Discount:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.EvaluationCount);
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.RecommendationRate);
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.PublishTime);
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.CutNow);
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.PriceNow);
                            break;

                    }
                    break;
                case SteamSortType.Price:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.EvaluationCount);
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.RecommendationRate);
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.PublishTime);
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.CutNow);
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.PriceNow);
                            break;

                    }
                    break;
            };
        }
    }
}
