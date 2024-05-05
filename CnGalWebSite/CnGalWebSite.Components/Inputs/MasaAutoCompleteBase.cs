using Masa.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Components.Inputs
{
    public class MasaAutoCompleteBase<TItem, TItemValue, TValue>:MAutocomplete<TItem, TItemValue, TValue>
    {
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (MMenu != null)
            {
                MMenu.AllowOverflow = true;
            }
        }
    }
}
