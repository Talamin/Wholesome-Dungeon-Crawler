using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.States
{
    class Dead : State, IState
    {
        public override string DisplayName => "Dead";

        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private robotManager.Helpful.Timer _forceReleaseTimer = null;
        private int _forceReleaseTimeInSeconds = 180;

        public Dead(IEntityCache iEntityCache, IProfileManager profilemanager)
        {
            _entityCache = iEntityCache;
            _profileManager = profilemanager;
        }

        private List<WoWClass> _rezzClasses = new List<WoWClass> { WoWClass.Druid, WoWClass.Paladin, WoWClass.Priest, WoWClass.Shaman };
        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnected
                    || !_entityCache.Me.IsValid)
                {
                    return false;
                }

                return _entityCache.Me.IsDead;
            }
        }

        public override void Run()
        {
            for (int i = 1; i <= 3; i++)
            {
                if (Lua.LuaDoString<bool>($"return StaticPopup{i}Text:IsVisible();"))
                {
                    string resText = Lua.LuaDoString<string>($"return StaticPopup{i}Text:GetText();");
                    if (resText != null && resText.Contains("wants to resurrect you"))
                    {
                        Logger.LogOnce("Accepting party resurrection");
                        Lua.LuaDoString($"StaticPopup{i}Button1:Click();");
                        _forceReleaseTimer = null;
                        return;
                    }
                }
            }

            if (_entityCache.Me.Auras.ContainsKey(20762)) // Soulstone
            {
                Logger.LogOnce("Accepting soulstone resurrection");
                for (int i = 1; i <= 3; i++)
                {
                    Lua.LuaDoString($"StaticPopup{i}Button1:Click();");
                }
                return;
            }

            if (_entityCache.Me.Auras.ContainsKey(8326)) // Ghost
            {
                _forceReleaseTimer = null;
                if (MovementManager.InMovement)
                {
                    return;
                }

                if (_profileManager.CurrentDungeonProfile?.DeathRunPath != null
                    && _profileManager.CurrentDungeonProfile.DeathRunPath.Count > 0)
                {
                    Logger.Log("Running profile's death run");
                    List<Vector3> adjustedDeathPath = WTPathFinder.PathFromClosestPoint(_profileManager.CurrentDungeonProfile.DeathRunPath);
                    if (_profileManager.CurrentDungeonProfile.DungeonModel != null)
                    {
                        adjustedDeathPath.Add(_profileManager.CurrentDungeonProfile.DungeonModel.EntranceLoc);
                    }
                    MovementManager.Go(adjustedDeathPath, false);
                }
                else
                {
                    DungeonModel dungeon = Lists.AllDungeons.Where(x => x.ContinentId == Usefuls.ContinentId)
                            .OrderBy(dungeon => dungeon.EntranceLoc.DistanceTo(_entityCache.Me.PositionCorpse))
                            .FirstOrDefault();
                    Logger.Log("No profile death run found, using pathfinder towards EntranceLoc.");
                    if (dungeon != null)
                    {
                        GoToTask.ToPosition(dungeon.EntranceLoc, skipIfCannotMakePath: false);
                    }
                }
            }
            else
            {
                // Failsafe for loading screen
                Thread.Sleep(3000);
                if (!_entityCache.Me.IsDead)
                {
                    _forceReleaseTimer = null;
                    return;
                }

                MovementManager.StopMove();
                if (_entityCache.ListGroupMember
                    .Any(resPlayer => _rezzClasses.Contains(resPlayer.WoWClass)
                        && !resPlayer.IsDead
                        && _entityCache.Me.PositionWithoutType.DistanceTo(resPlayer.PositionWithoutType) < 50))
                {
                    if (_forceReleaseTimer == null)
                    {
                        Logger.Log($"Starting force release timer ({_forceReleaseTimeInSeconds} seconds)");
                        _forceReleaseTimer = new robotManager.Helpful.Timer(_forceReleaseTimeInSeconds * 1000);
                    }

                    if (_forceReleaseTimer.TimeLeft() % 10000 == 0)
                    {
                        Logger.LogOnce($"Forcing release in {_forceReleaseTimer.TimeLeft() / 1000}");
                    }

                    Logger.LogOnce("A group member can resurrect me. Waiting.");

                    if (_forceReleaseTimer.IsReady)
                    {
                        ReleaseCorpse("Timer is up. Forcing release.");
                        return;
                    }

                    Thread.Sleep(3000);
                }
                else if (_entityCache.EnemiesAttackingGroup.Length > 0)
                {
                    _forceReleaseTimer = null;
                    Logger.LogOnce("Group is still fighting. Waiting.");
                    Thread.Sleep(3000);
                }
                else
                {
                    ReleaseCorpse("No one can resurrect me");
                }
            }
        }

        private void ReleaseCorpse(string reason)
        {
            _forceReleaseTimer = null;
            Thread.Sleep(1000);
            Logger.Log($"Releasing corpse ({reason}).");
            Lua.LuaDoString("RepopMe();");
            Thread.Sleep(5000);
        }
    }
}
