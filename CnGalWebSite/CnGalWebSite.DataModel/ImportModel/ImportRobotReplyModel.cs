namespace CnGalWebSite.DataModel.ImportModel
{
    public class ImportRobotReplyModel
    {
        public string LxKey { get; set; }

        public string LxValue { get; set; }

        public LxType LxType { get; set; }

        public string Time { get; set; }

        public string AfterTime { get; set; }

        public string BeforeTime { get; set; }
    }

    public enum LxType
    {
        /// <summary>
        /// 完全匹配
        /// </summary>
        ExactMatch,
        /// <summary>
        /// 匹配星号
        /// </summary>
        Asterisk,
        /// <summary>
        /// 正则表达式
        /// </summary>
        RegularExpression
    }
}
