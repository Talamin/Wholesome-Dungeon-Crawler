using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.ProductCache
{
    internal class Cache : ICache
    {
        private object cacheLock = new object();

        public bool IsInInstance { get; private set; }
        public string CurrentState { get; set; }
        public bool LFGProposalShown { get; private set; }
        public bool LFGRoleCheckShown { get; private set; }
        public bool LootRollShow { get; private set; }
        public bool IAmAlliance { get; private set; }
        public bool InLoadingScreen { get; private set; }
        public bool IsRunningForcedTownRun { get; set; }

        public Cache()
        {
        }

        public void Initialize()
        {
            IAmAlliance = ObjectManager.Me.IsAlliance;
            CacheIsInInstance();
            CacheRoleCheckShow();
            CacheLFGProposalShown();
            CacheLootRollShow();

            robotManager.Events.FiniteStateMachineEvents.OnRunState += FiniteStateMachineEvents_OnRunState;
        }

        public void Dispose()
        {
            robotManager.Events.FiniteStateMachineEvents.OnRunState -= FiniteStateMachineEvents_OnRunState;
        }

        private void FiniteStateMachineEvents_OnRunState(robotManager.FiniteStateMachine.Engine engine, robotManager.FiniteStateMachine.State state, System.ComponentModel.CancelEventArgs cancelable)
        {
            if (!state.DisplayName.Contains("Security"))
            {
                CurrentState = state.DisplayName;
            }
        }

        public void CacheIsInInstance()
        {
            lock (cacheLock)
            {
                IsInInstance = WTLocation.IsInInstance();
            }
        }

        public void ResetLoadingScreenLock()
        {
            InLoadingScreen = false;
        }

        public void CacheInLoadingScreen(string eventName)
        {
            MovementManager.StopMove();
            InLoadingScreen = true;
        }
        /*
        private void GetLFGModes()
        {
            GetLFGMode = Lua.LuaDoString<string>("local mode, submode= GetLFGMode(); if mode == nil then return 'nil' else return mode end;");
            MiniMapLFGFrameIcon = Lua.LuaDoString<bool>("return MiniMapLFGFrameIcon:IsVisible()");
        }
        */
        public void CacheLFGProposalShown()
        {
            LFGProposalShown = Lua.LuaDoString<bool>("return LFDDungeonReadyDialogEnterDungeonButton:IsVisible()");
        }

        public void CacheRoleCheckShow()
        {
            LFGRoleCheckShown = Lua.LuaDoString<bool>("return LFDRoleCheckPopupAcceptButton:IsVisible()");
        }

        public void CacheLootRollShow()
        {
            // This doesn't work
            //LootRollShow = Lua.LuaDoString<bool>("for i = 1, 4 do local b = ['GroupLootFrame'..i] if b and b:IsVisible() then return true end end return false");
        }
    }
}
