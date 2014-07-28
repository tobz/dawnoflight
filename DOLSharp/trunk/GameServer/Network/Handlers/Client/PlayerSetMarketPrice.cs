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
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerSetMarketPrice, ClientStatus.PlayerInGame)]
    public class PlayerSetMarketPriceHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GamePacketIn packet)
        {
			int slot = packet.ReadByte();
			int unk1 = packet.ReadByte();
			ushort unk2 = packet.ReadShort();
			uint price = packet.ReadInt();

			// only IGameInventoryObjects can handle set price commands
			if (client.Player.TargetObject == null || (client.Player.TargetObject is IGameInventoryObject) == false)
				return;

			(client.Player.TargetObject as IGameInventoryObject).SetSellPrice(client.Player, (ushort)slot, price);
        }
    }
}