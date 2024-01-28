using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Stores
{
    public class GamalyticDataModel
    {
        public int ReviewsSteam { get; set; }
        public double ReviewScore { get; set; }
        public double AvgPlaytime { get; set; }
        public bool Unreleased { get; set; }
        public int Owners { get; set; }
        public int Revenue { get; set; }
        public int Players { get; set; }
        public double Accuracy { get; set; }
    }
}
