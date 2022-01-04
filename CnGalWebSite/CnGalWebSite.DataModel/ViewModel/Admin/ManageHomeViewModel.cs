using CnGalWebSite.DataModel.Model;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ManageHomeViewModel
    {
        public List<Carousel> Carousels { get; set; }
        public List<FriendLink> Links { get; set; }

        public string AppImage { get; set; }
        public string UserImage { get; set; }
        public string UserBackgroundImage { get; set; }
        public string BackgroundImage { get; set; }
        public string CertificateImage { get; set; }
    }

}
