using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class AddUserIntegralModel
    {
        public string UserId { get; set; }

        public int Count { get; set; }

        public string Note { get; set; }

        public UserIntegralType Type { get; set; }
    }
}
