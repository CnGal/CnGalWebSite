using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Others
{
    public enum IconType
    {
        Entry,
        Article,
        Tag,
        Tags,
        Periphery,
        Video,
        Lottery,
        Vote,
        News,
        RecentlyPublish,
        RecentlyEdit,
        ComingSoon,
        Link,
        Notice,
        Game,
        Staff,
        Role,
        Group,
        Dub,
        Make,
        Publisher,
        Participate,
        SpecialThanks,
        Infor,
        Image,
        PhotoAlbum,
        GameRecord,
        Audio,
        MainPage,
        Home,
        Steam,
        Export,
        Comment,
        Login,
        Create,
        SeeDetails,
        Catalogs,
        EditRecord,
        Note,
        Gift,
        User,
        Users,
        Read
    }

    public static class IconTypeHelper
    {
        public static string ToIconString(this IconType type)
        {
            switch (type)
            {
                case IconType.Entry:
                    return "mdi-archive";
                case IconType.Article:
                    return "mdi-script-text";
                case IconType.Tag:
                    return "mdi-tag";
                case IconType.Tags:
                    return "mdi-tag-multiple ";
                case IconType.Video:
                    return "mdi-animation-play ";
                case IconType.Periphery:
                    return "mdi-basket";
                case IconType.Lottery:
                    return "mdi-wallet-giftcard";
                case IconType.Vote:
                    return "mdi-vote";
                case IconType.News:
                    return "mdi-newspaper-variant-outline ";
                case IconType.RecentlyPublish:
                    return "mdi-publish ";
                case IconType.RecentlyEdit:
                    return "mdi-history ";
                case IconType.ComingSoon:
                    return "mdi-av-timer ";
                case IconType.Link:
                    return "mdi-link";
                case IconType.Notice:
                    return "mdi-bullhorn-variant-outline ";
                case IconType.Game:
                    return "mdi-gamepad-square ";
               case IconType.Staff:
                    return "mdi-sitemap";
                case IconType.Role:
                    return "mdi-account-supervisor ";
                case IconType.Group:
                    return "mdi-group ";
                case IconType.Dub:
                    return "mdi-microphone ";
                case IconType.Make:
                    return "mdi-auto-fix";
                case IconType.Publisher:
                    return "mdi-send  ";
                case IconType.Participate:
                    return "mdi-sitemap ";
                case IconType.SpecialThanks:
                    return "mdi-flower-outline ";
                case IconType.Infor:
                    return "mdi-information-outline  ";
                case IconType.Image:
                    return "mdi-image";
                case IconType.GameRecord:
                    return "mdi-message-draw ";
                case IconType.PhotoAlbum:
                    return "mdi-folder-multiple-image ";
                case IconType.Audio:
                    return "mdi-volume-high";
                case IconType.MainPage:
                    return "mdi-text-box-outline ";
                case IconType.Home:
                    return "mdi-home ";
                case IconType.Steam:
                    return "mdi-steam ";
                case IconType.Export:
                    return "mdi-database-export";
                case IconType.Comment:
                    return "mdi-message-reply-text ";
                case IconType.Login:
                    return "mdi-login";
                case IconType.Create:
                    return "mdi-plus";
                case IconType.SeeDetails:
                    return "mdi-share-all-outline";
                case IconType.Catalogs:
                    return "mdi-format-list-bulleted-square ";
                case IconType.EditRecord:
                    return "mdi-file-document-edit-outline  ";
                case IconType.Note:
                    return "mdi-note-outline ";
                case IconType.Gift:
                    return "mdi-gift ";
                case IconType.User:
                    return "mdi-account ";
                case IconType.Users:
                    return "mdi-account-multiple  ";
                case IconType.Read:
                    return "mdi-eye";
                default:
                    return "";
            }
            
        }
    }
}
