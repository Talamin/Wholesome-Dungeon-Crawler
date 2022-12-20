using robotManager.Helpful;
using WholesomeDungeonCrawler.ProductCache.Entity;
using WholesomeDungeonCrawler.Profiles.Steps;
using wManager.Wow.Helpers;

namespace WholesomeDungeonCrawler.Managers
{
    internal class PartyChatManager : IPartyChatManager
    {
        private readonly IEntityCache _entityCache;
        private readonly char _separator = '$';
        private readonly string _channelName = "CHANNEL_NAME"; // Build channel name from entity cache ?
        private RegroupStep _regroupStep;

        public PlayerStatus TankStatus { get; private set; }

        public PartyChatManager(IEntityCache entityCache)
        {
            _entityCache = entityCache;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void SetRegroupStep(RegroupStep regroupStep)
        {
            _regroupStep = regroupStep;
        }

        public void PartyReadyReceived()
        {
            _regroupStep?.PartyReadyReceived();
        }
        /*
        private void ResetTankStatus()
        {
            Logger.Log("Resetting tank status because he is in sight");
            TankStatus = null;
        }
        
        private void BroadCastTankStatus()
        {
            if (_entityCache.IAmTank)
            {
                float myX = _entityCache.Me.PositionWithoutType.X;
                float myY = _entityCache.Me.PositionWithoutType.Y;
                float myZ = _entityCache.Me.PositionWithoutType.Z;
                int stepIndex = -1;
                string stepName = "";
                if (_profileManager.CurrentDungeonProfile?.CurrentStep != null)
                {
                    stepIndex = _profileManager.CurrentDungeonProfile.CurrentStep.Order;
                    stepName = _profileManager.CurrentDungeonProfile.CurrentStep.Name;
                }
                Broadcast(ChatMessageType.TANK_STATUS, $"{myX}${myY}${myZ}${Usefuls.ContinentId}${stepIndex}${stepName}");
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
                    case ChatMessageType.TANK_STATUS:
                        if (!_entityCache.IAmTank && _entityCache.TankUnit == null)
                        {
                            int tankMapId = int.Parse(messageParts[5]);
                            int mapId = Usefuls.ContinentId;
                            if (tankMapId == mapId)
                            {
                                float posX = float.Parse(messageParts[2]);
                                float posY = float.Parse(messageParts[3]);
                                float posZ = float.Parse(messageParts[4]);
                                int stepOrder = int.Parse(messageParts[6]);
                                string stepName = messageParts[7];
                                TankStatus = new PlayerStatus(new Vector3(posX, posY, posZ), stepOrder, stepName, mapId);
                                Logger.LogError($"Tank position is {TankStatus.Position}, mapId {TankStatus.MapId}, step name {TankStatus.StepName} order {TankStatus.StepOrder}");

                                if (_profileManager.CurrentDungeonProfile?.CurrentStep.Order > TankStatus.StepOrder)
                                {
                                    Logger.LogError($@"We are ahead of the tank ({TankStatus.StepOrder} / {_profileManager.CurrentDungeonProfile.CurrentStep.Order}). We must teleport out and back in");
                                }
                            }
                        }
                        break;
                    case ChatMessageType.WROBOT_START:
                        if (_entityCache.IAmTank)
                        {
                            BroadCastTankStatus();
                        }
                        break;
                    case ChatMessageType.PLAYER_ENTERING_WORLD:
                        if (_entityCache.IAmTank)
                        {
                            BroadCastTankStatus();
                        }
                        break;
                    case ChatMessageType.ASSIST_WITH_ENEMIES_AHEAD:
                        if (author != _entityCache.Me.Name)
                        {
                            Logger.LogError($"{author} is in trouble, porting out and back in to rejoin them");
                        }
                        break;
                }
            }
            else
            {
                Logger.LogError($"Message type unknown : {messageParts[1]}");
            }
        }
        */
        public void Broadcast(ChatMessageType messageType, string message)
        {
            Lua.LuaDoString($@"
                    SendChatMessage(""{_channelName}{_separator}{messageType}{_separator}{message}"", ""PARTY"");
                ");
        }

        internal class PlayerStatus
        {
            public Vector3 Position { get; }
            public int StepOrder { get; }
            public int MapId { get; }
            public string StepName { get; }

            public PlayerStatus(Vector3 position, int stepOrder, string stepName, int mapId)
            {
                Position = position;
                StepName = stepName;
                StepOrder = stepOrder;
                MapId = mapId;
            }
        }

        public enum ChatMessageType
        {
            TANK_STATUS,
            WROBOT_START,
            WROBOT_STOP,
            PLAYER_ENTERING_WORLD,
            ASSIST_WITH_ENEMIES_AHEAD
        }
    }
}