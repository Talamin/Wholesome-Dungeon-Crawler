using robotManager.FiniteStateMachine;
using System;
using wManager.Wow.Bot.States;
using wManager.Wow.Helpers;
using WholesomeToolbox;
using Wholesome_Dungeon_Crawler.Helpers;

namespace Wholesome_Dungeon_Crawler.Bot
{
    internal class CrawlerBot
    {
        private readonly Engine _fsm = new Engine();

        internal bool InitialSetup()
        {
            try
            {
                //Update Spellbook after Initialization
                SpellManager.UpdateSpellBook();

                //Load CustomClass
                CustomClass.LoadCustomClass();

                //FSM
                _fsm.States.Clear();

                //_fsm.AddState(new BattlegrounderCombination { Priority = 15 });
                //_fsm.AddState(new Resurrect { Priority = 13 });
                //_fsm.AddState(new MyMacro { Priority = 12 });
                //_fsm.AddState(new wManager.Wow.Bot.States.IsAttacked { Priority = 13 });
                //_fsm.AddState(new BattlePetState { Priority = 11 });
                //_fsm.AddState(new Looting { Priority = 9 });

                //_fsm.AddState(new Farming { Priority = 8 });
                //_fsm.AddState(new MillingState { Priority = 7 });
                //_fsm.AddState(new ProspectingState { Priority = 6 });
                //_fsm.AddState(new FlightMasterTakeTaxiState { Priority = 6 });
                _fsm.AddState(new ToTown { Priority = 4 });
                //_fsm.AddState(new FlightMasterDiscoverState { Priority = 3 });
                //_fsm.AddState(new Talents { Priority = 3 });
                _fsm.AddState(new Trainers { Priority = 3 });

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
                Fight.StopFight();
            }
            catch(Exception e)
            {
                Logger.LogError("Bot > Bot > Dispose: " + e);
            }
        }

    }
}
