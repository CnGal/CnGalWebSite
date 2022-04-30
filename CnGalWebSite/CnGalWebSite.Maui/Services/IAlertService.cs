using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Maui.Services
{
    public interface IAlertService
    {
        void ShowAlert(string Title, string Text);

        void Init(MainPage page);
    }
}
