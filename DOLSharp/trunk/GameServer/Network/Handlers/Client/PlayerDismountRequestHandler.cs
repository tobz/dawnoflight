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
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
	[PacketHandler(PacketType.TCP, ClientPackets.PlayerDismountRequest, ClientStatus.PlayerInGame)]
	public class PlayerDismountRequestHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			new DismountRequestHandler(client.Player).Start(1);
		}

		#endregion

		#region Nested type: DismountRequestHandler

		/// <summary>
		/// Handles player dismount requests
		/// </summary>
		protected class DismountRequestHandler : RegionAction
		{
			/// <summary>
			/// Constructs a new DismountRequestHandler
			/// </summary>
			/// <param name="actionSource"></param>
			public DismountRequestHandler(GamePlayer actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;

				if (!player.IsRiding)
				{
					ChatUtil.SendSystemMessage(player, "You are not riding any steed!");
					return;
				}

				player.DismountSteed(false);
			}
		}

		#endregion
	}
}