using robotManager.FiniteStateMachine;
using robotManager.Products;
using System;
using Wholesome_Dungeon_Crawler;
using Wholesome_Dungeon_Crawler.Bot;
using Wholesome_Dungeon_Crawler.Helpers;
using WholesomeToolbox;
using static robotManager.Helpful.Logging;


public class Main : IProduct
{
    public System.Windows.Controls.UserControl Settings => throw new NotImplementedException();
    public bool IsStarted => throw new NotImplementedException();
    private readonly WholesomeDungeonCrawler _crawler = new WholesomeDungeonCrawler();

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

        }
        catch (Exception e)
        {
            Logger.LogError("Main -> Start(): " + e);
        }
    }

    public void Stop()
    {
        try
        {
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

