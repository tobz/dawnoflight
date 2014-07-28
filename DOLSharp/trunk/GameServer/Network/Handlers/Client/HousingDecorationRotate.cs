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
using DawnOfLight.GameServer.Housing;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.HousingDecorationRotate, ClientStatus.PlayerInGame)]
	public class HousingDecorationRotateHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			int unk1 = packet.ReadByte();
			int position = packet.ReadByte();
			ushort houseNumber = packet.ReadShort();
			ushort angle = packet.ReadShort();
			ushort unk2 = packet.ReadShort();

			// rotation only works for inside items
			if (!client.Player.InHouse)
				return;

			// house is null, return
			var house = HouseMgr.GetHouse(houseNumber);
			if (house == null)
				return;

			// no permission to change the interior, return
			if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
				return;

			if (house.IndoorItems.ContainsKey(position) == false)
				return;

			// grab the item in question
			var iitem = house.IndoorItems[position];
			if (iitem == null)
			{
				client.Player.Out.SendMessage("error: id was null", ChatType.CT_Help, ChatLocation.CL_SystemWindow);
				return;
			} //should this ever happen?

			// adjust the item's roation
			int old = iitem.Rotation;
			iitem.Rotation = (iitem.Rotation + angle)%360;

			if (iitem.Rotation < 0)
			{
				iitem.Rotation = 360 + iitem.Rotation;
			}

			iitem.DatabaseItem.Rotation = iitem.Rotation;

			// save item
			GameServer.Database.SaveObject(iitem.DatabaseItem);

			ChatUtil.SendSystemMessage(client, string.Format("Interior decoration rotated from {0} degrees to {1}", old, iitem.Rotation));

			// update all players in the house.
			foreach (GamePlayer plr in house.GetAllPlayersInHouse())
			{
				plr.Client.Out.SendFurniture(house, position);
			}
		}
	}
}