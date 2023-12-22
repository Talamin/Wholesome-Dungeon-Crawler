using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using WholesomeDungeonCrawler.CrawlerSettings;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles;
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

        public Dead(IEntityCache iEntityCache,
            IProfileManager profilemanager)
        {
            _entityCache = iEntityCache;
            _profileManager = profilemanager;
        }

        public void Initalize()
        {
            if (WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar)
            {
                if (!Radar3D.IsLaunched) Radar3D.Pulse();
                Radar3D.OnDrawEvent += DrawEventDeadState;
            }
        }

        public void Dispose()
        {
            if (WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar)
            {
                Radar3D.OnDrawEvent -= DrawEventDeadState;
                Radar3D.Stop();
            }
        }

        private List<WoWClass> _rezzClasses = new List<WoWClass> { WoWClass.Druid, WoWClass.Paladin, WoWClass.Priest, WoWClass.Shaman };
        public override bool NeedToRun
        {
            get
            {
                return _entityCache.Me.IsDead;
            }
        }

        public override void Run()
        {
            Thread.Sleep(1000);
            for (int i = 1; i <= 3; i++)
            {
                if (Lua.LuaDoString<bool>($"return StaticPopup{i}Text:IsVisible();"))
                {
                    for (int j = 1; j <= 2; j++)
                    {
                        string buttonText = Lua.LuaDoString<string>($"return StaticPopup{i}Button{j}:GetText();");
                        if (buttonText != null
                            && (buttonText.Contains("Use Soulstone") || buttonText.Contains("Reincarnation"))
                            && Lua.LuaDoString<bool>($"return StaticPopup{i}Button{j}:IsVisible();"))
                        {
                            Logger.LogOnce("Using soulstone/reincarnation");
                            Lua.LuaDoString($"StaticPopup{i}Button{j}:Click();");
                            _forceReleaseTimer = null;
                            return;
                        }
                    }

                    string frameText = Lua.LuaDoString<string>($"return StaticPopup{i}Text:GetText();");
                    if (frameText != null && frameText.Contains("wants to resurrect you"))
                    {
                        Logger.LogOnce("Accepting party resurrection");
                        Lua.LuaDoString($"StaticPopup{i}Button1:Click();");
                        _forceReleaseTimer = null;
                        return;
                    }
                }
            }

            if (_entityCache.Me.Auras.ContainsKey(8326)) // Ghost
            {
                _forceReleaseTimer = null;
                if (MovementManager.InMovement)
                {
                    return;
                }

                if (_profileManager.ProfileIsRunning
                    && _profileManager.CurrentDungeonProfile.DeathRunPaths.Count > 0
                    && _profileManager.CurrentDungeonProfile.DeathRunPaths.Any(dr => dr.Path.Count > 0))
                {
                    DeathRun closestDeathRun = null;
                    float closestDistance = float.MaxValue;
                    Vector3 myPos = _entityCache.Me.PositionWT;
                    foreach (DeathRun deathRun in _profileManager.CurrentDungeonProfile.DeathRunPaths)
                    {
                        if (deathRun.Path.Count <= 0)
                        {
                            Logger.LogError($"WARNING: The deathrun path {deathRun.Name} in your profile is empty!");
                            continue;
                        }

                        Vector3 closestNode = deathRun.Path.OrderBy(node => node.DistanceTo(myPos)).FirstOrDefault();
                        if (closestNode != null
                            && closestNode.DistanceTo(myPos) < closestDistance)
                        {
                            closestDeathRun = deathRun;
                            closestDistance = closestNode.DistanceTo(myPos);
                        }
                    }

                    if (closestDeathRun != null)
                    {
                        if (closestDistance > 30)
                            Logger.LogError($"WARNING: The closest deathrun {closestDeathRun.Name} is {closestDistance} yards away. Is that correct?");
                        Logger.Log($"Running profile's death run called {closestDeathRun.Name}");
                        List<Vector3> adjustedDeathPath = WTPathFinder.PathFromClosestPoint(closestDeathRun.Path);
                        if (_profileManager.CurrentDungeonProfile?.DungeonModel?.EntranceLoc != null)
                        {
                            Logger.Log($"Adding EntranceLoc {_profileManager.CurrentDungeonProfile.DungeonModel.EntranceLoc} to end of path");
                            adjustedDeathPath.Add(_profileManager.CurrentDungeonProfile.DungeonModel.EntranceLoc);
                        }
                        MovementManager.Go(adjustedDeathPath);
                    }
                    else
                    {
                        Logger.LogOnce($"ERROR: Couldn't find closest deathrun", true);
                    }
                }
                else
                {
                    DungeonModel dungeon = Lists.AllDungeons.Where(x => x.ContinentId == Usefuls.ContinentId)
                            .OrderBy(dungeon => dungeon.EntranceLoc.DistanceTo(_entityCache.Me.PositionCorpse))
                            .FirstOrDefault();
                    if (dungeon != null && dungeon.EntranceLoc != null)
                    {
                        Logger.Log("No profile death run found, using pathfinder towards EntranceLoc.");
                        GoToTask.ToPosition(dungeon.EntranceLoc, skipIfCannotMakePath: false);
                    }
                    else
                    {
                        Logger.LogOnce("No profile death run found and no EntranceLoc defined.", true);
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
                        && _entityCache.Me.PositionWT.DistanceTo(resPlayer.PositionWT) < 50))
                {
                    if (_forceReleaseTimer == null)
                    {
                        Logger.Log($"Starting force release timer ({_forceReleaseTimeInSeconds} seconds)");
                        _forceReleaseTimer = new robotManager.Helpful.Timer(_forceReleaseTimeInSeconds * 1000);
                    }

                    if ((int)(_forceReleaseTimer.TimeLeft()) % 10000 == 0)
                    {
                        Logger.LogOnce($"Forcing release in {(int)(_forceReleaseTimer.TimeLeft() / 1000)}");
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

        private void DrawEventDeadState()
        {
            if (!WholesomeDungeonCrawlerSettings.CurrentSetting.EnableRadar) return;

            if (_profileManager.ProfileIsRunning
                && _entityCache.Me.IsDead)
            {
                foreach (DeathRun deathRunPath in _profileManager.CurrentDungeonProfile.DeathRunPaths)
                {
                    List<Vector3> deathRun = deathRunPath.Path;
                    for (int i = 0; i < deathRun.Count - 1; i++)
                    {
                        if (deathRun[i] != null && deathRun[i + 1] != null)
                        {
                            Radar3D.DrawLine(deathRun[i], deathRun[i + 1], Color.Red, 150);
                        }
                    }

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
