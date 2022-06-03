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
        public string Right { get; set; } = "20px";
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
}
