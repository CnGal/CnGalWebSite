using CnGalWebSite.DataModel.ViewModel.Others;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public class MauiService : IMauiService
    {
        public void ChangeTheme(string theme)
        {
            
        }

        public Task OpenNewPage(string url)
        {
            return Task.CompletedTask;
        }

        public void Quit()
        {
            
        }

        public void SetStatusBarTransparent(bool hide)
        {
           
        }

        public Task ShareLink(ShareLinkModel model)
        {
            return Task.CompletedTask;
        }
    }
}
