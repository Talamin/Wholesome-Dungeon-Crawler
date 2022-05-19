using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WholesomeDungeonCrawler.CrawlerSettings;
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
        public bool LootRollShow { get; private set; }
        public List<ulong> ListPartyMemberGuid { get; private set; } = new List<ulong>();
        //public List<IWoWUnit> PartyMember { get; private set; } = new List<IWoWUnit>();
        public ulong TankGuid { get; private set; }

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
            //CachePlayerSpec();
            CacheRoleCheckShow();
            if(ObjectManager.Me.IsInGroup)
            {
                CachePartyMemberChanged();
            }
            //Beginning of Event Subscriptions
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
        
            //EventsLua.AttachEventLua("PLAYER_LEVEL_UP", m => CachePlayerSpec());

            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsLuaWithArgs_OnEventsLuaWithArgs;
            //EventsLua.AttachEventLua(LuaEventsId.PLAYER_LEVEL_UP  <-- depreciated, only for lookup
        }

        private void EventsLuaWithArgs_OnEventsLuaWithArgs(string id, List<string> args)
        {
            switch(id)
            {
                case "WORLD_MAP_UPDATE":
                    CacheIsInInstance();
                    CacheListPartyMemberGuid();
                    break;
                case "PARTY_INVITE_REQUEST":
                    CachePartyInviteRequest();
                    break;
                case "LFG_COMPLETION_REWARD":
                    CacheLFGCompletionReward();
                    break;
                case "PARTY_MEMBERS_CHANGED":
                    CachePartyMemberChanged();
                    CacheListPartyMemberGuid();
                    break;
                case "LFG_QUEUE_STATUS_UPDATE":
                    GetLFGModes();
                    break;
                case "LFG_PROPOSAL_SHOW":
                    CacheLFGProposalShow();
                    break;
                case "LFG_PROPOSAL_FAILED":
                    CacheLFGProposalFailed();
                    break;
                case "LFG_PROPOSAL_SUCCEEDED":
                    CacheLFGProposalSucceeded();
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

            }
        }

        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
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
            Debugger.Break();
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

        //private void CachePlayerSpec()
        //{
        //    lock(cacheLock)
        //    {
        //        var Talents = new Dictionary<string, int>();
        //        for (int i = 1; i <= 3; i++)
        //        {
        //            Talents.Add(
        //                Lua.LuaDoString<string>($"local name, iconTexture, pointsSpent = GetTalentTabInfo({i}); return name"),
        //                Lua.LuaDoString<int>($"local name, iconTexture, pointsSpent = GetTalentTabInfo({i}); return pointsSpent")
        //            );
        //        }
        //        var highestTalents = Talents.Max(x => x.Value);
        //        GetPlayerSpec = Talents.FirstOrDefault(t => t.Value == highestTalents).Key.Replace(" ", "");
        //    }
        //}

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

        private void CacheLootRollShow()
        {
            LootRollShow = Lua.LuaDoString<bool>("for i = 1, 4 do local b = ['GroupLootFrame'..i] if b and b:IsVisible() then return true end end return false");
        }

        private void CacheListPartyMemberGuid()
        {
            List<ulong> partyMembers = new List<ulong>();
            foreach(WoWPlayer p in Party.GetParty())
            {
                partyMembers.Add(p.Guid);
                Logger.Log($"Updated Party, added Groupmember: {p.Name} ");
                if(p.Name == WholesomeDungeonCrawlerSettings.CurrentSetting.TankName)
                {
                    TankGuid = p.Guid;
                    Logger.Log($"Updated Party, added Tank: {p.Name} ");
                }
            }
            partyMembers.Add(ObjectManager.Me.Guid);
            ListPartyMemberGuid = partyMembers;
        }

    }
}
