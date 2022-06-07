using robotManager.FiniteStateMachine;
using System;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.States;
using wManager.Wow.Bot.States;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Bot
{
    internal class CrawlerBot
    {
        private readonly Engine _fsm = new Engine();
        private ICache _cache;
        private IEntityCache _entityCache;
        private IProfileManager _profileManager;
        private ITargetingManager _targetingManager;
        private IPartyChatManager _partyChatManager;
        //private MovementSlaveZero _movementSlaveZero;

        internal bool InitialSetup()
        {
            try
            {
                _cache = new Cache();
                _cache.Initialize();
                _entityCache = new EntityCache();
                _entityCache.Initialize();
                _partyChatManager = new PartyChatManager(_entityCache);
                _partyChatManager.Initialize();
                _profileManager = new ProfileManager(_entityCache, _cache);
                _profileManager.Initialize();
                _targetingManager = new TargetingManager(_entityCache, _cache);
                _targetingManager.Initialize();
                //Update Spellbook after Initialization
                SpellManager.UpdateSpellBook();

                //Load CustomClass
                CustomClass.LoadCustomClass();

                //FSM
                _fsm.States.Clear();
                _fsm.AddState(new Relogger { Priority = 200 });
                _fsm.AddState(new Pause { Priority = 150 });
                //Custom  State
                _fsm.AddState(new Dead(_cache, _entityCache, _profileManager, 25));
                _fsm.AddState(new OpenSatchel(_cache, 24));
                _fsm.AddState(new MyMacro { Priority = 23 });
                _fsm.AddState(new Regeneration { Priority = 22 });
                _fsm.AddState(new NPCScanState { Priority = 21 });
                _fsm.AddState(new ToTown { Priority = 20 });
                _fsm.AddState(new Trainers { Priority = 19 });

                _fsm.AddState(new GroupInviteAccept(_cache, 18));
                _fsm.AddState(new GroupInvite(_cache, _entityCache, 17));
                _fsm.AddState(new GroupProposal(_cache, 16));
                _fsm.AddState(new GroupQueueAccept(_cache, _entityCache, 16));
                _fsm.AddState(new GroupQueue(_cache, _entityCache, 15));

                _fsm.AddState(new LeaveDungeon(_cache, _entityCache, _profileManager, 14));


                _fsm.AddState(new GroupRevive(_cache, _entityCache, 13));
                _fsm.AddState(new WaitRest(_cache, _entityCache, 12));
                //_fsm.AddState(new MovementSlaveBETA(_cache, _entityCache, 11));
                //_fsm.AddState(new MovementSlave(_cache, _entityCache, 11));


                _fsm.AddState(new SlaveCombat(_cache, _entityCache, 9));
                _fsm.AddState(new TankCombat(_cache, _entityCache, 8));
                _fsm.AddState(new ClearPathCombat(_cache, _entityCache, 7));
                _fsm.AddState(new Looting { Priority = 6 });
                /*
                _movementSlaveZero = new MovementSlaveZero(_cache, _entityCache, _profileManager, 5);
                _movementSlaveZero.Initialize();
                _fsm.AddState(_movementSlaveZero);
                */
                _fsm.AddState(new DungeonLogic(_cache, _entityCache, _profileManager, 4));


                //Default State
                _fsm.AddState(new Idle { Priority = 1 });

                _fsm.States.Sort();

                _fsm.StartEngine(10, "WholesomeDungeonCrawler");

                StopBotIf.LaunchNewThread();

                return true;
            }
            catch (Exception e)
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
                //_movementSlaveZero.Dispose();
                _partyChatManager?.Dispose();
                _fsm?.StopEngine();
                _cache?.Dispose();
                _entityCache?.Dispose();
                _profileManager?.Dispose();
                _targetingManager?.Dispose();
                Fight.StopFight();
            }
            catch (Exception e)
            {
                Logger.LogError("Bot > Bot > Dispose: " + e);
            }
        }

    }
}
