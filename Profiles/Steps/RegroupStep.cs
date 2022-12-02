using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeDungeonCrawler.Profiles.Steps
{
    internal class RegroupStep : Step
    {
        private RegroupModel _regroupModel;
        private readonly IEntityCache _entityCache;
        public override string Name { get; }
        public override int Order { get; }
        public Vector3 RegroupSpot { get; private set; }
        private Timer _readyCheckTimer = new Timer();
        private string _lastLog;
        private int _foodMin;
        private int _drinkMin;
        private bool _drinkAllowed;

        public RegroupStep(RegroupModel regroupModel, IEntityCache entityCache)
        {
            _regroupModel = regroupModel;
            _entityCache = entityCache;
            Name = regroupModel.Name;
            Order = regroupModel.Order;
            RegroupSpot = regroupModel.RegroupSpot;
            _foodMin = wManager.wManagerSetting.CurrentSetting.FoodPercent;
            _drinkMin = wManager.wManagerSetting.CurrentSetting.DrinkPercent;
            _drinkAllowed = wManager.wManagerSetting.CurrentSetting.RestingMana;
        }

        public override void Run()
        {
            // Move to regroup spot location
            if (!MovementManager.InMovement
                && _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) > 5f)
            {
                LogUnique($"[{_regroupModel.Name}] Moving to regroup spot location");
                GoToTask.ToPosition(RegroupSpot, 0.5f);
                IsCompleted = false;
                return;
            }

            // Move to the exact regroup spot
            if (!MovementManager.InMovement
                && !MovementManager.InMoveTo
                && _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) > 1f)
            {
                LogUnique($"[{_regroupModel.Name}] Moving precisely to regroup spot");
                MovementManager.MoveTo(RegroupSpot);
                IsCompleted = false;
                return;
            }

            // Check if everyone is here
            if (_entityCache.ListGroupMember.Length != _entityCache.ListPartyMemberNames.Count
                || _entityCache.ListGroupMember.Any(member => member.PositionWithoutType.DistanceTo(RegroupSpot) > 8f)
                || _entityCache.Me.PositionWithoutType.DistanceTo(RegroupSpot) > 8f)
            {
                LogUnique($"Waiting for the team to regroup.");
                IsCompleted = false;
                return;
            }

            // Check for regen conditions
            if (_drinkAllowed && _entityCache.Me.Mana > 0 && _entityCache.Me.ManaPercent < _drinkMin)
            {
                LogUnique($"Skipping ready check vote until mana is restored");
                IsCompleted = false;
                return;
            }

            if (_entityCache.Me.HealthPercent < _foodMin)
            {
                LogUnique($"Skipping ready check vote until health is restores");
                IsCompleted = false;
                return;
            }

            bool imPartyLeader = Lua.LuaDoString<bool>("return IsPartyLeader() == 1;");
            int luaTimeRemaining = Lua.LuaDoString<int>("return GetReadyCheckTimeLeft();");
            bool imReady = Lua.LuaDoString<bool>($"return GetReadyCheckStatus('{_entityCache.Me.Name}') == 'ready';");

            // If you're the party leader, the LUA timer goes back to 0 when a ready check is finished
            // If you're not the party leader, the countdown continues even after a ready check ends

            if (luaTimeRemaining > _readyCheckTimer.TimeLeft() / 1000 + 1)
            {
                _readyCheckTimer.Reset(luaTimeRemaining * 1000);
            }

            // Party leader logic
            if (imPartyLeader)
            {
                // We need to reinitiate the check
                if (_readyCheckTimer.TimeLeft() <= 0)
                {
                    Logger.Log($"No ready check in progress. Initiating.");
                    Lua.LuaDoString("DoReadyCheck();");
                    Thread.Sleep(1000);
                    return;
                }

                // No check in progress
                if (_readyCheckTimer.TimeLeft() > 5000 
                    && _readyCheckTimer.TimeLeft() < 27000
                    && AllStatusAreNil())
                {
                    if (!_entityCache.IAmTank)
                    {
                        Thread.Sleep(1000);
                    }
                    Logger.Log("Everyone is ready");
                    IsCompleted = true;
                    return;
                }

                IsCompleted = false;
                return;
            }
            else
            // Party follower logic
            {
                if (_readyCheckTimer.TimeLeft() > 5000 
                    && _readyCheckTimer.TimeLeft() < 27000)
                {
                    if (!imReady)
                    {
                        Logger.Log($"Answering yes to ready check.");
                        Lua.LuaDoString("ReadyCheckFrameYesButton:Click();");
                        Lua.LuaDoString("ConfirmReadyCheck(true);");
                    }

                    if (AllStatusAreNil())
                    {
                        if (!_entityCache.IAmTank)
                        {
                            Thread.Sleep(1000);
                        }
                        Logger.Log("Everyone is ready");
                        IsCompleted = true;
                        return;
                    }

                    IsCompleted = false;
                    return;
                }
            }
        }

        private bool AllStatusAreNil()
        {
            List<string> partyMemberNames = _entityCache.ListPartyMemberNames.ToList();
            partyMemberNames.Add(_entityCache.Me.Name);
            bool everyoneReady = true;

            foreach (string partyMemberName in partyMemberNames)
            {
                bool memberReady = Lua.LuaDoString<bool>($"return GetReadyCheckStatus('{partyMemberName}') == nil;");
                if (!memberReady)
                {
                    everyoneReady = false;
                }
            }

            if (everyoneReady)
            {
                Logger.Log($"Everyone is ready");
            }

            return everyoneReady;
        }

        private void LogUnique(string text)
        {
            if (_lastLog != text)
            {
                _lastLog = text;
                Logger.Log(text);
            }
        }
    }
}
