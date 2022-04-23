using System.Collections.Generic;
using System.Linq;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Helpers;

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

        public Cache()
        {
        }

        public void Initialize()
        {
            //First Initalization of Variables
            CacheIsInInstance();
            CachePartyInviteRequest();
            CacheLFGCompletionReward();
            CachePartyInviteRequest();
            //CachePartyMemberChanged();
            //Beginning of Event Subscriptions
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            EventsLua.AttachEventLua("WORLD_MAP_UPDATE", m => CacheIsInInstance());
            EventsLua.AttachEventLua("PARTY_INVITE_REQUEST", m => CachePartyInviteRequest());
            EventsLua.AttachEventLua("LFG_COMPLETION_REWARD", m => CacheLFGCompletionReward());
            EventsLua.AttachEventLua("PARTY_MEMBERS_CHANGED", m => CachePartyMemberChanged());
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
            string StaticPopupText = Lua.LuaDoString<string>("StaticPopup1Text:GetText()");
            IsPartyInviteRequest = StaticPopupText.Contains("invites you to a group");
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
            GetLFGMode = Lua.LuaDoString<string>("local mode, submode = GetLFGMode(); return mode;");
        }
    }
}
