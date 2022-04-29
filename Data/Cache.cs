using System.Collections.Generic;
using System.Linq;
using WholesomeDungeonCrawler.Helpers;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.Data
{
    internal class Cache : ICache
    {

        private object cacheLock = new object();


        public bool IsInInstance { get; private set; }
        public bool IsPartyInviteRequest { get; private set; }
        public bool HaveSatchel { get; private set; }
        public List<string> ListPartyMember { get; private set; } = new List<string>();
        public string GetLFGMode { get; private set; }
        public bool MiniMapLFGFrameIcon { get; private set; }
        public string GetPlayerSpec { get; private set; }
        public bool LFGProposalShown { get; private set; }
        public bool LFGRoleCheckShown { get; private set; }

        //general

        public Cache()
        {
        }

        public void Initialize()
        {
            //First Initalization of Variables
            CacheIsInInstance();
            CacheLFGCompletionReward();
            CachePartyInviteRequest();
            CachePlayerSpec();
            CacheRoleCheckShow();
            if(ObjectManager.Me.IsInGroup)
            {
                CachePartyMemberChanged();
            }
            //Beginning of Event Subscriptions
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            EventsLua.AttachEventLua("WORLD_MAP_UPDATE", m => CacheIsInInstance());
            EventsLua.AttachEventLua("PARTY_INVITE_REQUEST", m => CachePartyInviteRequest());
            EventsLua.AttachEventLua("LFG_COMPLETION_REWARD", m => CacheLFGCompletionReward());
            EventsLua.AttachEventLua("PARTY_MEMBERS_CHANGED", m => CachePartyMemberChanged());
            EventsLua.AttachEventLua("PLAYER_LEVEL_UP", m => CachePlayerSpec());
            EventsLua.AttachEventLua("LFG_QUEUE_STATUS_UPDATE", m => GetLFGModes());
            EventsLua.AttachEventLua("LFG_PROPOSAL_SHOW", m => CacheLFGProposalShow());
            EventsLua.AttachEventLua("LFG_PROPOSAL_FAILED", m => CacheLFGProposalFailed());
            EventsLua.AttachEventLua("LFG_PROPOSAL_SUCCEEDED", m => CacheLFGProposalSucceeded());
            EventsLua.AttachEventLua("LFG_ROLE_CHECK_SHOW", m => CacheRoleCheckShow());
            EventsLua.AttachEventLua("LFG_ROLE_CHECK_HIDE", m => CacheRoleCheckShow());
            EventsLua.AttachEventLua("LFG_ROLE_CHECK_ROLE_CHOSEN", m => CacheRoleCheckShow());
            EventsLua.AttachEventLua("LFG_ROLE_CHECK_UPDATE", m => CacheRoleCheckShow());
            //EventsLua.AttachEventLua(LuaEventsId.PLAYER_LEVEL_UP  <-- depreciated, only for lookup
        }

        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
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

        private void CachePartyInviteRequest()
        {
            string StaticPopupText = Lua.LuaDoString<string>("return StaticPopup1Text:GetText()");
            bool isvisible = Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();");
            if(isvisible && StaticPopupText.Contains("invites you to a group"))
            {
                IsPartyInviteRequest = true;
                return;
            }
            IsPartyInviteRequest = false;         
        }

        private void CacheLFGCompletionReward()
        {
            lock (cacheLock)
            {
                HaveSatchel = Bag.GetBagItem().Count(item => item.Name.Contains("Satchel of")) > 0;
            }
        }

        private void ClearCachedLists()
        {
            ListPartyMember.Clear();
        }

        private void CachePartyMemberChanged()
        {
            CachePartyInviteRequest();
            ClearCachedLists();
            GetLFGModes();
            lock (cacheLock)
            {
                var plist = Lua.LuaDoString<string>(@"
                    plist='';
                    for i=1,4 do
                        if (UnitName('party'..i)) then
                            plist = plist .. UnitName('party'..i) ..','
                        end
                    end", "plist");

                ListPartyMember = plist.Remove(plist.Length - 1, 1).Split(',').ToList();
            }
        }

        private void GetLFGModes()
        {
            GetLFGMode = Lua.LuaDoString<string>("local mode, submode= GetLFGMode(); if mode == nil then return 'nil' else return mode end;");
            MiniMapLFGFrameIcon = Lua.LuaDoString<bool>("return MiniMapLFGFrameIcon: IsVisible()");
        }

        private void CachePlayerSpec()
        {
            lock(cacheLock)
            {
                var Talents = new Dictionary<string, int>();
                for (int i = 1; i <= 3; i++)
                {
                    Talents.Add(
                        Lua.LuaDoString<string>($"local name, iconTexture, pointsSpent = GetTalentTabInfo({i}); return name"),
                        Lua.LuaDoString<int>($"local name, iconTexture, pointsSpent = GetTalentTabInfo({i}); return pointsSpent")
                    );
                }
                var highestTalents = Talents.Max(x => x.Value);
                GetPlayerSpec = Talents.FirstOrDefault(t => t.Value == highestTalents).Key.Replace(" ", "");
            }
        }

        private void CacheLFGProposalShow()
        {
            LFGProposalShown = Lua.LuaDoString<bool>("return LFDDungeonReadyDialogEnterDungeonButton:IsVisible()");
        }
        private void CacheLFGProposalFailed()
        {
            LFGProposalShown = Lua.LuaDoString<bool>("return LFDDungeonReadyDialogEnterDungeonButton:IsVisible()");
        }
        private void CacheLFGProposalSucceeded()
        {
            LFGProposalShown = Lua.LuaDoString<bool>("return LFDDungeonReadyDialogEnterDungeonButton:IsVisible()");
        }
        private void CacheRoleCheckShow()
        {
            LFGRoleCheckShown = Lua.LuaDoString<bool>("return LFDRoleCheckPopupAcceptButton:IsVisible()");
        }
    }
}
