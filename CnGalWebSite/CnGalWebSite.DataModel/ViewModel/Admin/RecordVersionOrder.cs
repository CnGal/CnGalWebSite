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

        /// <summary>
        /// 回放应停在哪条记录之后：最后一个被请求版本在回放顺序（通过时间）里的位置。
        /// 不能用最大 Id——审核通过的顺序与 Id 顺序无关。
        /// </summary>
        public static int LastRequestedPosition(IReadOnlyList<Examine> records, IReadOnlyCollection<long> ids)
        {
            for (var position = records.Count - 1; position >= 0; position--)
            {
                if (ids.Contains(records[position].Id)) return position;
            }
            return -1;
        }
    }
}
