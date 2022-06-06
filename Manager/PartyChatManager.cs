using System.Collections.Generic;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Manager
{
    internal class PartyChatManager : IPartyChatManager
    {
        public void Initialize()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += OnEventLuaWithArgs;
        }

        public void Dispose()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= OnEventLuaWithArgs;
        }
        private void OnEventLuaWithArgs(string id, List<string> args)
        {
            switch (id)
            {
                case "CHAT_MSG_PARTY":
                    HandleAddonChatMessage(id, args);
                    break;
            }
        }

        private void HandleAddonChatMessage(string id, List<string> args)
        {
            string prefix = args[0];
            string message = args[1];
            string distribution = args[2];
            string sender = args[3];
            Logger.LogError(id);
            Logger.LogError(prefix);
            Logger.LogError(message);
            Logger.LogError(distribution);
            Logger.LogError(sender);
        }

        public void Broadcast(string message)
        {
            Lua.LuaDoString($@"
                    SendChatMessage({message}, ""PARTY"");
                ");
        }
    }
}
