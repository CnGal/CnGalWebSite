using CnGalWebSite.Kanban.Components.Dialogs;
using CnGalWebSite.Kanban.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Dialogs
{
    public interface IDialogBoxService
    {
        void ShowDialogBox(DialogBoxModel model);

        void ShowDialogBox(string content, int priority);

        void Init(DialogBoxCard dialogBoxCard);

        void CloseDialogBox();
    }
}
