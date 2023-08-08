using robotManager.Helpful;
using robotManager.Products;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using WholesomeDungeonCrawler.Bot;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.GUI;
using WholesomeDungeonCrawler.Helpers;
using WholesomeToolbox;
using wManager;
using wManager.Plugin;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : IProduct
{
    private readonly CrawlerBot _crawler = new CrawlerBot();
    private ProductSettingsControl _settingsUserControl;
    private readonly string _productVersion = FileVersionInfo.GetVersionInfo(Others.GetCurrentDirectory + $@"\Products\WholesomeDungeonCrawler.dll").FileVersion;
    private readonly string _productName = "Wholesome Dungeon Crawler";
    public bool IsStarted { get; private set; }

    public void Initialize()
    {
        try
        {
            WholesomeDungeonCrawlerSettings.Load();
            Logger.Log($"{_productName} version {_productVersion} loaded");
        }
        catch (Exception e)
        {
            Logging.WriteError("Main -> Initialize(): " + e);
        }
    }

    public void Start()
    {
        try
        {
            /*
            if (AutoUpdater.CheckUpdate(_productVersion))
            {
                return;
            }
            */

            if (ObjectManager.Me.Faction == 35)
            {
                Logger.LogError($"WARNING: You are you in GM mode. Unexpected behavior will happen.");
            }

            if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.Unspecified)
            {
                Logger.LogError($"You must select a role in the product settings first!");
                return;
            }
            if (WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole != LFGRoles.Tank
                && string.IsNullOrEmpty(WholesomeDungeonCrawlerSettings.CurrentSetting.TankName.Trim()))
            {
                Logger.LogError($"You must enter the name of your tank in the product settings first!");
                return;
            }

            if (_crawler.InitialSetup())
            {
                Logger.Log("Started");
                PluginsManager.LoadAllPlugins();
                WTSettings.AddRecommendedOffmeshConnections();
                WTTransport.AddRecommendedTransportsOffmeshes();

                if (wManagerSetting.CurrentSetting.CloseIfPlayerTeleported == true
                    || wManagerSetting.CurrentSetting.WallDistancePathFinder != 1)
                {
                    wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
                    wManagerSetting.CurrentSetting.WallDistancePathFinder = 1;
                    wManagerSetting.CurrentSetting.Save();
                }
                IsStarted = true;
            }
        }
        catch (Exception e)
        {
            IsStarted = false;
            Logger.LogError("Main -> Start(): " + e);
        }
    }

    public void Stop()
    {
        try
        {
            MovementManager.StopMove();
            IsStarted = false;
            PluginsManager.DisposeAllPlugins();
            _crawler.Dispose();
        }
        catch (Exception e)
        {
            Logger.LogError("Main -> Stop(): " + e);
        }
    }

    public void Dispose()
    {
        try
        {

        }
        catch (Exception e)
        {
            Logger.LogError("Main -> Dispose(): " + e);
        }
    }

    // GUI
    public UserControl Settings
    {
        get
        {
            try
            {
                if (_settingsUserControl == null)
                {
                    _settingsUserControl = new ProductSettingsControl();
                }
                return _settingsUserControl;
            }
            catch (Exception e)
            {
                Logger.Log("> Main > Settings(): " + e);
            }

            return null;
        }
    }
}

