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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.LookingForAGroup, ClientStatus.PlayerInGame)]
	public class LookingForAGroupHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
		    var playersSearching = new List<GamePlayer>();

			byte grouped = (byte)packet.ReadByte();
			if (grouped != 0x00)
			{
			    var groups = GroupMgr.ListGroupByStatus(0x00);
			    playersSearching.AddRange(groups.Where(x => GameServer.ServerRules.IsAllowedToGroup(x.Leader, client.Player, true)).Select(x => x.Leader));
			}

		    var lfgPlayers = GroupMgr.LookingForGroupPlayers();
		    playersSearching.AddRange(lfgPlayers.Where(x => x != client.Player && GameServer.ServerRules.IsAllowedToGroup(client.Player, x, true)));

		    client.Out.SendFindGroupWindowUpdate(playersSearching);
		}
	}
}