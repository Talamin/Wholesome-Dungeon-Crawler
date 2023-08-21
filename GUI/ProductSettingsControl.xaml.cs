using System;
using System.Windows;
using System.Windows.Controls;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.GUI
{
    public partial class ProductSettingsControl : UserControl
    {
        public ProductSettingsControl()
        {
            InitializeComponent();
            DataContext = WholesomeDungeonCrawlerSettings.CurrentSetting;
            cbLFGRole.ItemsSource = Enum.GetValues(typeof(LFGRoles));
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

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refresh client LFD list
                Lua.LuaDoString($@"
                    if not LFDQueueFrame:IsVisible() then
                        LFDMicroButton:Click();
                    end
                    if not LFDQueueFrameSpecific:IsVisible() then
                        LFDQueueFrameTypeDropDownButton:Click();
                        DropDownList1Button1:Click();
                    end
                    LFDMicroButton:Click();
                ");
                AdvancedSettings advancedSettings = new AdvancedSettings();
                advancedSettings.ShowDialog();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        private void cbLFGRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WholesomeDungeonCrawlerSettings.CurrentSetting.Save();

            switch (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole)
            {
                case LFGRoles.Tank:
                    Lua.LuaDoString("SetLFGRoles(false, true, false, false)");
                    break;
                case LFGRoles.Heal:
                    Lua.LuaDoString("SetLFGRoles(false, false, true, false)");
                    break;
                case LFGRoles.MDPS:
                    Lua.LuaDoString("SetLFGRoles(false, false, false, true)");
                    break;
                case LFGRoles.RDPS:
                    Lua.LuaDoString("SetLFGRoles(false, false, false, true)");
                    break;
            }
        }
    }
}
