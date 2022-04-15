using System.Windows.Controls;
using WholesomeDungeonCrawler.Bot;

namespace WholesomeDungeonCrawler.GUI
{
    /// <summary>
    /// Interaction logic for ProductSettingsControl.xaml
    /// </summary>
    public partial class ProductSettingsControl : UserControl
    {
        public ProductSettingsControl()
        {
            InitializeComponent();
            DataContext = WholesomeDungeonCrawlerSettings.CurrentSetting;
        }
    }
}
