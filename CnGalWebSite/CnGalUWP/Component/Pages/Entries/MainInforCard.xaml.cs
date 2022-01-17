using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CnGalUWP.Component.Pages.Entries
{
    public sealed partial class MainInforCard : UserControl
    {
        public MainInforCard()
        {
            InitializeComponent();
        }

        private void GroupLink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 700 || e.NewSize.Width > 1000)
            {
                //设置布局概览
                MainImage.Margin = new Thickness(0, 70, 0, 0);
                MainImage.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
                MainImage.SetValue(RelativePanel.AlignRightWithPanelProperty, true);

                TitleCard.SetValue(RelativePanel.RightOfProperty, null);
                TitleCard.SetValue(RelativePanel.BelowProperty, MainImage);
                TitleCard.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
                TitleCard.SetValue(RelativePanel.AlignRightWithPanelProperty, true);

                NameText.Margin = new Thickness(0, 30, 0, 0);
                NameText.TextAlignment = TextAlignment.Center;
                NameText.HorizontalAlignment = HorizontalAlignment.Center;
                NameText.Width = 350;

                GroupLink.Margin = new Thickness(0, 5, 0, 0);
                GroupLink.HorizontalAlignment = HorizontalAlignment.Center;

                RecordButton.Margin = new Thickness(0, 60, 0, 0);

                spRecord.SetValue(RelativePanel.BelowProperty, TitleCard);
                spRecord.SetValue(RelativePanel.AlignTopWithPanelProperty, false);
                spRecord.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
                spRecord.SetValue(RelativePanel.AlignRightWithPanelProperty, true);

                CountShowPanel.Margin = new Thickness(0, 20, 0, 0);
                CountShowPanel.SetValue(RelativePanel.BelowProperty, RecordButton);
                CountShowPanel.SetValue(RelativePanel.AlignLeftWithProperty, RecordButton);
                CountShowPanel.SetValue(RelativePanel.AlignRightWithProperty, RecordButton);

                DespritionText.Margin = new Thickness(0, 10, 0, 0);
                DespritionText.SetValue(RelativePanel.RightOfProperty, null);
                DespritionText.SetValue(RelativePanel.LeftOfProperty, null);
                DespritionText.SetValue(RelativePanel.BelowProperty, spRecord);
                DespritionText.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
                DespritionText.SetValue(RelativePanel.AlignRightWithPanelProperty, true);
                DespritionText.Width = 350;

                TagList.Margin = new Thickness(0, 30, 0, 20);
                TagList.HorizontalAlignment = HorizontalAlignment.Center;
                TagList.SetValue(RelativePanel.BelowProperty, DespritionText);
                TagList.SetValue(RelativePanel.AlignLeftWithProperty, null);
                TagList.SetValue(RelativePanel.AlignRightWithProperty, null);
                TagList.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
                TagList.SetValue(RelativePanel.AlignRightWithPanelProperty, true);

                spOtherButton.Margin = new Thickness(0, 0, 25, 25);


                //设置边距
                if (e.NewSize.Height > 800 || (e.NewSize.Width < 700 && e.NewSize.Width != 400))
                {
                    RecordButton.Margin = new Thickness(0, 60, 0, 0);
                    CountShowPanel.Margin = new Thickness(0, 70, 0, 0);
                    DespritionText.Margin = new Thickness(0, 60, 0, 0);
                    NameText.Margin = new Thickness(0, 30, 0, 0);
                    DespritionText.Visibility = Visibility.Visible;
                    TagList.Visibility = Visibility.Visible;
                    MainImage.Visibility = Visibility.Visible;
                    MainImageExitStoryboard.Begin();
                }
                else if (e.NewSize.Height <= 800 && e.NewSize.Height > 750)
                {
                    RecordButton.Margin = new Thickness(0, 30, 0, 0);
                    CountShowPanel.Margin = new Thickness(0, 50, 0, 0);
                    DespritionText.Margin = new Thickness(0, 30, 0, 0);
                    NameText.Margin = new Thickness(0, 30, 0, 0);
                    DespritionText.Visibility = Visibility.Visible;
                    TagList.Visibility = Visibility.Visible;
                    MainImage.Visibility = Visibility.Visible;
                    MainImageExitStoryboard.Begin();
                }
                else if (e.NewSize.Height <= 800 && e.NewSize.Height > 600)
                {
                    RecordButton.Margin = new Thickness(0, 30, 0, 0);
                    CountShowPanel.Margin = new Thickness(0, 50, 0, 0);
                    DespritionText.Margin = new Thickness(0, 30, 0, 0);
                    NameText.Margin = new Thickness(0, 30, 0, 0);
                    DespritionText.Visibility = Visibility.Collapsed;
                    TagList.Visibility = Visibility.Visible;
                    MainImage.Visibility = Visibility.Visible;
                    MainImageExitStoryboard.Begin();
                }
                else if (e.NewSize.Height <= 600 && e.NewSize.Height > 550)
                {
                    RecordButton.Margin = new Thickness(0, 30, 0, 0);
                    CountShowPanel.Margin = new Thickness(0, 50, 0, 0);
                    DespritionText.Margin = new Thickness(0, 30, 0, 0);
                    NameText.Margin = new Thickness(0, 30, 0, 0);
                    DespritionText.Visibility = Visibility.Collapsed;
                    TagList.Visibility = Visibility.Collapsed;
                    MainImage.Visibility = Visibility.Visible;
                    MainImageExitStoryboard.Begin();
                }
                else
                {
                    RecordButton.Margin = new Thickness(0, 30, 0, 0);
                    CountShowPanel.Margin = new Thickness(0, 50, 0, 0);
                    DespritionText.Margin = new Thickness(0, 30, 0, 0);
                    NameText.Margin = new Thickness(0, 50, 0, 0);
                    DespritionText.Visibility = Visibility.Collapsed;
                    TagList.Visibility = Visibility.Collapsed;
                    MainImage.Visibility = Visibility.Collapsed;
                    MainImageEnterStoryboard.Begin();
                }

            }
            else
            {
                //设置布局概览
                MainImage.SetValue(RelativePanel.AlignLeftWithPanelProperty, false);
                MainImage.SetValue(RelativePanel.AlignRightWithPanelProperty, false);
                MainImage.Margin = new Thickness(25, 25, 15, 0);

                TitleCard.SetValue(RelativePanel.BelowProperty, null);
                TitleCard.SetValue(RelativePanel.RightOfProperty, MainImage);
                TitleCard.SetValue(RelativePanel.AlignLeftWithPanelProperty, false);
                TitleCard.SetValue(RelativePanel.AlignRightWithPanelProperty, true);

                NameText.Margin = new Thickness(0, 18, 0, 0);
                NameText.TextAlignment = TextAlignment.Left;
                NameText.HorizontalAlignment = HorizontalAlignment.Left;
                NameText.Width = double.NaN;

                GroupLink.Padding = new Thickness(0, 0, 0, 0);
                GroupLink.HorizontalAlignment = HorizontalAlignment.Left;

                RecordButton.Margin = new Thickness(25, 25, 25, 0);

                spRecord.SetValue(RelativePanel.BelowProperty, null);
                spRecord.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
                spRecord.SetValue(RelativePanel.AlignLeftWithPanelProperty, false);
                spRecord.SetValue(RelativePanel.AlignRightWithPanelProperty, true);

                CountShowPanel.Margin = new Thickness(0, 20, 0, 0);
                CountShowPanel.SetValue(RelativePanel.BelowProperty, RecordButton);
                CountShowPanel.SetValue(RelativePanel.AlignLeftWithProperty, RecordButton);
                CountShowPanel.SetValue(RelativePanel.AlignRightWithProperty, RecordButton);

                DespritionText.Margin = new Thickness(0, 10, 0, 0);
                DespritionText.SetValue(RelativePanel.RightOfProperty, MainImage);
                DespritionText.SetValue(RelativePanel.LeftOfProperty, spRecord);
                DespritionText.SetValue(RelativePanel.BelowProperty, TitleCard);
                DespritionText.SetValue(RelativePanel.AlignLeftWithPanelProperty, false);
                DespritionText.SetValue(RelativePanel.AlignRightWithPanelProperty, false);
                DespritionText.Width = double.NaN;

                TagList.Margin = new Thickness(0, 20, 0, 20);
                TagList.HorizontalAlignment = HorizontalAlignment.Left;
                TagList.SetValue(RelativePanel.BelowProperty, DespritionText);
                TagList.SetValue(RelativePanel.AlignLeftWithProperty, DespritionText);
                TagList.SetValue(RelativePanel.AlignRightWithProperty, DespritionText);
                TagList.SetValue(RelativePanel.AlignLeftWithPanelProperty, false);
                TagList.SetValue(RelativePanel.AlignRightWithPanelProperty, false);

                spOtherButton.Margin = new Thickness(0, 0, 25, 25);

            }
        }
    }
}
