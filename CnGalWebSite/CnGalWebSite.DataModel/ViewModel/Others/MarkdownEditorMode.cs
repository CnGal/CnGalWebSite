using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Others
{

    public enum MarkdownEditorMode
    {
        [Display(Name ="所见即所得")]
        sv,
        [Display(Name = "即时渲染")]
        ir,
        [Display(Name = "分屏预览")]
        wysiwyg
    }

}
