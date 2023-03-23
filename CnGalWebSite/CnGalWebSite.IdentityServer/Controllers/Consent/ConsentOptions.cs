// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI
{
    public class ConsentOptions
    {
        public static bool EnableOfflineAccess = true;
        public static string OfflineAccessDisplayName = "离线访问";
        public static string OfflineAccessDescription = "即使处于离线状态也可以访问你的应用和资源";

        public static readonly string MustChooseOneErrorMessage = "你必须至少选择一个授权";
        public static readonly string InvalidSelectionErrorMessage = "无效的选择";
    }
}
