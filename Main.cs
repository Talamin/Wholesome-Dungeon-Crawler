using robotManager.Products;
using System;
using Wholesome_Dungeon_Crawler.Bot;
using Wholesome_Dungeon_Crawler.Helpers;


public class Main : IProduct
{
    public System.Windows.Controls.UserControl Settings => throw new NotImplementedException();
    private readonly CrawlerBot _crawler = new CrawlerBot();

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
}

