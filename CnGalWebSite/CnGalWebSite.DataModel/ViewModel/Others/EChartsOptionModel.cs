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

    #region 折线图 基类
    public class EChartsOptionTitle
    {
        public string Left { get; set; } = "center";

        public string Text { get; set; }
    }
    public class EChartsOptionTooltip
    {
        public string Trigger { get; set; } = "axis";
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

}
