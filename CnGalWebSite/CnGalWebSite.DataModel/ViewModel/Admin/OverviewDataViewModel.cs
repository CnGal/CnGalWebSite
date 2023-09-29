using System;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class UserDataOverviewModel
    {
        public long UserTotalNumber { get; set; }

        public long UserVerifyEmailNumber { get; set; }
        public long UserSecondaryVerificationNumber { get; set; }
        public long UserActiveNumber { get; set; }
    }

}
