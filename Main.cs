using robotManager.Products;
using System;
using System.Windows.Controls;
using WholesomeDungeonCrawler.Bot;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.GUI;
using WholesomeDungeonCrawler.Helpers;

public class Main : IProduct
{
    private readonly CrawlerBot _crawler = new CrawlerBot();
    private ProductSettingsControl _settingsUserControl;
    private readonly string _productVersion = "0.0.01";
    private readonly string _productName = "Wholesome Dungeon Crawler";
    private readonly string _fileName = "WholesomeDungeonCrawler";
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
            Logger.LogError("Main -> Initialize(): " + e);
        }
    }

    public void Start()
    {
        try
        {
            /*
            if (AutoUpdater.CheckUpdate(_productVersion, _fileName))
            {
                return;
            }
            */
            if (_crawler.InitialSetup())
            {
                Logger.Log("Started");
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
            IsStarted = false;
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

