
namespace CnGalWebSite.Extensions
{
    public static class ColorExtensions
    {
        public static string GetGradientColor(int index, bool isText = false, string[] colorList = null)
        {
            if (colorList == null)
            {
                colorList = new string[]
                {
                    "red",
                    "pink",
                    "purple",
                    "deep-purple",
                    "indigo",
                    "blue",
                    "light-blue",
                    "cyan",
                    "teal",
                    "green",
                    "light-green",
                    "amber  ",
                    "orange ",
                    "deep-orange"
                };
            }
            var color = colorList[index % colorList.Length];
            if (isText)
            {

                color = GetTextColor(color);
            }

            return color;
        }

        public static string GetTextColor(string color)
        {
            var colors = color.Split(' ');
            var stringBuilder = new System.Text.StringBuilder();
            if (colors.Length > 0)
            {
                stringBuilder.Append($"{colors[0]}--text");
            }
            if (colors.Length > 1)
            {
                stringBuilder.Append($" text--{colors[1]}");
            }


            return stringBuilder.ToString();
        }
    }
}
