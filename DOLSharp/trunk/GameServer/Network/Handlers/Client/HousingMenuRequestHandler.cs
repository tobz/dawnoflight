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
    [PacketHandler(PacketType.TCP, ClientPackets.HousingMenuRequest, ClientStatus.PlayerInGame)]
	public class HousingMenuRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			int houseNumber = packet.ReadShort();
			int menuId = packet.ReadByte();
			int flag = packet.ReadByte();

			var house = HouseMgr.GetHouse(client.Player.CurrentRegionID, houseNumber);
			if (house == null)
				return;

			client.Player.CurrentHouse = house;

			switch (menuId)
			{
				case 0: // Exterior decoration (Garden)
					{
						if (!house.CanChangeGarden(client.Player, DecorationPermissions.Add))
							return;

						HouseMgr.SendHousingMerchantWindow(client.Player, eMerchantWindowType.HousingOutsideShop);
						break;
					}
				case 1: // Interior decoration
					{
						if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
							return;

						HouseMgr.SendHousingMerchantWindow(client.Player, eMerchantWindowType.HousingInsideShop);
						break;
					}
				case 2: // Exterior menu
					{
						if (!house.CanChangeGarden(client.Player, DecorationPermissions.Add))
							return;

						client.Player.Out.SendMerchantWindow(HouseTemplateMgr.OutdoorMenuItems, eMerchantWindowType.HousingOutsideMenu);
						break;
					}
				case 3: // interior npc
					{
						if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
							return;

						HouseMgr.SendHousingMerchantWindow(client.Player, eMerchantWindowType.HousingNPCHookpoint);
						break;
					}
				case 4: // vault shop
					{
						if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
							return;

						HouseMgr.SendHousingMerchantWindow(client.Player, eMerchantWindowType.HousingVaultHookpoint);
						break;
					}
				case 5: // craft shop
					{
						if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
							return;

						HouseMgr.SendHousingMerchantWindow(client.Player, eMerchantWindowType.HousingCraftingHookpoint);
						break;
					}
				case 6: // bindstone shop
					{
						if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
							return;

						HouseMgr.SendHousingMerchantWindow(client.Player, eMerchantWindowType.HousingBindstoneHookpoint);
						break;
					}
				case 7:
					house.SendHouseInfo(client.Player);
					break;

				case 8: // Interior menu (flag = 0x00 - roof, 0xFF - floor or wall)
					if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
						return;

					client.Player.Out.SendMerchantWindow(HouseTemplateMgr.IndoorMenuItems, eMerchantWindowType.HousingInsideMenu);
					break;

				default:
					client.Out.SendMessage("Invalid menu id " + menuId + " (hookpoint?).", ChatType.CT_System, ChatLocation.CL_SystemWindow);
					break;
			}

		}
	}
}