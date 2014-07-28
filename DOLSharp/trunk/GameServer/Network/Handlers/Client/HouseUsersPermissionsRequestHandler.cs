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
using DawnOfLight.GameServer.Housing;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.HouseUsersPermissionsRequest, ClientStatus.PlayerInGame)]
	public class HouseUsersPermissionsRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			int unk1 = packet.ReadByte();
			int unk2 = packet.ReadByte();
			ushort houseNumber = packet.ReadShort();

			// house is null, return
			var house = HouseMgr.GetHouse(houseNumber);
			if (house == null)
				return;

			// player has no owner permissions and isn't a GM or admin, return
			if (!house.HasOwnerPermissions(client.Player) && client.Account.PrivLevel <= 1)
				return;

			// build the packet
			client.Out.SendHouseUsersPermissions(house);
		}
	}
}