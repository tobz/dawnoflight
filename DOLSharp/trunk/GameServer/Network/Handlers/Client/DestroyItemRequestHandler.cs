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
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.DestroyItemRequest)]
	public class DestroyItemRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			packet.Skip(4);
			int slot = packet.ReadShort();

			var item = client.Player.Inventory.GetItem((InventorySlot)slot);
		    if (item == null)
                return;

		    if (item.IsIndestructible)
		    {
		        client.Out.SendMessage(String.Format("You can't destroy {0}!", item.GetName(0, false)), ChatType.CT_System, ChatLocation.CL_SystemWindow);
		        return;
		    }

		    if (item.Id_nb == "ARelic")
		    {
		        client.Out.SendMessage("You cannot destroy a relic!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
		        return;
		    }

		    if (client.Player.Inventory.EquippedItems.Contains(item))
		    {
		        client.Out.SendMessage("You cannot destroy an equipped item!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
		        return;
		    }

		    if (client.Player.Inventory.RemoveItem(item))
		    {
		        client.Out.SendMessage("You destroy the " + item.Name + ".", ChatType.CT_System, ChatLocation.CL_SystemWindow);
		        InventoryLogging.LogInventoryAction(client.Player, "(destroy)", eInventoryActionType.Other, item.Template, item.Count);
		    }
		}
	}
}
