using robotManager.FiniteStateMachine;
using System;
using System.Linq;
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
        private ILuaStatusFrameManager _luaStatusFrameManager;

        internal bool InitialSetup()
        {
            try
            {
                _cache = new Cache();
                _cache.Initialize();
                _entityCache = new EntityCache();
                _entityCache.Initialize();
                _profileManager = new ProfileManager(_entityCache, _cache);
                _profileManager.Initialize();
                _partyChatManager = new PartyChatManager(_entityCache, _profileManager);
                _partyChatManager.Initialize();
                _targetingManager = new TargetingManager(_entityCache, _cache);
                _targetingManager.Initialize();
                _luaStatusFrameManager = new LuaStatusFrameManager(_cache, _entityCache, _profileManager);
                _luaStatusFrameManager.Initialize();
                //Update Spellbook after Initialization
                SpellManager.UpdateSpellBook();

                //Load CustomClass
                CustomClass.LoadCustomClass();

                //FSM
                _fsm.States.Clear();

                // List of states, top of the list is highest priority
                State[] states = new State[]
                {
                    new Relogger(),
                    new Pause(),
                    new Dead(_entityCache, _profileManager),
                    new MyMacro(),
                    new SlaveCombat(_cache, _entityCache),
                    new TankCombat(_cache, _entityCache),
                    new Regeneration(),
                    new GroupRevive(_cache, _entityCache),
                    new Looting(),
                    new OpenSatchel(_cache),
                    new ToTown(),
                    new Trainers(),
                    new GroupInviteAccept(_cache),
                    new GroupInvite(_cache, _entityCache),
                    new GroupProposal(_cache, _entityCache),
                    new GroupQueueAccept(_cache, _entityCache),
                    new GroupQueue(_cache, _entityCache),
                    new WaitRest(_cache, _entityCache),
                    new CheckPathAhead(_entityCache, _partyChatManager),
                    new ClearPathCombat(_entityCache),
                    new LeaveDungeon(_cache, _entityCache, _profileManager),
                    new DungeonLogic(_entityCache, _profileManager, _partyChatManager, _cache),
                    new AntiAfk(),
                    new Idle(),
                };
                // Reverse the array so hiest prio states have the highest index
                states = states.Reverse().ToArray();
                // Add the states with correct priority
                for (int i = 0; i < states.Length; i++)
                {
                    Logger.Log($"State:{i}, Prio:{states[i].DisplayName}");
                    states[i].Priority = i;
                    _fsm.AddState(states[i]);
                }

                //_fsm.AddState(new Relogger { Priority = 200 });
                //_fsm.AddState(new Pause { Priority = 150 });
                //_fsm.AddState(new Dead(_cache, _entityCache, _profileManager));
                //_fsm.AddState(new OpenSatchel(_cache, 24));
                //_fsm.AddState(new MyMacro { Priority = 23 });
                //_fsm.AddState(new Regeneration { Priority = 22 });
                //_fsm.AddState(new NPCScanState { Priority = 21 });
                //_fsm.AddState(new ToTown { Priority = 20 });
                //_fsm.AddState(new Trainers { Priority = 19 });
                //_fsm.AddState(new GroupInviteAccept(_cache));
                //_fsm.AddState(new GroupInvite(_cache, _entityCache));
                //_fsm.AddState(new GroupProposal(_cache, _entityCache));
                //_fsm.AddState(new GroupQueueAccept(_cache, _entityCache));
                //_fsm.AddState(new GroupQueue(_cache, _entityCache));
                //_fsm.AddState(new LeaveDungeon(_cache, _entityCache, _profileManager));
                //_fsm.AddState(new GroupRevive(_cache, _entityCache));
                //_fsm.AddState(new WaitRest(_cache, _entityCache));              
                //_fsm.AddState(new MovementSlave(_cache, _entityCache));
                //_fsm.AddState(new SlaveCombat(_cache, _entityCache));
                //_fsm.AddState(new TankCombat(_cache, _entityCache));
                //_fsm.AddState(new ClearPathCombat(_cache, _entityCache));
                //_fsm.AddState(new Looting { Priority = 6 });
                //_fsm.AddState(new DungeonLogic(_cache, _entityCache, _profileManager));
                //_fsm.AddState(new Idle { Priority = 1 });

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
                _partyChatManager?.Dispose();
                _fsm?.StopEngine();
                _cache?.Dispose();
                _entityCache?.Dispose();
                _profileManager?.Dispose();
                _targetingManager?.Dispose();
                _luaStatusFrameManager?.Dispose();
                Fight.StopFight();
            }
            catch (Exception e)
            {
                Logger.LogError("Bot > Bot > Dispose: " + e);
            }
        }

    }
}
