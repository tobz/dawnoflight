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
    [PacketHandler(PacketType.TCP, ClientPackets.KeepComponentInteract, ClientStatus.PlayerInGame)]
	public class KeepComponentInteractHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			ushort keepId = packet.ReadShort();
			ushort wallId = packet.ReadShort();
			ushort response = packet.ReadShort();
			int hpIndex = packet.ReadShort();

			var keep = GameServer.KeepManager.GetKeepByID(keepId);
			if (keep == null || !(GameServer.ServerRules.IsSameRealm(client.Player, keep.KeepComponents[wallId], true) || client.Account.PrivLevel > 1))
				return;

		    if (response == 0x00) //show info
		    {
		        client.Out.SendKeepComponentInteract(keep.KeepComponents[wallId]);
		    }
			else if (response == 0x01) // click on hookpoint button
			{
			    client.Out.SendKeepComponentHookPoint(keep.KeepComponents[wallId], hpIndex);
			}
			else if (response == 0x02)//select an hookpoint
			{
				var hp = keep.KeepComponents[wallId];
				client.Out.SendClearKeepComponentHookPoint(hp, hpIndex);
				client.Out.SendHookPointStore(hp.HookPoints[hpIndex] as GameKeepHookPoint);
			}
		}
	}
}
