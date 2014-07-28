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
using DawnOfLight.GameServer.Keeps;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
	[PacketHandler(PacketType.TCP, ClientPackets.BuyHookPoint, ClientStatus.PlayerInGame)]
	public class BuyHookPointHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			ushort keepId = packet.ReadShort();
			ushort wallId = packet.ReadShort();
			int hookpointId = packet.ReadShort();
            ushort itemSlot = packet.ReadShort();
			int payType = packet.ReadByte();
			int unk2 = packet.ReadByte();
			int unk3 = packet.ReadByte();
			int unk4 = packet.ReadByte();

			var keep = GameServer.KeepManager.GetKeepByID(keepId);
			if (keep == null)
                return;

			var component = keep.KeepComponents[wallId];
			if (component == null)
                return;

            var inventory = HookPointInventory.RedHPInventory; // guard
			if(hookpointId > 0x80) inventory = HookPointInventory.YellowHPInventory; // oil
			else if(hookpointId > 0x60) inventory = HookPointInventory.GreenHPInventory;// big siege
			else if(hookpointId > 0x40) inventory = HookPointInventory.LightGreenHPInventory; // small siege
			else if (hookpointId > 0x20) inventory = HookPointInventory.BlueHPInventory;// npc

		    if (inventory == null)
                return;

		    var item = inventory.GetItem(itemSlot);
		    if (item != null)
		    {
		        item.Invoke(client.Player, payType, component.HookPoints[hookpointId] as GameKeepHookPoint, component);
		    }
		}
	}
}