using System.Text;

namespace CnGalWebSite.Helper.Helper
{
    public static class OutputHelper
    {
        public static void PressError(Exception ex, string message = "", string reason = "服务器网络异常", string resolvent = "检查网络是否正常，加群761794704反馈")
        {
            StringBuilder sb=new StringBuilder();
            if (string.IsNullOrWhiteSpace(message) == false)
            {
            sb.AppendLine($"> { message}");
            }
            if (string.IsNullOrWhiteSpace(reason) == false)
            {
            sb.AppendLine($"> 原因：{reason}");

            }
            if (string.IsNullOrWhiteSpace(resolvent) == false)
            {
            sb.AppendLine($"> 解决方法：{resolvent}");

            }


            sb.AppendLine($"> 报错：{ex.Message}");
            sb.AppendLine($"> 调用：{ex.StackTrace}");

            Write(OutputLevel.Dager, sb.ToString());
        }

        public static void Write(OutputLevel level, string text)
        {
            switch (level)
            {
                case OutputLevel.Infor:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case OutputLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case OutputLevel.Dager:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteCenter(string text, double rate = 2, int count = 50)
        {
            var space = (count - text.Length * rate) / 2;

            for (var i = 0; i < space; i++)
            {
                Console.Write(' ');
            }
            Console.WriteLine(text);
        }


        public static void Repeat(string text = "=", bool lineBreak = true, int count = 50)
        {
            for (var i = 0; i < count; i++)
            {
                Console.Write(text);
            }
            if (lineBreak)
            {
                Console.WriteLine();
            }
        }
    }

    public enum OutputLevel
    {
        Infor,
        Warning,
        Dager
    }

}
