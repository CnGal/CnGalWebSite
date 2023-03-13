using System.Text;

namespace CnGalWebSite.Extensions
{
    public static class ListExtension
    {
        public static List<T> Random<T>(this List<T> sources)
        {
            var rd = new Random();
            var index = 0;
            T temp;
            for (var i = 0; i < sources.Count; i++)
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


        /// <summary>
        /// string 的 列表 清理相同项目
        /// </summary>
        /// <param name="informations"></param>
        /// <returns></returns>
        public static List<string> Purge(this List<string> informations)
        {
            var list = informations.ToList();
            foreach (var item in list)
            {
                if (informations.Count(s => item == s) > 1)
                {
                    var temp = informations.FirstOrDefault(s => item == s);
                    if (temp != null)
                    {
                        informations.Remove(temp);
                    }
                }
            }

            return informations;
        }
    }
}
