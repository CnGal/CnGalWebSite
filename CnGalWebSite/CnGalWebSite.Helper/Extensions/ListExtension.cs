using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.Extensions
{
    public static class ListExtension
    {
        public static List<T> Random<T>(this List<T> sources)
        {
            Random rd = new Random();
            int index = 0;
            T temp;
            for (int i = 0; i < sources.Count; i++)
            {
                index = rd.Next(0, sources.Count - 1);
                if (index != i)
                {
                    temp = sources[i];
                    sources[i] = sources[index];
                    sources[index] = temp;
                }
            }
            return sources;
        }
    }
}
