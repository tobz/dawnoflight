/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerPickUpRequest, ClientStatus.PlayerInGame)]
    public class PlayerPickUpRequestHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GamePacketIn packet)
        {
            uint X = packet.ReadInt();
            uint Y = packet.ReadInt();
            ushort id = packet.ReadShort();
            ushort obj = packet.ReadShort();

            var target = client.Player.TargetObject;
            if (target == null)
            {
                client.Out.SendMessage(
                    LanguageMgr.GetTranslation(client.Account.Language, "PlayerPickUpRequestHandler.HandlePacket.Target"),
                    ChatType.CT_System, ChatLocation.CL_SystemWindow);
                return;
            }

            if (target.ObjectState != GameObject.eObjectState.Active)
            {
                client.Out.SendMessage(
                    LanguageMgr.GetTranslation(client.Account.Language,
                        "PlayerPickUpRequestHandler.HandlePacket.InvalidTarget"), ChatType.CT_System,
                    ChatLocation.CL_SystemWindow);
                return;
            }

            client.Player.PickupObject(target, false);
        }
    }
}