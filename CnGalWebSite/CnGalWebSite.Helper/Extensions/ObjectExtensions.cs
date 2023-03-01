using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.Extensions
{
    public static class ObjectExtensions
    {
        static public void SynchronizationProperties(this object des,object src )
        {
            Type srcType = src.GetType();
            object val;
            foreach (var item in srcType.GetProperties())
            {
                val = item.GetValue(src);
                item.SetValue(des, val);

            }
        }
    }
}
