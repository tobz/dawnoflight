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

using System;
using DawnOfLight.Database;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Housing;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerSellRequest, ClientStatus.PlayerInGame)]
	public class PlayerSellRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			uint x = packet.ReadInt();
			uint y = packet.ReadInt();
			ushort id = packet.ReadShort();
			ushort itemSlot = packet.ReadShort();

			if (client.Player.TargetObject == null)
			{
				client.Out.SendMessage("You must select an NPC to sell to.", ChatType.CT_Merchant, ChatLocation.CL_SystemWindow);
				return;
			}

			lock (client.Player.Inventory)
			{
				InventoryItem item = client.Player.Inventory.GetItem((InventorySlot)itemSlot);
				if (item == null)
					return;

				int itemCount = Math.Max(1, item.Count);
				int packSize = Math.Max(1, item.PackSize);

			    var merchant = client.Player.TargetObject as GameMerchant;
			    if (merchant != null)
				{
					//Let the merchant choos how to handle the trade.
					merchant.OnPlayerSell(client.Player, item);

				}
				else
			    {
			        var lotMarker = client.Player.TargetObject as GameLotMarker;
			        if (lotMarker != null)
			        {
			            lotMarker.OnPlayerSell(client.Player, item);
			        }
			    }
			}
		}
	}
}