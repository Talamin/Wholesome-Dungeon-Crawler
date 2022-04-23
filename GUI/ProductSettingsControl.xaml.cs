using System.Windows;
using System.Windows.Controls;
using WholesomeDungeonCrawler.CrawlerSettings;

namespace WholesomeDungeonCrawler.GUI
{
    public partial class ProductSettingsControl : UserControl
    {
        public ProductSettingsControl()
        {
            InitializeComponent();
            DataContext = WholesomeDungeonCrawlerSettings.CurrentSetting;
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            WholesomeDungeonCrawlerSettings.CurrentSetting.Save();
        }
    }
}
