using robotManager.Products;
using System;
using System.Windows.Controls;
using Wholesome_Dungeon_Crawler.Helpers;
using WholesomeDungeonCrawler.Bot;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.GUI;

public class Main : IProduct
{
    //public System.Windows.Controls.UserControl Settings => throw new NotImplementedException();
    private readonly CrawlerBot _crawler = new CrawlerBot();
    private ProductSettingsControl _settingsUserControl;
    public bool IsStarted { get; private set; }

    public void Initialize()
    {
        try
        {
            WholesomeDungeonCrawlerSettings.Load();
        }
        catch(Exception e)
        {
            Logger.LogError("Main -> Initialize(): " + e);
        }
    }

    public void Start()
    {
        try
        {
            IsStarted = true;
            if(_crawler.InitialSetup())
            {
                Logger.Log("Started");
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

