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
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerBuyRequest, ClientStatus.PlayerInGame)]
	public class PlayerBuyRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			uint X = packet.ReadInt();
			uint Y = packet.ReadInt();
			ushort id = packet.ReadShort();
			ushort itemSlot = packet.ReadShort();
			byte itemCount = (byte)packet.ReadByte();
			byte menuId = (byte)packet.ReadByte();

			switch ((eMerchantWindowType)menuId)
			{
				case eMerchantWindowType.HousingInsideShop:
				case eMerchantWindowType.HousingOutsideShop:
				case eMerchantWindowType.HousingBindstoneHookpoint:
				case eMerchantWindowType.HousingCraftingHookpoint:
				case eMerchantWindowType.HousingNPCHookpoint:
				case eMerchantWindowType.HousingVaultHookpoint:
					{
						HouseMgr.BuyHousingItem(client.Player, itemSlot, itemCount, (eMerchantWindowType)menuId);
						break;
					}
				default:
					{
						if (client.Player.TargetObject == null)
							return;

						//Forward the buy process to the merchant
						if (client.Player.TargetObject is GameMerchant)
						{
							//Let merchant choose what happens
							((GameMerchant)client.Player.TargetObject).OnPlayerBuy(client.Player, itemSlot, itemCount);
						}
						else if (client.Player.TargetObject is GameLotMarker)
						{
							((GameLotMarker)client.Player.TargetObject).OnPlayerBuy(client.Player, itemSlot, itemCount);
						}
						break;
					}
			}
		}
	}
}