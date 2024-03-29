﻿using robotManager.FiniteStateMachine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.States;
using wManager.Events;
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
        private IPathManager _pathManager;
        private IAvoidAOEManager _avoidAOEManager;
        private Stopwatch _stopWatchState = Stopwatch.StartNew();
        private int _lastCheckedStateIndex;

        private CheckPathAhead _checkPathAheadState;
        private Dead _deadState;
        private AvoidAOE _avoidAOEState;

        private void BeforeStateHandler(Engine engine, State state, CancelEventArgs cancelable)
        {
            if (_stopWatchState.ElapsedMilliseconds > 110)
            {
                Logger.LogError($"{engine.States[_lastCheckedStateIndex].DisplayName} took {_stopWatchState.ElapsedMilliseconds}ms");
            }
            Logger.Log($"CHECKING {state.DisplayName}");
            int stateIndex = engine.States.IndexOf(state);
            _lastCheckedStateIndex = stateIndex;
            _stopWatchState.Restart();
        }

        internal bool InitialSetup()
        {
            try
            {
                _cache = new Cache();
                _cache.Initialize();
                _entityCache = new EntityCache();
                _entityCache.Initialize();
                _avoidAOEManager = new AvoidAOEManager(_entityCache, _cache);
                _avoidAOEManager.Initialize();
                _pathManager = new PathManager();
                _pathManager.Initialize();
                _partyChatManager = new PartyChatManager();
                _partyChatManager.Initialize();
                _profileManager = new ProfileManager(_entityCache, _cache, _pathManager, _partyChatManager);
                _profileManager.Initialize();
                _targetingManager = new TargetingManager(_entityCache, _profileManager);
                _targetingManager.Initialize();
                _luaStatusFrameManager = new LuaStatusFrameManager(_cache, _entityCache, _profileManager);
                _luaStatusFrameManager.Initialize();

                SpellManager.UpdateSpellBook();
                CustomClass.LoadCustomClass();

                _fsm.States.Clear();

                _checkPathAheadState = new CheckPathAhead(_entityCache, _partyChatManager, _cache, _profileManager);
                _checkPathAheadState.Initialize();
                _deadState = new Dead(_entityCache, _profileManager);
                _deadState.Initalize();
                _avoidAOEState = new AvoidAOE(_entityCache, _avoidAOEManager, _cache);
                _avoidAOEState.Initialize();

                EventsLuaWithArgs.OnEventsLuaStringWithArgs += OnEventsLuaStringWithArgs;
                OthersEvents.OnMount += OnMount;
                //FiniteStateMachineEvents.OnBeforeCheckIfNeedToRunState += BeforeStateHandler;

                // List of states, top of the list is highest priority
                State[] states = new State[]
                {
                    new Relogger(),
                    new Unconnected(_entityCache),
                    //new IsCastSwitch(_entityCache),
                    new Pause(),
                    new LoadingScreenLock(_cache, _entityCache),
                    new LoadUnloadProfile(_cache, _entityCache, _profileManager),
                    new AntiStuck(_cache, _entityCache, _profileManager),
                    _avoidAOEState,
                    new DeadDive(_entityCache),
                    _deadState,
                    new ForceRegroup(_cache, _entityCache, _profileManager),
                    //new MyMacro(),
                    new Regen(_entityCache, _profileManager),
                    new OOCHeal(_cache, _entityCache),
                    new WaitCombatFlagsDisappear(_cache, _entityCache),
                    new CombatTurboLoot(_entityCache),
                    new SlaveCombat(_cache, _entityCache, _profileManager),
                    new TankCombat(_cache, _entityCache, _profileManager),
                    //new Regeneration(),
                    new GroupRevive(_cache, _entityCache),
                    new WaitRest(_cache, _entityCache),
                    new TurboLoot(_entityCache),
                    _checkPathAheadState,
                    new OpenSatchel(_cache),
                    new ToTown(),
                    new Trainers(),
                    new GroupInviteAccept(_cache),
                    new GroupInvite(_cache, _entityCache),
                    new GroupProposal(_cache, _entityCache),
                    new GroupQueueAccept(_cache, _entityCache),
                    new GroupQueue(_cache, _entityCache),
                    new ForceTownRun(_cache, _entityCache, _profileManager),
                    new RejoinDungeonAfterForcedTownRun(_cache, _entityCache, _profileManager),
                    new DungeonLogic(_profileManager, _cache),
                    //new AntiAfk(),
                    new Idle(),
                };

                // Reverse the array so highest prio states have the highest index
                states = states.Reverse().ToArray();

                // Add the states with correct priority
                for (int i = 0; i < states.Length; i++)
                {
                    // Logger.Log($"State: {states[i].DisplayName}, Prio: {i}");
                    states[i].Priority = i;
                    _fsm.AddState(states[i]);
                }

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

        private void OnEventsLuaStringWithArgs(string id, List<string> args)
        {
            /*
            Logger.LogError($"{id}");
            for (int i = 0; i < args.Count; i++)
            {
                Logger.LogError($"{i} -> {args[i]}");
            }
            */
            switch (id)
            {
                case "PLAYER_ENTERING_WORLD":
                    MovementManager.StopMove();
                    _entityCache.CacheGroupMembers(id);
                    _cache.CacheIsInInstance();
                    _cache.CacheInLoadingScreen(id);
                    break;
                case "PLAYER_LEAVING_WORLD":
                    MovementManager.StopMove();
                    _cache.CacheIsInInstance();
                    _cache.CacheInLoadingScreen(id);
                    break;
                case "CHAT_MSG_SYSTEM":
                    if (args[0] == "Everyone is Ready")
                        _partyChatManager.PartyReadyReceived();
                    break;
                case "WORLD_MAP_UPDATE":
                    _cache.CacheIsInInstance();
                    _entityCache.CacheGroupMembers(id);
                    //_cache.CacheInLoadingScreen(id);
                    break;
                case "LFG_PROPOSAL_SHOW":
                case "LFG_PROPOSAL_FAILED":
                case "LFG_PROPOSAL_SUCCEEDED":
                case "LFG_PROPOSAL_UPDATE":
                    _cache.CacheLFGProposalShown();
                    break;
                case "LFG_ROLE_CHECK_SHOW":
                case "LFG_ROLE_CHECK_HIDE":
                case "LFG_ROLE_CHECK_ROLE_CHOSEN":
                case "LFG_ROLE_CHECK_UPDATE":
                    _cache.CacheRoleCheckShow();
                    break;
                case "START_LOOT_ROLL":
                case "CANCEL_LOOT_ROLL":
                /*
            case "CONFIRM_LOOT_ROLL":
                _cache.CacheLootRollShow();
                break;
                */
                case "PARTY_MEMBERS_CHANGED":
                case "PARTY_MEMBER_DISABLE":
                case "PARTY_MEMBER_ENABLE":
                case "RAID_ROSTER_UPDATE":
                case "GROUP_ROSTER_CHANGED":
                case "PARTY_CONVERTED_TO_RAID":
                case "RAID_TARGET_UPDATE":
                    _entityCache.CacheGroupMembers(id);
                    break;
                case "INSTANCE_LOCK_STOP":
                case "INSTANCE_LOCK_START":
                    _cache.CacheInLoadingScreen(id);
                    break;
                case "PLAYER_LEVEL_UP":
                    Task.Factory.StartNew(() =>
                    {
                        SpellManager.UpdateSpellBook();
                        CustomClass.ResetCustomClass();
                    });
                    _profileManager?.UpdateAvailableDungeonList();
                    break;
                case "COMBAT_LOG_EVENT_UNFILTERED":
                    string logType = args[1];
                    if (logType == "SPELL_CAST_SUCCESS"
                        || logType == "SPELL_CAST_START")
                    {       
                        _avoidAOEManager.CheckSpells(args);   
                    }
                    break;
            }
        }

        internal void Dispose()
        {
            try
            {
                //FiniteStateMachineEvents.OnBeforeCheckIfNeedToRunState -= BeforeStateHandler;
                EventsLuaWithArgs.OnEventsLuaStringWithArgs -= OnEventsLuaStringWithArgs;
                OthersEvents.OnMount -= OnMount;
                _avoidAOEState?.Dispose();
                _avoidAOEManager?.Dispose();
                _checkPathAheadState?.Dispose();
                _deadState?.Dispose();
                _partyChatManager?.Dispose();
                _fsm?.StopEngine();
                _cache?.Dispose();
                _entityCache?.Dispose();
                _pathManager?.Dispose();
                _profileManager?.Dispose();
                _targetingManager?.Dispose();
                _luaStatusFrameManager?.Dispose();
                Fight.StopFight();
                CustomClass.DisposeCustomClass();
            }
            catch (Exception e)
            {
                Logger.LogError("Bot > Bot > Dispose: " + e);
            }
        }

        private void OnMount(string mountName, CancelEventArgs cancelable)
        {
            cancelable.Cancel = _cache.IsInInstance;
        }
    }
}
