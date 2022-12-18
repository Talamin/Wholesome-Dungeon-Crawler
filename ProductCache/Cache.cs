using System.Collections.Generic;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.ProductCache
{
    internal class Cache : ICache
    {

        private object cacheLock = new object();

        public bool IsInInstance { get; private set; }
        public bool PartyInviteShown { get; private set; }
        public bool HaveSatchel { get; private set; }
        public string CurrentState { get; set; }
        public string GetLFGMode { get; private set; }
        public bool MiniMapLFGFrameIcon { get; private set; }
        public string GetPlayerSpec { get; private set; }
        public bool LFGProposalShown { get; private set; }
        public bool LFGRoleCheckShown { get; private set; }
        public bool LootRollShow { get; private set; }
        public bool IAmAlliance { get; private set; }
        public bool HaveResurrection { get; private set; }

        public Cache()
        {
        }

        public void Initialize()
        {
            IAmAlliance = ObjectManager.Me.IsAlliance;
            CacheIsInInstance();
            CacheRoleCheckShow();
            if (ObjectManager.Me.IsInGroup)
            {
                CachePartyMemberChanged();
            }
            CacheHaveResurretion();
            GetLFGModes();
            CacheLFGProposalShown();
            CacheLootRollShow();

            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            robotManager.Events.FiniteStateMachineEvents.OnRunState += FiniteStateMachineEvents_OnRunState;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsLuaWithArgs_OnEventsLuaWithArgs;
        }

        private void FiniteStateMachineEvents_OnRunState(robotManager.FiniteStateMachine.Engine engine, robotManager.FiniteStateMachine.State state, System.ComponentModel.CancelEventArgs cancelable)
        {
            if (!state.DisplayName.Contains("Security"))
                CurrentState = state.DisplayName;
        }

        private void EventsLuaWithArgs_OnEventsLuaWithArgs(string id, List<string> args)
        {
            switch (id)
            {
                case "WORLD_MAP_UPDATE":
                    CacheIsInInstance();
                    break;
                case "PARTY_MEMBERS_CHANGED":
                    CachePartyMemberChanged();
                    break;
                case "LFG_QUEUE_STATUS_UPDATE":
                    GetLFGModes();
                    break;
                case "LFG_PROPOSAL_SHOW":
                    CacheLFGProposalShown();
                    break;
                case "LFG_PROPOSAL_FAILED":
                    CacheLFGProposalShown();
                    break;
                case "LFG_PROPOSAL_SUCCEEDED":
                    CacheLFGProposalShown();
                    break;
                case "LFG_ROLE_CHECK_SHOW":
                    CacheRoleCheckShow();
                    break;
                case "LFG_ROLE_CHECK_HIDE":
                    CacheRoleCheckShow();
                    break;
                case "LFG_ROLE_CHECK_ROLE_CHOSEN":
                    CacheRoleCheckShow();
                    break;
                case "LFG_ROLE_CHECK_UPDATE":
                    CacheRoleCheckShow();
                    break;
                case "START_LOOT_ROLL":
                    CacheLootRollShow();
                    break;
                case "CANCEL_LOOT_ROLL":
                    CacheLootRollShow();
                    break;
                case "RESURRECT_REQUEST":
                    CacheHaveResurretion();
                    break;
            }
        }

        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
            robotManager.Events.FiniteStateMachineEvents.OnRunState -= FiniteStateMachineEvents_OnRunState;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsLuaWithArgs_OnEventsLuaWithArgs;
        }

        private void OnObjectManagerPulse()
        {
            lock (cacheLock)
            {

            }
        }

        private void CacheIsInInstance()
        {
            lock (cacheLock)
            {
                IsInInstance = WTLocation.IsInInstance();
            }
        }

        private void CachePartyMemberChanged()
        {
            GetLFGModes();
        }

        private void GetLFGModes()
        {
            GetLFGMode = Lua.LuaDoString<string>("local mode, submode= GetLFGMode(); if mode == nil then return 'nil' else return mode end;");
            MiniMapLFGFrameIcon = Lua.LuaDoString<bool>("return MiniMapLFGFrameIcon:IsVisible()");
        }

        private void CacheLFGProposalShown()
        {
            LFGProposalShown = Lua.LuaDoString<bool>("return LFDDungeonReadyDialogEnterDungeonButton:IsVisible()");
        }

        private void CacheRoleCheckShow()
        {
            LFGRoleCheckShown = Lua.LuaDoString<bool>("return LFDRoleCheckPopupAcceptButton:IsVisible()");
        }

        private void CacheLootRollShow()
        {
            LootRollShow = Lua.LuaDoString<bool>("for i = 1, 4 do local b = ['GroupLootFrame'..i] if b and b:IsVisible() then return true end end return false");
        }

        private void CacheHaveResurretion()
        {
            HaveResurrection = Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();");
        }
    }
}
