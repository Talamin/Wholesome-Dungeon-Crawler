using robotManager.Helpful;
using System;
using System.Collections.Generic;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.ProductCache.Entity;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Managers
{
    internal class PartyChatManager : IPartyChatManager
    {
        private readonly IEntityCache _entityCache;
        private readonly IProfileManager _profileManager;
        private readonly char _separator = '$';
        private readonly string _channelName = "CHANNEL_NAME"; // Build channel name from entity cache ?

        public Vector3 TankPosition { get; private set; }

        public PartyChatManager(IEntityCache entityCache, IProfileManager profileManager)
        {
            _profileManager = profileManager;
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
                    HandleMessageReceived(args);
                    break;
                case "CHAT_MSG_PARTY_LEADER":
                    HandleMessageReceived(args);
                    break;
                case "PARTY_MEMBER_ENABLE":
                    Logger.LogError("PARTY_MEMBER_ENABLE");
                    UpdateTankValues();
                    break;
                case "PARTY_MEMBER_DISABLE":
                    Logger.LogError("PARTY_MEMBER_DISABLE");
                    BroadCastTankStatus();
                    break;
            }
        }

        private void UpdateTankValues()
        {
            if (_entityCache.TankUnit != null)
            {
                Logger.Log("Resetting tank values because he is in sight");
                TankPosition = null;
            }
        }

        private void BroadCastTankStatus()
        {
            if (_entityCache.IAmTank)
            {
                float myX = _entityCache.Me.PositionWithoutType.X;
                float myY = _entityCache.Me.PositionWithoutType.Y;
                float myZ = _entityCache.Me.PositionWithoutType.Z;
                Broadcast(ChatMessageType.TANKSTATUS, $"{myX}${myY}${myZ}${Usefuls.ContinentId}");
            }
        }

        private void HandleMessageReceived(List<string> args)
        {
            string message = args[0];
            string author = args[1];
            Logger.Log($"Message sent by {author} : {message}");
            string[] messageParts = message.Split(_separator);
            if (Enum.TryParse(messageParts[1], out ChatMessageType messageType))
            {
                switch (messageType)
                {
                    case ChatMessageType.TANKSTATUS:
                        if (!_entityCache.IAmTank && _entityCache.TankUnit == null)
                        {
                            float posX = float.Parse(messageParts[2]);
                            float posY = float.Parse(messageParts[3]);
                            float posZ = float.Parse(messageParts[4]);
                            int tankMapId = int.Parse(messageParts[5]);
                            if (tankMapId == Usefuls.ContinentId)
                            {
                                TankPosition = new Vector3(posX, posY, posZ);
                                Logger.LogError($"Tank position is {TankPosition}, mapId {tankMapId}");
                            }
                        }
                        break;
                }
            }
            else
            {
                Logger.LogError($"Message type unknown : {messageParts[1]}");
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
    TANKSTATUS
}