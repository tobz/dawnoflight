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

using System.Reflection;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Utilities;
using DawnOfLight.GameServer.World;
using log4net;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerCreationRequest, ClientStatus.LoggedIn)]
	public class PlayerCreationRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			ushort id = packet.ReadShort();
			var target = WorldMgr.GetClientFromID(id);
			if(target == null)
			{
				if (log.IsWarnEnabled)
					log.Warn(string.Format("Client {0}:{1} account {2} requested invalid client {3} --- disconnecting", client.SessionID, client.TcpEndpointAddress, client.Account == null ? "null" : client.Account.Name, id));

				client.Disconnect();
				return;
			}

			if(target.IsPlaying && target.Player != null && target.Player.ObjectState == GameObject.eObjectState.Active)
			{
				client.Out.SendPlayerCreate(target.Player);
				client.Out.SendLivingEquipmentUpdate(target.Player);
			}
		}
	}
}