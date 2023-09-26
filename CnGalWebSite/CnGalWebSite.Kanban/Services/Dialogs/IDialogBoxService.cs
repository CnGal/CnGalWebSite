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
        Task ShowDialogBox(DialogBoxModel model);

        Task ShowDialogBox(string content);

        void Init(DialogBoxCard dialogBoxCard);
    }
}
