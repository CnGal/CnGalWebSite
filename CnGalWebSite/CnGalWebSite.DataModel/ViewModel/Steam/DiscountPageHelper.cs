using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class DiscountPageHelper
    {
        private List<SteamInforTipViewModel> Model = new List<SteamInforTipViewModel>();
        public List<SteamInforTipViewModel> Items = new List<SteamInforTipViewModel>();

        public List<int> PurchasedGames=new List<int>();

        public int TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 12;

        public int TotalPages => ((Items.Count-1) / MaxCount) + 1;

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

        public SteamSortType thenOrderType;
        public SteamSortType ThenOrderType
        {
            get => thenOrderType;
            set
            {
                thenOrderType = value;
                SetItems();
            }
        }

        public SteamDisplayType steamDisplayType { get; set; }

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
            switch (ScreenType)
            {
                case ScreenSteamType.All:
                    Items = Model;
                    break;
                case ScreenSteamType.NewHistoryLow:
                    Items = Model.Where(s => s.CutNow > s.CutLowest && s.CutLowest > 0).ToList();
                    break;
                case ScreenSteamType.FlatHistoryLow:
                    Items = Model.Where(s => s.CutNow == s.CutLowest && s.CutLowest > 0).ToList();
                    break;
            };

            switch (PurchasedType)
            {
                case PurchasedSteamType.All:
                    Items = Model;
                    break;
                case PurchasedSteamType.Purchased:
                    Items = Model.Where(s => PurchasedGames.Contains(s.Id)).ToList();
                    break;
                case PurchasedSteamType.UnPurchased:
                    Items = Model.Where(s => PurchasedGames.Contains(s.Id)==false).ToList();
                    break;
            };


            switch (OrderType)
            {

                case SteamSortType.EvaluationCount:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.EvaluationCount).ToList();
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.RecommendationRate).ToList();
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.PublishTime).ToList();
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.CutNow).ToList();
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.EvaluationCount).ThenByDescending(s => s.PriceNow).ToList();
                            break;
                    }
                    break;
                case SteamSortType.RecommendationRate:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.EvaluationCount).ToList();
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.RecommendationRate).ToList();
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.PublishTime).ToList();
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.CutNow).ToList();
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.RecommendationRate).ThenByDescending(s => s.PriceNow).ToList();
                            break;

                    }
                    break;
                case SteamSortType.PublishTime:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.EvaluationCount).ToList();
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.RecommendationRate).ToList();
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.PublishTime).ToList();
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.CutNow).ToList();
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.PublishTime).ThenByDescending(s => s.PriceNow).ToList();
                            break;

                    }
                    break;
                case SteamSortType.Discount:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.EvaluationCount).ToList();
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.RecommendationRate).ToList();
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.PublishTime).ToList();
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.CutNow).ToList();
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.CutNow).ThenByDescending(s => s.PriceNow).ToList();
                            break;

                    }
                    break;
                case SteamSortType.Price:
                    switch (ThenOrderType)
                    {
                        case SteamSortType.EvaluationCount:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.EvaluationCount).ToList();
                            break;
                        case SteamSortType.RecommendationRate:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.RecommendationRate).ToList();
                            break;
                        case SteamSortType.PublishTime:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.PublishTime).ToList();
                            break;
                        case SteamSortType.Discount:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.CutNow).ToList();
                            break;
                        case SteamSortType.Price:
                            Items = Items.OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.PriceNow).ToList();
                            break;

                    }
                    break;
            };
        }
    }
}
