namespace CnGalWebSite.DataModel.Model
{
    public class RobotGroup
    {
        public long Id { get; set; }

        public long GroupId { get; set; }

        public bool IsHidden { get; set; }

        public string Note { get; set; }

        /// <summary>
        /// 只有当消息中含有 参数 Name 的值 时才匹配
        /// </summary>
        public bool ForceMatch { get; set; }

    }
}
