﻿using robotManager.FiniteStateMachine;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.States
{
    class WaitCombatFlagsDisappear : State, IState
    {
        public override string DisplayName => "Waiting for party combat flags to wear off";
        private Timer _timerUntilBan = null;
        private Timer _bannedTimer = new Timer();
        private readonly int _banTime = 30;
        private readonly int _waitTime = 15;

        private readonly ICache _cache;
        private readonly IEntityCache _entityCache;

        public WaitCombatFlagsDisappear(
            ICache iCache,
            IEntityCache entityCache)
        {
            _cache = iCache;
            _entityCache = entityCache;
        }

        public override bool NeedToRun
        {
            get
            {
                if (!_cache.IsInInstance
                    || !_bannedTimer.IsReady)
                {
                    return false;
                }

                if (_entityCache.EnemiesAttackingGroup.Length > 0)
                {
                    _timerUntilBan = null;
                    return false;
                }

                if (_entityCache.ListGroupMember.Any(m => m.InCombatFlagOnly))
                {
                    if (_timerUntilBan == null)
                    {
                        _timerUntilBan = new Timer(_waitTime * 1000);
                    }

                    if (_timerUntilBan.IsReady)
                    {
                        _timerUntilBan = null;
                        _bannedTimer = new Timer(_banTime * 1000);
                        Logger.LogError($"Banned Wait combat flags state for {_banTime}s");
                        return false;
                    }

                    return true;
                }

                _timerUntilBan = null;
                return false;
            }
        }

        public override void Run()
        {
            Logger.LogOnce($"Waiting for party combat flags to wear off (max {_waitTime}s)");
            MovementManager.StopMove();
        }
    }
}
