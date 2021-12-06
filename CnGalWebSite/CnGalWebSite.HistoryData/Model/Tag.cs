using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#if NET5_0_OR_GREATER
#else
#endif
namespace CnGalWebSite.DataModel.Model
{
    public class Tag
    {
        public int Id { get; set; }
        [Display(Name = "名称")]
        [Required]
        public string Name { get; set; }

        public bool IsHidden { get; set; }

        /// <summary>
        /// 关联词条
        /// </summary>
        public ICollection<Entry> Entries { get; set; }

        public Tag ParentCodeNavigation { get; set; }
        public ICollection<Tag> InverseParentCodeNavigation { get; set; }
    }
}
