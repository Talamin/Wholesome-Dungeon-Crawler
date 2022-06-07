using robotManager.Helpful;
using System;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Data;
using WholesomeDungeonCrawler.Helpers;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Manager
{
    internal class PartyChatManager : IPartyChatManager
    {
        private readonly IEntityCache _entityCache;
        private readonly char _separator = '$';
        private readonly string _channelName = "CHANNEL_NAME"; // Build channel name from entity cache ?

        public Vector3 TankPosition { get; private set; }

        public PartyChatManager(IEntityCache entityCache)
        {
            _entityCache = entityCache;
        }

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
                    HandleChatMessageParty(args);
                    break;
                case "CHAT_MSG_PARTY_LEADER":
                    HandleChatMessageParty(args);
                    break;
                case "PARTY_MEMBER_ENABLE":
                    float myX = _entityCache.Me.PositionWithoutType.X;
                    float myY = _entityCache.Me.PositionWithoutType.Y;
                    float myZ = _entityCache.Me.PositionWithoutType.Z;
                    Broadcast(ChatMessageType.TANKPOSITION, $"{myX}${myY}${myZ}");
                    break;
            }
        }

        private void HandleChatMessageParty(List<string> args)
        {
            string message = args[0];
            string author = args[1];
            Logger.Log($"Message sent by {author} : {message}");
            string[] messageParts = message.Split(_separator);
            if (Enum.TryParse(messageParts[1], out ChatMessageType messageType))
            {
                switch (messageType)
                {
                    case ChatMessageType.TANKPOSITION:
                        float posX = float.Parse(messageParts[2]);
                        float posY = float.Parse(messageParts[3]);
                        float posZ = float.Parse(messageParts[4]);
                        TankPosition = new Vector3(posX, posY, posZ);
                        Logger.LogError($"Tank position is {TankPosition}");
                        break;
                }
            }
            else
            {
                Logger.LogError($"Message type unknown : {messageParts[0]}");
            }
        }

        public void Broadcast(ChatMessageType messageType, string message)
        {
            Lua.LuaDoString($@"
                    SendChatMessage(""{_channelName}{_separator}{messageType}{_separator}{message}"", ""PARTY"");
                ");
        }
    }
}

public enum ChatMessageType
{
    TANKPOSITION,
    CURRENTSTEP
}