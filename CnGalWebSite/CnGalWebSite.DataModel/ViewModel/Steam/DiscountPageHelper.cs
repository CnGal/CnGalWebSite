using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class DiscountPageHelper
    {
        private List<SteamInforTipViewModel> Model = new List<SteamInforTipViewModel>();
        public List<SteamInforTipViewModel> Items = new List<SteamInforTipViewModel>();

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
