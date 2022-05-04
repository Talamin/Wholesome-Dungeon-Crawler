﻿using robotManager.FiniteStateMachine;
using System;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Manager;
using WholesomeDungeonCrawler.States;
using WholesomeDungeonCrawler.States.ProfileStates;
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
        private IProfile _profile;

        internal bool InitialSetup()
        {
            try
            {
                _cache = new Cache();
                _cache.Initialize();
                _entityCache = new EntityCache();
                _entityCache.Initialize();
                _profileManager = new ProfileManager(_entityCache);
                _profileManager.Initialize();
                _profile = new ProfileModel();
                //Update Spellbook after Initialization
                SpellManager.UpdateSpellBook();

                //Load CustomClass
                CustomClass.LoadCustomClass();

                //FSM
                _fsm.States.Clear();
                _fsm.AddState(new Relogger { Priority = 200 });
                _fsm.AddState(new Pause { Priority = 150 });
                //Custom  State

                //_fsm.AddState(new GroupInviteAccept(_cache, 30));
                //_fsm.AddState(new GroupInvite(_cache, _entityCache, 29));
                //_fsm.AddState(new GroupQueue(_cache, _entityCache, 28));
                //_fsm.AddState(new GroupQueueAccept(_cache, 27));

                //_fsm.AddState(new GroupRevive(_cache, _entityCache, 26));
                //_fsm.AddState(new SExecute(_cache, _entityCache, 20));
                //_fsm.AddState(new SGoTo(_cache, _entityCache, 20));
                //_fsm.AddState(new SInteractWith(_cache, _entityCache, _profile, 20));
                _fsm.AddState(new SMoveAlongPath(_cache, _entityCache, _profile, 20));
                //_fsm.AddState(new SMoveToUnit(_cache, _entityCache, 20));
                //_fsm.AddState(new SPickupObject(_cache, _entityCache, 20));


                //_fsm.AddState(new OpenSatchel(_cache, 17));
                //_fsm.AddState(new Loot(_cache, _entityCache, 16));

                //Default State
                //_fsm.AddState(new MyMacro { Priority = 12 });
                //_fsm.AddState(new Regeneration { Priority = 10 });
                //_fsm.AddState(new NPCScanState { Priority = 5 });
                //_fsm.AddState(new ToTown { Priority = 4 });
                //_fsm.AddState(new Trainers { Priority = 3 });
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
                _fsm.StopEngine();
                _cache.Dispose();
                _entityCache.Dispose();
                _profileManager.Dispose();
                Fight.StopFight();
            }
            catch (Exception e)
            {
                Logger.LogError("Bot > Bot > Dispose: " + e);
            }
        }

    }
}
