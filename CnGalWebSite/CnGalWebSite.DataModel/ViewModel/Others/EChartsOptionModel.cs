using System;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Others
{
    /// <summary>
    /// 折线图
    /// </summary>
    [Serializable]
    public class EChartsOptionModel
    {
        public EChartsOptionTitle Title { get; set; } = new EChartsOptionTitle();
        public EChartsOptionTooltip Tooltip { get; set; } = new EChartsOptionTooltip();
        public EChartsOptionLegend Legend { get; set; } = new EChartsOptionLegend();
        public EChartsOptionXAxis XAxis { get; set; } = new EChartsOptionXAxis();
        public EChartsOptionYAxis YAxis { get; set; } = new EChartsOptionYAxis();
        public List<EChartsOptionSery> Series { get; set; } = new List<EChartsOptionSery>();
    }

    /// <summary>
    /// 雷达图
    /// </summary>
    public class EChartsRadarOptionModel
    {
        public EChartsOptionTooltip Tooltip { get; set; } = new EChartsOptionTooltip
        {
            Trigger = "item"
        };
        public EChartsRadarOptionLegend Legend { get; set; } = new EChartsRadarOptionLegend();

        public EChartsRadarOptionRadar Radar { get; set; } = new EChartsRadarOptionRadar();
        public List<EChartsRadarOptionSery> Series { get; set; } = new List<EChartsRadarOptionSery>();
    }

    /// <summary>
    /// 矩形树图
    /// </summary>
    public class EChartsTreeMapOptionModel
    {
        public List<EChartsTreeMapOptionSery> Series { get; set; } = new List<EChartsTreeMapOptionSery>();
        public EChartsOptionTooltip Tooltip { get; set; } = new EChartsOptionTooltip
        {
            Trigger = "item"
        };

    }

    /// <summary>
    /// 饼图
    /// </summary>
    public class EChartsPieOptionModel
    {
        public EChartsOptionTitle Title { get; set; } = new EChartsOptionTitle();
        public EChartsOptionTooltip Tooltip { get; set; } = new EChartsOptionTooltip
        {
            Trigger = "item"
        };
        public EChartsPieOptionLegend Legend { get; set; } = new EChartsPieOptionLegend();
        public List<EChartsPieOptionSery> Series { get; set; } = new List<EChartsPieOptionSery>();
    }

    /// <summary>
    /// 热力图
    /// </summary>
    public class EChartsHeatMapOptionModel
    {
        public EChartsOptionTitle Title { get; set; } = new EChartsOptionTitle();
        public EChartsOptionTooltip Tooltip { get; set; } = new EChartsOptionTooltip
        {
            Trigger = null,
            Formatter= "{c0}"
        };
        public List<EChartsHeatMapOptionSery> Series { get; set; } = new List<EChartsHeatMapOptionSery>();
        public EChartsHeatMapOptionCalendar Calendar { get; set; } = new EChartsHeatMapOptionCalendar();
        public EChartsHeatMapOptionVisualMap VisualMap { get; set; } = new EChartsHeatMapOptionVisualMap();

        public EChartsHeatMapOptionModel()
        {
            SetTheme(false,false,true);
        }


        public void SetTheme(bool isApp, bool isDark, bool showScrollBar)
        {
            VisualMap.SetColor(isDark);
            VisualMap.Show = showScrollBar;
            if (isApp)
            {
                VisualMap.Top = 40;
                Calendar.Orient = "vertical";
                Calendar.Left = "center";
                Calendar.Top = showScrollBar ? 150:90;
                Calendar.CellSize = new List<object> { 15, "auto" };
            }
            else
            {
                VisualMap.Top = 190;
                Calendar.Orient = "horizontal";
                Calendar.Left = 30;
                Calendar.Top = 65;
                Calendar.CellSize = new List<object> { "auto", 15 };
            }
        }
    }

    /// <summary>
    /// 关系图
    /// </summary>
    public class EChartsGraphOptionModel
    {
        public List<EChartsTreeMapOptionSery> Series { get; set; } = new List<EChartsTreeMapOptionSery>();
        public EChartsOptionTooltip Tooltip { get; set; } = new EChartsOptionTooltip
        {
            Trigger = "item"
        };
    }

    #region 折线图 基类
    public class EChartsOptionTitle
    {
        public string Left { get; set; } = "center";
        public int Top { get; set; }
        public string Text { get; set; }
    }
    public class EChartsOptionTooltip
    {
        public string Trigger { get; set; } = "axis";
        public string Formatter { get; set; }
    }
    public class EChartsOptionLegend
    {
        public int Bottom { get; set; } = 10;
        public List<string> Data { get; set; } = new List<string>();
    }

    public class EChartsOptionXAxis
    {
        public List<string> Data { get; set; } = new List<string>();
    }

    public class EChartsOptionYAxis
    {

    }

    public class EChartsOptionSery
    {
        public string Name { get; set; }
        public string Type { get; set; } = "line";
        public string Stack { get; set; }
        public bool Smooth { get; set; } = true;
        public List<double> Data { get; set; } = new List<double>();
    }
    #endregion

    #region 饼图
  public class EChartsPieOptionSery
    {
        public string Name { get; set; }
        public string Type { get; set; } = "pie";
        public string Radius { get; set; } = "50%";
        public List<EChartsPieOptionSeryData> Data { get; set; } = new List<EChartsPieOptionSeryData>();
    }
    public class EChartsPieOptionSeryData
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class EChartsPieOptionLegend
    {
        public string Orient { get; set; } = "vertical";
        public string Left { get; set; } = "left";
    }
    #endregion

    #region 雷达图

    public class EChartsRadarOptionRadar
    {
        public List<EChartsRadarOptionIndicator> Indicator { get; set; } = new List<EChartsRadarOptionIndicator>();
    }

    public class EChartsRadarOptionIndicator
    {
        public string Name { get; set; }
        public double Max { get; set; } = 10;
        public double Min { get; set; } = 0;
    }
    public class EChartsRadarOptionSery
    {
        public string Name { get; set; }
        public string Type { get; set; } = "radar";
        public List<EChartsRadarOptionSeryData> Data { get; set; } = new List<EChartsRadarOptionSeryData>();
    }
    public class EChartsRadarOptionSeryData
    {
        public string Name { get; set; }
        public List<double> Value { get; set; } = new List<double>();
    }
    public class EChartsRadarOptionLegend
    {
        public string Type { get; set; } = "scroll";
        public int Bottom { get; set; } = 0;
        public List<string> Data { get; set; } = new List<string>();
    }
    #endregion

    #region 矩形树图
    public class EChartsTreeMapOptionSery
    {
        public string Name { get; set; }
        public string Type { get; set; } = "treemap";
        public int visibleMin { get; set; } = 100;
        public List<EChartsTreeMapOptionSeryData> Data { get; set; } = new List<EChartsTreeMapOptionSeryData>();
    }
    public class EChartsTreeMapOptionSeryData
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public List<EChartsTreeMapOptionSeryDataChildren> Children { get; set; } = new List<EChartsTreeMapOptionSeryDataChildren>();
    }
    public class EChartsTreeMapOptionSeryDataChildren
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public int Id;
        public List<EChartsTreeMapOptionSeryDataChildren> Children { get; set; } = new List<EChartsTreeMapOptionSeryDataChildren>();
    }
    #endregion

    #region 热力图
    public class EChartsHeatMapOptionSery
    {
        public string Type { get; set; } = "heatmap";
        public string CoordinateSystem { get; set; } = "calendar";
        public List<EChartsHeatMapOptionSeryData> Data { get; set; } = new List<EChartsHeatMapOptionSeryData>();

    }
    public class EChartsHeatMapOptionSeryData
    {
        public List< object> Value { get; set; } = new List<object>();
    }
    public class EChartsHeatMapOptionCalendar
    {
        public int Top { get; set; } = 120;
        public object Left { get; set; } = "center";
        public string Orient { get; set; } = "horizontal";
        public List<string> Range { get; set; } = new List<string>();
        public List<object> CellSize { get; set; } = new List<object> { "auto", 15};
    }

    public class EChartsHeatMapOptionVisualMap
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 10;
        public bool Calculable { get; set; } = true;
        public string Orient { get; set; } = "horizontal";
        public object Left { get; set; } = "center";
        public int Top { get; set; } = 35;
        public bool Show { get; set; } = true;
        public List<string> Color { get; set; }

        public EChartsHeatMapOptionVisualMap()
        {
            SetColor(false);
        }

        public void SetColor(bool isDark)
        {

            Color = isDark? new List<string> { "#39d353", "#26a641", "#006d32", "#0e4429" }: new List<string> { "#216e39", "#30a14e", "#40c463", "#9be9a8" };
        }
    }
    #endregion

}
