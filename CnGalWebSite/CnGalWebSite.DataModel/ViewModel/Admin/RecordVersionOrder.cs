using System;
using System.Collections.Generic;
using System.Linq;
using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public static class RecordVersionOrder
    {
        public static DateTime Time(Examine record) => record.PassedTime ?? record.ApplyTime;

        public static int Compare(Examine left, Examine right)
        {
            var time = Time(left).CompareTo(Time(right));
            return time != 0 ? time : left.Id.CompareTo(right.Id);
        }

        public static IEnumerable<Examine> Approved(IEnumerable<Examine> records) => records
            .Where(r => r.IsPassed == true)
            .OrderBy(Time)
            .ThenBy(r => r.Id);
    }
}
