using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CnGalUWP.Pages.Entries
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Index : Page
    {
        public Index()
        {
            InitializeComponent();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 1000)
            {
                MainInforCard.SetValue(RelativePanel.AlignRightWithPanelProperty, false);
                MainInforCard.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
                MainInforCard.Width = 400;
                MainInforCard.Height = double.NaN;
            }
            else if (e.NewSize.Width <= 1000 && e.NewSize.Width > 700)
            {
                MainInforCard.SetValue(RelativePanel.AlignRightWithPanelProperty, true);
                MainInforCard.SetValue(RelativePanel.AlignBottomWithPanelProperty, false);
                MainInforCard.Width = double.NaN;
                MainInforCard.Height = double.NaN;
            }
            else
            {
                MainInforCard.SetValue(RelativePanel.AlignRightWithPanelProperty, true);
                MainInforCard.SetValue(RelativePanel.AlignBottomWithPanelProperty, false);
                MainInforCard.Width = double.NaN;
                MainInforCard.Height = 750;
            }
        }
    }
}
