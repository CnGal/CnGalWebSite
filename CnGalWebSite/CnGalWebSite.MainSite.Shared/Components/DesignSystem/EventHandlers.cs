using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

[EventHandler("oncginputcommit", typeof(ChangeEventArgs))]
[EventHandler("oncgfocusleave", typeof(CgFocusLeaveEventArgs))]
public static class EventHandlers
{
}

public sealed class CgFocusLeaveEventArgs : EventArgs
{
    public bool IsInternal { get; set; }
}
