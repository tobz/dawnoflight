using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects.CustomNPC;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerTrainWindow, ClientStatus.PlayerInGame)]
    public class PlayerTrainWindowHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GamePacketIn packet)
        {
            var trainer = client.Player.TargetObject as GameTrainer;
            if (trainer == null || (trainer.CanTrain(client.Player) == false && trainer.CanTrainChampionLevels(client.Player) == false))
            {
                client.Out.SendMessage("You must select a valid trainer for your class.", ChatType.CT_Important, ChatLocation.CL_ChatWindow);
                return;
            }

            client.Out.SendTrainerWindow();
        }
    }
}
