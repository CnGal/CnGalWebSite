using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.Components.Extensions;
using CnGalWebSite.Components.Service;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.Kanban.Extensions;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Service;
using Masa.Blazor.Popup;
using Masa.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Masa.Blazor.Presets;


namespace CnGalWebSite.Shared.Extentions
{
    public static class IServiceCollectionExtentions
    {
        public static IServiceCollection AddMainSite(this IServiceCollection services)
        {
            //Masa Blazor 组件库
            services.AddMasaBlazor(options =>
            {
                //主题
                options.ConfigureTheme(theme =>
                {
                    theme.DefaultTheme = "pink-dark";
                    // Amber Light Theme
                    theme.Themes.Add("amber-light", false, custom =>
                    {
                        custom.Primary = "#785900";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#6b5d3f";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#4a6547";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#1e1b16";
                        custom.SurfaceDim = "#e9e1d8";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#f7efe7";
                        custom.SurfaceContainerLow = "#fff8f2";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e9e1d8";
                        custom.SurfaceContainerHighest = "#ccc5bd";
                        custom.InversePrimary = "#fabd00";
                        custom.InverseSurface = "#33302a";
                        custom.InverseOnSurface = "#f7efe7";
                    });

                    // Amber Dark Theme
                    theme.Themes.Add("amber-dark", true, custom =>
                    {
                        custom.Primary = "#fabd00";
                        custom.OnPrimary = "#3f2e00";
                        custom.Secondary = "#d8c4a0";
                        custom.OnSecondary = "#3a2f15";
                        custom.Accent = "#b0cfaa";
                        custom.OnAccent = "#1d361c";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1e1b16";
                        custom.OnSurface = "#e9e1d8";
                        custom.SurfaceDim = "#1e1b16";
                        custom.SurfaceBright = "#4a4640";
                        custom.SurfaceContainer = "#33302a";
                        custom.SurfaceContainerLow = "#1e1b16";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4a4640";
                        custom.SurfaceContainerHighest = "#56524b";
                        custom.InversePrimary = "#785900";
                        custom.InverseSurface = "#e9e1d8";
                        custom.InverseOnSurface = "#1e1b16";
                    });

                    // Blue Light Theme
                    theme.Themes.Add("blue-light", false, custom =>
                    {
                        custom.Primary = "#0061a4";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#535f70";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#6b5778";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fdfcff";
                        custom.OnSurface = "#1a1c1e";
                        custom.SurfaceDim = "#e2e2e6";
                        custom.SurfaceBright = "#fdfcff";
                        custom.SurfaceContainer = "#f1f0f4";
                        custom.SurfaceContainerLow = "#faf9fc";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e2e2e6";
                        custom.SurfaceContainerHighest = "#c6c6ca";
                        custom.InversePrimary = "#9ecaff";
                        custom.InverseSurface = "#2f3033";
                        custom.InverseOnSurface = "#f1f0f4";
                    });

                    // Blue Dark Theme
                    theme.Themes.Add("blue-dark", true, custom =>
                    {
                        custom.Primary = "#9ecaff";
                        custom.OnPrimary = "#003258";
                        custom.Secondary = "#bbc7db";
                        custom.OnSecondary = "#253140";
                        custom.Accent = "#d6bee4";
                        custom.OnAccent = "#3b2948";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1a1c1e";
                        custom.OnSurface = "#e2e2e6";
                        custom.SurfaceDim = "#1a1c1e";
                        custom.SurfaceBright = "#45474a";
                        custom.SurfaceContainer = "#2f3033";
                        custom.SurfaceContainerLow = "#1a1c1e";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#45474a";
                        custom.SurfaceContainerHighest = "#515255";
                        custom.InversePrimary = "#0061a4";
                        custom.InverseSurface = "#e2e2e6";
                        custom.InverseOnSurface = "#1a1c1e";
                    });

                    // Red Light Theme
                    theme.Themes.Add("red-light", false, custom =>
                    {
                        custom.Primary = "#bb1614";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#775652";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#705c2e";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#201a19";
                        custom.SurfaceDim = "#ede0de";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#fbeeec";
                        custom.SurfaceContainerLow = "#fff8f7";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#ede0de";
                        custom.SurfaceContainerHighest = "#d0c4c2";
                        custom.InversePrimary = "#ffb4a9";
                        custom.InverseSurface = "#362f2e";
                        custom.InverseOnSurface = "#fbeeec";
                    });

                    // Red Dark Theme
                    theme.Themes.Add("red-dark", true, custom =>
                    {
                        custom.Primary = "#ffb4a9";
                        custom.OnPrimary = "#690002";
                        custom.Secondary = "#e7bdb7";
                        custom.OnSecondary = "#442926";
                        custom.Accent = "#dfc38c";
                        custom.OnAccent = "#3e2e04";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#201a19";
                        custom.OnSurface = "#ede0de";
                        custom.SurfaceDim = "#201a19";
                        custom.SurfaceBright = "#4d4544";
                        custom.SurfaceContainer = "#362f2e";
                        custom.SurfaceContainerLow = "#201a19";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4d4544";
                        custom.SurfaceContainerHighest = "#59504f";
                        custom.InversePrimary = "#bb1614";
                        custom.InverseSurface = "#ede0de";
                        custom.InverseOnSurface = "#201a19";
                    });

                    // Green Light Theme
                    theme.Themes.Add("green-light", false, custom =>
                    {
                        custom.Primary = "#006e1c";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#52634f";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#38656a";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fcfdf6";
                        custom.OnSurface = "#1a1c19";
                        custom.SurfaceDim = "#e2e3dd";
                        custom.SurfaceBright = "#fcfdf6";
                        custom.SurfaceContainer = "#f0f1eb";
                        custom.SurfaceContainerLow = "#f9faf4";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e2e3dd";
                        custom.SurfaceContainerHighest = "#c6c7c1";
                        custom.InversePrimary = "#78dc77";
                        custom.InverseSurface = "#2f312d";
                        custom.InverseOnSurface = "#f0f1eb";
                    });

                    // Green Dark Theme
                    theme.Themes.Add("green-dark", true, custom =>
                    {
                        custom.Primary = "#78dc77";
                        custom.OnPrimary = "#00390a";
                        custom.Secondary = "#baccb3";
                        custom.OnSecondary = "#253423";
                        custom.Accent = "#a0cfd4";
                        custom.OnAccent = "#00363b";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1a1c19";
                        custom.OnSurface = "#e2e3dd";
                        custom.SurfaceDim = "#1a1c19";
                        custom.SurfaceBright = "#454743";
                        custom.SurfaceContainer = "#2f312d";
                        custom.SurfaceContainerLow = "#1a1c19";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#454743";
                        custom.SurfaceContainerHighest = "#51534f";
                        custom.InversePrimary = "#006e1c";
                        custom.InverseSurface = "#e2e3dd";
                        custom.InverseOnSurface = "#1a1c19";
                    });

                    // Purple Light Theme
                    theme.Themes.Add("purple-light", false, custom =>
                    {
                        custom.Primary = "#9a25ae";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#6b586b";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#82524a";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#1e1a1d";
                        custom.SurfaceDim = "#e9e0e4";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#f7eef3";
                        custom.SurfaceContainerLow = "#fff7fa";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e9e0e4";
                        custom.SurfaceContainerHighest = "#ccc4c8";
                        custom.InversePrimary = "#f9abff";
                        custom.InverseSurface = "#332f32";
                        custom.InverseOnSurface = "#f7eef3";
                    });

                    // Purple Dark Theme
                    theme.Themes.Add("purple-dark", true, custom =>
                    {
                        custom.Primary = "#f9abff";
                        custom.OnPrimary = "#570066";
                        custom.Secondary = "#d7bfd5";
                        custom.OnSecondary = "#3b2b3c";
                        custom.Accent = "#f6b8ad";
                        custom.OnAccent = "#4c251f";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1e1a1d";
                        custom.OnSurface = "#e9e0e4";
                        custom.SurfaceDim = "#1e1a1d";
                        custom.SurfaceBright = "#4a4549";
                        custom.SurfaceContainer = "#332f32";
                        custom.SurfaceContainerLow = "#1e1a1d";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4a4549";
                        custom.SurfaceContainerHighest = "#565154";
                        custom.InversePrimary = "#9a25ae";
                        custom.InverseSurface = "#e9e0e4";
                        custom.InverseOnSurface = "#1e1a1d";
                    });

                    // Deep Orange Light Theme
                    theme.Themes.Add("deep-orange-light", false, custom =>
                    {
                        custom.Primary = "#b02f00";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#77574e";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#6c5d2f";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#201a18";
                        custom.SurfaceDim = "#ede0dc";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#fbeeeb";
                        custom.SurfaceContainerLow = "#fff8f6";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#ede0dc";
                        custom.SurfaceContainerHighest = "#d0c4c1";
                        custom.InversePrimary = "#ffb5a0";
                        custom.InverseSurface = "#362f2d";
                        custom.InverseOnSurface = "#fbeeeb";
                    });

                    // Deep Orange Dark Theme
                    theme.Themes.Add("deep-orange-dark", true, custom =>
                    {
                        custom.Primary = "#ffb5a0";
                        custom.OnPrimary = "#5f1500";
                        custom.Secondary = "#e7bdb2";
                        custom.OnSecondary = "#442a22";
                        custom.Accent = "#d8c58d";
                        custom.OnAccent = "#3b2f05";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#201a18";
                        custom.OnSurface = "#ede0dc";
                        custom.SurfaceDim = "#201a18";
                        custom.SurfaceBright = "#4d4543";
                        custom.SurfaceContainer = "#362f2d";
                        custom.SurfaceContainerLow = "#201a18";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4d4543";
                        custom.SurfaceContainerHighest = "#59514e";
                        custom.InversePrimary = "#b02f00";
                        custom.InverseSurface = "#ede0dc";
                        custom.InverseOnSurface = "#201a18";
                    });

                    // Brown Light Theme
                    theme.Themes.Add("brown-light", false, custom =>
                    {
                        custom.Primary = "#9a4522";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#77574c";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#695e2f";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#201a18";
                        custom.SurfaceDim = "#ede0dc";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#fbeeea";
                        custom.SurfaceContainerLow = "#fff8f6";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#ede0dc";
                        custom.SurfaceContainerHighest = "#d0c4c0";
                        custom.InversePrimary = "#ffb59a";
                        custom.InverseSurface = "#362f2c";
                        custom.InverseOnSurface = "#fbeeea";
                    });

                    // Brown Dark Theme
                    theme.Themes.Add("brown-dark", true, custom =>
                    {
                        custom.Primary = "#ffb59a";
                        custom.OnPrimary = "#5b1b00";
                        custom.Secondary = "#e7beaf";
                        custom.OnSecondary = "#442a20";
                        custom.Accent = "#d5c68e";
                        custom.OnAccent = "#393005";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#201a18";
                        custom.OnSurface = "#ede0dc";
                        custom.SurfaceDim = "#201a18";
                        custom.SurfaceBright = "#4d4542";
                        custom.SurfaceContainer = "#362f2c";
                        custom.SurfaceContainerLow = "#201a18";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4d4542";
                        custom.SurfaceContainerHighest = "#59514e";
                        custom.InversePrimary = "#9a4522";
                        custom.InverseSurface = "#ede0dc";
                        custom.InverseOnSurface = "#201a18";
                    });

                    // Cyan Light Theme
                    theme.Themes.Add("cyan-light", false, custom =>
                    {
                        custom.Primary = "#006876";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#4a6268";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#545d7e";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fbfcfd";
                        custom.OnSurface = "#191c1d";
                        custom.SurfaceDim = "#e1e3e3";
                        custom.SurfaceBright = "#fbfcfd";
                        custom.SurfaceContainer = "#eff1f2";
                        custom.SurfaceContainerLow = "#f8fafa";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e1e3e3";
                        custom.SurfaceContainerHighest = "#c4c7c8";
                        custom.InversePrimary = "#44d8f1";
                        custom.InverseSurface = "#2e3132";
                        custom.InverseOnSurface = "#eff1f2";
                    });

                    // Cyan Dark Theme
                    theme.Themes.Add("cyan-dark", true, custom =>
                    {
                        custom.Primary = "#44d8f1";
                        custom.OnPrimary = "#00363e";
                        custom.Secondary = "#b1cbd1";
                        custom.OnSecondary = "#1c3439";
                        custom.Accent = "#bcc5eb";
                        custom.OnAccent = "#262f4d";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#191c1d";
                        custom.OnSurface = "#e1e3e3";
                        custom.SurfaceDim = "#191c1d";
                        custom.SurfaceBright = "#444748";
                        custom.SurfaceContainer = "#2e3132";
                        custom.SurfaceContainerLow = "#191c1d";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#444748";
                        custom.SurfaceContainerHighest = "#505354";
                        custom.InversePrimary = "#006876";
                        custom.InverseSurface = "#e1e3e3";
                        custom.InverseOnSurface = "#191c1d";
                    });

                    // Deep Purple Light Theme
                    theme.Themes.Add("deep-purple-light", false, custom =>
                    {
                        custom.Primary = "#6f43c0";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#635b70";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#7e525d";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#1d1b1e";
                        custom.SurfaceDim = "#e6e1e6";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#f5eff4";
                        custom.SurfaceContainerLow = "#fef8fd";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e6e1e6";
                        custom.SurfaceContainerHighest = "#cac5ca";
                        custom.InversePrimary = "#d3bbff";
                        custom.InverseSurface = "#323033";
                        custom.InverseOnSurface = "#f5eff4";
                    });

                    // Deep Purple Dark Theme
                    theme.Themes.Add("deep-purple-dark", true, custom =>
                    {
                        custom.Primary = "#d3bbff";
                        custom.OnPrimary = "#3f008d";
                        custom.Secondary = "#cdc2db";
                        custom.OnSecondary = "#342d40";
                        custom.Accent = "#f0b7c5";
                        custom.OnAccent = "#4a2530";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1d1b1e";
                        custom.OnSurface = "#e6e1e6";
                        custom.SurfaceDim = "#1d1b1e";
                        custom.SurfaceBright = "#48464a";
                        custom.SurfaceContainer = "#323033";
                        custom.SurfaceContainerLow = "#1d1b1e";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#48464a";
                        custom.SurfaceContainerHighest = "#545155";
                        custom.InversePrimary = "#6f43c0";
                        custom.InverseSurface = "#e6e1e6";
                        custom.InverseOnSurface = "#1d1b1e";
                    });

                    // Grey Light Theme
                    theme.Themes.Add("grey-light", false, custom =>
                    {
                        custom.Primary = "#006874";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#4a6267";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#525e7d";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fafdfd";
                        custom.OnSurface = "#191c1d";
                        custom.SurfaceDim = "#e1e3e3";
                        custom.SurfaceBright = "#fafdfd";
                        custom.SurfaceContainer = "#eff1f1";
                        custom.SurfaceContainerLow = "#f8fafa";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e1e3e3";
                        custom.SurfaceContainerHighest = "#c4c7c7";
                        custom.InversePrimary = "#4fd8eb";
                        custom.InverseSurface = "#2e3132";
                        custom.InverseOnSurface = "#eff1f1";
                    });

                    // Grey Dark Theme
                    theme.Themes.Add("grey-dark", true, custom =>
                    {
                        custom.Primary = "#4fd8eb";
                        custom.OnPrimary = "#00363d";
                        custom.Secondary = "#b1ccc6";
                        custom.OnSecondary = "#1c3438";
                        custom.Accent = "#bac6ea";
                        custom.OnAccent = "#24304d";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#191c1d";
                        custom.OnSurface = "#e1e3e3";
                        custom.SurfaceDim = "#191c1d";
                        custom.SurfaceBright = "#444748";
                        custom.SurfaceContainer = "#2e3132";
                        custom.SurfaceContainerLow = "#191c1d";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#444748";
                        custom.SurfaceContainerHighest = "#505354";
                        custom.InversePrimary = "#006874";
                        custom.InverseSurface = "#e1e3e3";
                        custom.InverseOnSurface = "#191c1d";
                    });

                    // Indigo Light Theme
                    theme.Themes.Add("indigo-light", false, custom =>
                    {
                        custom.Primary = "#4355b9";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#5b5d72";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#77536d";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fefbff";
                        custom.OnSurface = "#1b1b1f";
                        custom.SurfaceDim = "#e4e1e6";
                        custom.SurfaceBright = "#fefbff";
                        custom.SurfaceContainer = "#f3f0f4";
                        custom.SurfaceContainerLow = "#fbf8fd";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e4e1e6";
                        custom.SurfaceContainerHighest = "#c8c5ca";
                        custom.InversePrimary = "#bac3ff";
                        custom.InverseSurface = "#303034";
                        custom.InverseOnSurface = "#f3f0f4";
                    });

                    // Indigo Dark Theme
                    theme.Themes.Add("indigo-dark", true, custom =>
                    {
                        custom.Primary = "#bac3ff";
                        custom.OnPrimary = "#08218a";
                        custom.Secondary = "#c3c5dd";
                        custom.OnSecondary = "#2d2f42";
                        custom.Accent = "#e6bad7";
                        custom.OnAccent = "#44263d";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1b1b1f";
                        custom.OnSurface = "#e4e1e6";
                        custom.SurfaceDim = "#1b1b1f";
                        custom.SurfaceBright = "#47464a";
                        custom.SurfaceContainer = "#303034";
                        custom.SurfaceContainerLow = "#1b1b1f";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#47464a";
                        custom.SurfaceContainerHighest = "#535256";
                        custom.InversePrimary = "#4355b9";
                        custom.InverseSurface = "#e4e1e6";
                        custom.InverseOnSurface = "#1b1b1f";
                    });

                    // Light Blue Light Theme
                    theme.Themes.Add("light-blue-light", false, custom =>
                    {
                        custom.Primary = "#006493";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#50606e";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#65587b";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fcfcff";
                        custom.OnSurface = "#1a1c1e";
                        custom.SurfaceDim = "#e2e2e5";
                        custom.SurfaceBright = "#fcfcff";
                        custom.SurfaceContainer = "#f0f0f3";
                        custom.SurfaceContainerLow = "#f9f9fc";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e2e2e5";
                        custom.SurfaceContainerHighest = "#c6c6c9";
                        custom.InversePrimary = "#8dcdff";
                        custom.InverseSurface = "#2e3133";
                        custom.InverseOnSurface = "#f0f0f3";
                    });

                    // Light Blue Dark Theme
                    theme.Themes.Add("light-blue-dark", true, custom =>
                    {
                        custom.Primary = "#8dcdff";
                        custom.OnPrimary = "#00344f";
                        custom.Secondary = "#b7c9d9";
                        custom.OnSecondary = "#22323f";
                        custom.Accent = "#cfc0e8";
                        custom.OnAccent = "#362b4b";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1a1c1e";
                        custom.OnSurface = "#e2e2e5";
                        custom.SurfaceDim = "#1a1c1e";
                        custom.SurfaceBright = "#454749";
                        custom.SurfaceContainer = "#2e3133";
                        custom.SurfaceContainerLow = "#1a1c1e";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#454749";
                        custom.SurfaceContainerHighest = "#515255";
                        custom.InversePrimary = "#006493";
                        custom.InverseSurface = "#e2e2e5";
                        custom.InverseOnSurface = "#1a1c1e";
                    });

                    // Light Green Light Theme
                    theme.Themes.Add("light-green-light", false, custom =>
                    {
                        custom.Primary = "#3e6a00";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#576249";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#386663";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fdfcf5";
                        custom.OnSurface = "#1b1c18";
                        custom.SurfaceDim = "#e3e3db";
                        custom.SurfaceBright = "#fdfcf5";
                        custom.SurfaceContainer = "#f2f1e9";
                        custom.SurfaceContainerLow = "#fafaf2";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e3e3db";
                        custom.SurfaceContainerHighest = "#c7c7c0";
                        custom.InversePrimary = "#9ed75b";
                        custom.InverseSurface = "#30312c";
                        custom.InverseOnSurface = "#f2f1e9";
                    });

                    // Light Green Dark Theme
                    theme.Themes.Add("light-green-dark", true, custom =>
                    {
                        custom.Primary = "#9ed75b";
                        custom.OnPrimary = "#1e3700";
                        custom.Secondary = "#bfcbad";
                        custom.OnSecondary = "#2a331e";
                        custom.Accent = "#a0d0cc";
                        custom.OnAccent = "#003735";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1b1c18";
                        custom.OnSurface = "#e3e3db";
                        custom.SurfaceDim = "#1b1c18";
                        custom.SurfaceBright = "#464742";
                        custom.SurfaceContainer = "#30312c";
                        custom.SurfaceContainerLow = "#1b1c18";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#464742";
                        custom.SurfaceContainerHighest = "#52534d";
                        custom.InversePrimary = "#3e6a00";
                        custom.InverseSurface = "#e3e3db";
                        custom.InverseOnSurface = "#1b1c18";
                    });

                    // Light Red Light Theme
                    theme.Themes.Add("light-red-light", false, custom =>
                    {
                        custom.Primary = "#FF4D4F";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#FF8D1A";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#FFC300";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#201a1a";
                        custom.SurfaceDim = "#ede0de";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#fbeeec";
                        custom.SurfaceContainerLow = "#fff8f7";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#ede0de";
                        custom.SurfaceContainerHighest = "#d0c4c3";
                        custom.InversePrimary = "#ffb3ae";
                        custom.InverseSurface = "#362f2e";
                        custom.InverseOnSurface = "#fbeeec";
                    });

                    // Light Red Dark Theme
                    theme.Themes.Add("light-red-dark", true, custom =>
                    {
                        custom.Primary = "#ffb3ae";
                        custom.OnPrimary = "#68000c";
                        custom.Secondary = "#ffb780";
                        custom.OnSecondary = "#4e2600";
                        custom.Accent = "#f8be00";
                        custom.OnAccent = "#3f2e00";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#201a1a";
                        custom.OnSurface = "#ede0de";
                        custom.SurfaceDim = "#201a1a";
                        custom.SurfaceBright = "#4d4544";
                        custom.SurfaceContainer = "#362f2e";
                        custom.SurfaceContainerLow = "#201a1a";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4d4544";
                        custom.SurfaceContainerHighest = "#595050";
                        custom.InversePrimary = "#ba1725";
                        custom.InverseSurface = "#ede0de";
                        custom.InverseOnSurface = "#201a1a";
                    });

                    // Lime Light Theme
                    theme.Themes.Add("lime-light", false, custom =>
                    {
                        custom.Primary = "#5b6300";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#5e6044";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#3c665a";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#feffd8";
                        custom.OnSurface = "#1c1c17";
                        custom.SurfaceDim = "#e5e2da";
                        custom.SurfaceBright = "#feffd8";
                        custom.SurfaceContainer = "#f3f1e8";
                        custom.SurfaceContainerLow = "#fcf9f0";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e5e2da";
                        custom.SurfaceContainerHighest = "#c9c6be";
                        custom.InversePrimary = "#c1d02c";
                        custom.InverseSurface = "#31312b";
                        custom.InverseOnSurface = "#f3f1e8";
                    });

                    // Lime Dark Theme
                    theme.Themes.Add("lime-dark", true, custom =>
                    {
                        custom.Primary = "#c1d02c";
                        custom.OnPrimary = "#2f3300";
                        custom.Secondary = "#c7c9a6";
                        custom.OnSecondary = "#30321a";
                        custom.Accent = "#a2d0c1";
                        custom.OnAccent = "#07372d";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1c1c17";
                        custom.OnSurface = "#e5e2da";
                        custom.SurfaceDim = "#1c1c17";
                        custom.SurfaceBright = "#474741";
                        custom.SurfaceContainer = "#31312b";
                        custom.SurfaceContainerLow = "#1c1c17";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#474741";
                        custom.SurfaceContainerHighest = "#53534c";
                        custom.InversePrimary = "#5b6300";
                        custom.InverseSurface = "#e5e2da";
                        custom.InverseOnSurface = "#1c1c17";
                    });

                    // Orange Light Theme
                    theme.Themes.Add("orange-light", false, custom =>
                    {
                        custom.Primary = "#8b5000";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#725a42";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#58633a";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#201b16";
                        custom.SurfaceDim = "#ebe0d9";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#faefe7";
                        custom.SurfaceContainerLow = "#fff8f5";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#ebe0d9";
                        custom.SurfaceContainerHighest = "#cfc5be";
                        custom.InversePrimary = "#ffb870";
                        custom.InverseSurface = "#352f2b";
                        custom.InverseOnSurface = "#faefe7";
                    });

                    // Orange Dark Theme
                    theme.Themes.Add("orange-dark", true, custom =>
                    {
                        custom.Primary = "#ffb870";
                        custom.OnPrimary = "#4a2800";
                        custom.Secondary = "#e1c1a4";
                        custom.OnSecondary = "#402c18";
                        custom.Accent = "#c0cc9a";
                        custom.OnAccent = "#2b3410";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#201b16";
                        custom.OnSurface = "#ebe0d9";
                        custom.SurfaceDim = "#201b16";
                        custom.SurfaceBright = "#4c4640";
                        custom.SurfaceContainer = "#352f2b";
                        custom.SurfaceContainerLow = "#201b16";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4c4640";
                        custom.SurfaceContainerHighest = "#58514c";
                        custom.InversePrimary = "#8b5000";
                        custom.InverseSurface = "#ebe0d9";
                        custom.InverseOnSurface = "#201b16";
                    });

                    // Pink Light Theme
                    theme.Themes.Add("pink-light", false, custom =>
                    {
                        custom.Primary = "#bc004b";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#75565b";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#795831";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#201a1b";
                        custom.SurfaceDim = "#ece0e0";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#fbeeee";
                        custom.SurfaceContainerLow = "#fff8f7";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#ece0e0";
                        custom.SurfaceContainerHighest = "#cfc4c4";
                        custom.InversePrimary = "#ffb2be";
                        custom.InverseSurface = "#362f2f";
                        custom.InverseOnSurface = "#fbeeee";
                    });

                    // Pink Dark Theme
                    theme.Themes.Add("pink-dark", true, custom =>
                    {
                        custom.Primary = "#ffb2be";
                        custom.OnPrimary = "#660025";
                        custom.Secondary = "#e5bdc2";
                        custom.OnSecondary = "#43292d";
                        custom.Accent = "#ebbf90";
                        custom.OnAccent = "#452b08";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#201a1b";
                        custom.OnSurface = "#ece0e0";
                        custom.SurfaceDim = "#201a1b";
                        custom.SurfaceBright = "#4d4546";
                        custom.SurfaceContainer = "#362f2f";
                        custom.SurfaceContainerLow = "#201a1b";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#4d4546";
                        custom.SurfaceContainerHighest = "#595051";
                        custom.InversePrimary = "#bc004b";
                        custom.InverseSurface = "#ece0e0";
                        custom.InverseOnSurface = "#201a1b";
                    });

                    // Teal Light Theme
                    theme.Themes.Add("teal-light", false, custom =>
                    {
                        custom.Primary = "#006a60";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#4a635f";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#456179";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fafdfb";
                        custom.OnSurface = "#191c1b";
                        custom.SurfaceDim = "#e0e3e1";
                        custom.SurfaceBright = "#fafdfb";
                        custom.SurfaceContainer = "#eff1ef";
                        custom.SurfaceContainerLow = "#f7faf8";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e0e3e1";
                        custom.SurfaceContainerHighest = "#c4c7c5";
                        custom.InversePrimary = "#53dbc9";
                        custom.InverseSurface = "#2d3130";
                        custom.InverseOnSurface = "#eff1ef";
                    });

                    // Teal Dark Theme
                    theme.Themes.Add("teal-dark", true, custom =>
                    {
                        custom.Primary = "#53dbc9";
                        custom.OnPrimary = "#003731";
                        custom.Secondary = "#b1ccc6";
                        custom.OnSecondary = "#1c3531";
                        custom.Accent = "#adcae6";
                        custom.OnAccent = "#153349";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#191c1b";
                        custom.OnSurface = "#e0e3e1";
                        custom.SurfaceDim = "#191c1b";
                        custom.SurfaceBright = "#444746";
                        custom.SurfaceContainer = "#2d3130";
                        custom.SurfaceContainerLow = "#191c1b";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#444746";
                        custom.SurfaceContainerHighest = "#505352";
                        custom.InversePrimary = "#006a60";
                        custom.InverseSurface = "#e0e3e1";
                        custom.InverseOnSurface = "#191c1b";
                    });

                    // Yellow Light Theme
                    theme.Themes.Add("yellow-light", false, custom =>
                    {
                        custom.Primary = "#695f00";
                        custom.OnPrimary = "#ffffff";
                        custom.Secondary = "#645f41";
                        custom.OnSecondary = "#ffffff";
                        custom.Accent = "#406652";
                        custom.OnAccent = "#ffffff";
                        custom.Error = "#ba1a1a";
                        custom.OnError = "#ffffff";
                        custom.Surface = "#fffbff";
                        custom.OnSurface = "#1d1c16";
                        custom.SurfaceDim = "#e7e2d9";
                        custom.SurfaceBright = "#fffbff";
                        custom.SurfaceContainer = "#f5f0e7";
                        custom.SurfaceContainerLow = "#fef9f0";
                        custom.SurfaceContainerLowest = "#ffffff";
                        custom.SurfaceContainerHigh = "#e7e2d9";
                        custom.SurfaceContainerHighest = "#cac6bd";
                        custom.InversePrimary = "#dbc90a";
                        custom.InverseSurface = "#32302a";
                        custom.InverseOnSurface = "#f5f0e7";
                    });

                    // Yellow Dark Theme
                    theme.Themes.Add("yellow-dark", true, custom =>
                    {
                        custom.Primary = "#dbc90a";
                        custom.OnPrimary = "#363100";
                        custom.Secondary = "#cec7a3";
                        custom.OnSecondary = "#343117";
                        custom.Accent = "#a7d0b7";
                        custom.OnAccent = "#103726";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1d1c16";
                        custom.OnSurface = "#e7e2d9";
                        custom.SurfaceDim = "#1d1c16";
                        custom.SurfaceBright = "#494740";
                        custom.SurfaceContainer = "#32302a";
                        custom.SurfaceContainerLow = "#1d1c16";
                        custom.SurfaceContainerLowest = "#000000";
                        custom.SurfaceContainerHigh = "#494740";
                        custom.SurfaceContainerHighest = "#54524c";
                        custom.InversePrimary = "#695f00";
                        custom.InverseSurface = "#e7e2d9";
                        custom.InverseOnSurface = "#1d1c16";
                    });

                    // 保留原有的camel主题
                    theme.Themes.Add("camel", true, custom =>
                    {
                        custom.Primary = "#ffb68a";
                        custom.OnPrimary = "#522300";
                        custom.Secondary = "#e5bfa9";
                        custom.OnSecondary = "#432b1c";
                        custom.Accent = "#cbc992";
                        custom.OnAccent = "#333209";
                        custom.Error = "#ffb4ab";
                        custom.OnError = "#690005";
                        custom.Surface = "#1a120d";
                        custom.OnSurface = "#f0dfd7";
                        custom.SurfaceDim = "#1a120d";
                        custom.SurfaceBright = "#413732";
                        custom.SurfaceContainer = "#261e19";
                        custom.SurfaceContainerLow = "#221a15";
                        custom.SurfaceContainerLowest = "#140d08";
                        custom.SurfaceContainerHigh = "#312823";
                        custom.SurfaceContainerHighest = "#3d332d";
                        custom.InversePrimary = "#8c4f26";
                        custom.InverseSurface = "#f0dfd7";
                        custom.InverseOnSurface = "#382e29";
                    });
                });

                // 提示框
                options.Defaults = new Dictionary<string, IDictionary<string, object>>()
                {
                    {
                        PopupComponents.SNACKBAR, new Dictionary<string, object>()
                        {
                            { nameof(PEnqueuedSnackbars.Closeable), true },
                            { nameof(PEnqueuedSnackbars.Position), SnackPosition.BottomRight },
                            { nameof(PEnqueuedSnackbars.Text), true },
                            { nameof(PEnqueuedSnackbars.Elevation), new StringNumber(2) },
                            { nameof(PEnqueuedSnackbars.MaxCount), 5 },
                            { nameof(PEnqueuedSnackbars.Timeout), 5000 },
                        }
                    }
                };
            });

            //本地储存
            services.AddBlazoredLocalStorage().AddBlazoredSessionStorage();
            //添加公共组件
            services.AddCnGalComponents();
            //全局缓存数据
            services.AddScoped<IDataCacheService, DataCatcheService>();
            //用户
            services.AddScoped<IUserService, Userservice>();


            //工具箱
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IEntryService, EntryService>()
                .AddScoped<IArticleService, ArticleService>()
                .AddScoped<IImageService, ImageService>()
                .AddScoped<IVideoService, VideoService>();

            //空白MAUI服务
            services.AddScoped<IMauiService, MauiService>();

            //事件
            services.AddScoped<IEventService, EventService>();
            //结构化数据
            services.AddScoped<IStructuredDataService, StructuredDataService>();

            // 看板娘Live2D
            services.AddKanbanLive2D();


            return services;
        }
    }
}
