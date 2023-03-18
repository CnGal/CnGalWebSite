using CnGalWebSite.DataModel.ViewModel;
using System.Text;

namespace CnGalWebSite.Helper.Extensions
{
    public static class ListExtension
    {
        public static string Export(this List<StaffInforModel> sources)
        {
            var sb = new StringBuilder();
            foreach (var item in sources)
            {
                if (string.IsNullOrWhiteSpace(item.Modifier) == false)
                {
                    sb.AppendLine("\n" + item.Modifier + "：");
                }
                foreach (var infor in item.StaffList)
                {
                    sb.Append(infor.Modifier + "：");
                    foreach (var temp in infor.Names)
                    {
                        sb.Append(temp.DisplayName + (infor.Names.IndexOf(temp) == infor.Names.Count - 1 ? "\n" : "，"));
                    }
                }
            }
            var reslut = sb.ToString();
            return reslut;
        }
    }
}
