using System;
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

        private void btnProfileEditor_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ProfileEditor profileEditor = new ProfileEditor();
                profileEditor.ShowDialog();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
            
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            WholesomeDungeonCrawlerSettings.CurrentSetting.Save();
        }
    }
}
