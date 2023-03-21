using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Extensions
{
    public static class ObjectExtensions
    {
        static public void SynchronizationProperties(this object des, object src)
        {
            if (des == null ||src==null)
            {
                return;
            }
            Type srcType = src.GetType();
            Type desType = des.GetType();
            object val;
            foreach (var item in srcType.GetProperties().Where(s => desType.GetProperties().Select(s => s.Name).Contains(s.Name)))
            {
                val = item.GetValue(src);
                item.SetValue(des, val);
            }
        }
    }
}
