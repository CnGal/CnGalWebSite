using Masa.Blazor;
using Masa.Blazor.Presets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Components.Extensions
{
    public static class PopupServiceExtensions
    {
        public static async Task ToastErrorAsync(this IPopupService popupService, string title, string message=null)
        {
            await popupService.ToastAsync(title, message, AlertTypes.Error);
        }
        public static async Task ToastWarningAsync(this IPopupService popupService, string title, string message = null)
        {
            await popupService.ToastAsync(title, message, AlertTypes.Warning);
        }
        public static async Task ToastInfoAsync(this IPopupService popupService, string title, string message = null)
        {
            await popupService.ToastAsync(title, message, AlertTypes.Info);
        }
        public static async Task ToastSuccessAsync(this IPopupService popupService, string title, string message = null)
        {
            await popupService.ToastAsync(title, message, AlertTypes.Success);
        }

        public static async Task ToastAsync(this IPopupService popupService, string title, string message, AlertTypes type)
        {
            await popupService.EnqueueSnackbarAsync(new SnackbarOptions
            {
                Type = type,
                Title = title,
                Content = message,
            });
        }

        public static async Task ToastAsync(this IPopupService popupService, string title, AlertTypes type)
        {
            await popupService.ToastAsync(title, null, type);
        }
    }
}
