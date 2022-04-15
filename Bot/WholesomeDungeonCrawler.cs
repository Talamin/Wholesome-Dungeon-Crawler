using robotManager.FiniteStateMachine;
using System;
using wManager.Wow.Bot.States;
using wManager.Wow.Helpers;
using Wholesome_Dungeon_Crawler.Helpers;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.States;

namespace WholesomeDungeonCrawler.Bot
{
    internal class CrawlerBot
    {
        private readonly Engine _fsm = new Engine();
        private Cache _cache = new Cache();
        internal bool InitialSetup()
        {
            try
            {
                _cache.Initialize();

                //Update Spellbook after Initialization
                SpellManager.UpdateSpellBook();

                //Load CustomClass
                CustomClass.LoadCustomClass();

                //FSM
                _fsm.States.Clear();
                _fsm.AddState(new Relogger { Priority = 200 });
                _fsm.AddState(new Pause { Priority = 150 });
                _fsm.AddState(new OpenSatchel { Priority = 14 });
                _fsm.AddState(new Loot { Priority = 13 });
                _fsm.AddState(new MyMacro { Priority = 12 });
                _fsm.AddState(new Regeneration { Priority = 10 });
                _fsm.AddState(new NPCScanState { Priority = 5 });
                _fsm.AddState(new ToTown { Priority = 4 });
                _fsm.AddState(new Trainers { Priority = 3 });
                _fsm.AddState(new Idle { Priority = 1 });
                
                _fsm.States.Sort();

                _fsm.StartEngine(10, "WholesomeDungeonCrawler");

                StopBotIf.LaunchNewThread();

                return true;
            }
            catch(Exception e)
            {
                Dispose();
                Logger.LogError("Bot > Bot > Pulse: " + e);
                return false;
            }
        }

        internal void Dispose()
        {
            try
            {
                CustomClass.DisposeCustomClass();
                _fsm.StopEngine();
                _cache.Dispose();
                Fight.StopFight();
            }
            catch(Exception e)
            {
                Logger.LogError("Bot > Bot > Dispose: " + e);
            }
        }

    }
}
