using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Others
{
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

    public class EChartsRadarOptionModel 
    {
        public EChartsOptionTooltip Tooltip { get; set; } = new EChartsOptionTooltip
        {
            Trigger= "item"
        };
        public EChartsRadarOptionLegend Legend { get; set; } = new EChartsRadarOptionLegend();

        public EChartsRadarOptionRadar Radar { get; set; } = new EChartsRadarOptionRadar();
        public List<EChartsRadarOptionSery> Series { get; set; } = new List<EChartsRadarOptionSery>();
    }

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


    public class EChartsRadarOptionRadar
    {
        public List<EChartsRadarOptionIndicator> Indicator { get; set; } = new List<EChartsRadarOptionIndicator>();
    }

    public class EChartsRadarOptionIndicator
    {
        public string Name { get; set; }
        public double Max { get; set; } = 10;
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
}
