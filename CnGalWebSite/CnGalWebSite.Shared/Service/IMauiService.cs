using CnGalWebSite.DataModel.ViewModel.Others;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IMauiService
    {
        void ChangeTheme(string theme);

        Task ShareLink(ShareLinkModel model);

        void Quit();

        void SetStatusBarTransparent(bool hide);

        Task OpenNewPage(string url);
    }
}
