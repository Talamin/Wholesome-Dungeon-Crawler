using robotManager.FiniteStateMachine;
using robotManager.Products;
using System;
using WholesomeDungeonCrawler.Helpers;
using WholesomeToolbox;
using static robotManager.Helpful.Logging;


public class Main : IProduct
{
    private static readonly Engine FSM = new Engine();
    public static readonly string ProductName = "Wholesome Dungeon Crawler";
    public System.Windows.Controls.UserControl Settings => throw new NotImplementedException();
    public bool IsStarted => throw new NotImplementedException();

    public void Initialize()
    {
        try
        { 

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

