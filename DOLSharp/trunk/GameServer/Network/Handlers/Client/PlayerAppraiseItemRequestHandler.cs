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

using DawnOfLight.Database;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Housing;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
	[PacketHandler(PacketType.TCP, ClientPackets.PlayerAppraiseItemRequest, ClientStatus.PlayerInGame)]
	public class PlayerAppraiseItemRequestHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			uint X = packet.ReadInt();
			uint Y = packet.ReadInt();
			ushort id = packet.ReadShort();
			ushort item_slot = packet.ReadShort();

			new AppraiseActionHandler(client.Player, item_slot).Start(1);
		}

		#endregion

		#region Nested type: AppraiseActionHandler

		/// <summary>
		/// Handles item apprise actions
		/// </summary>
		protected class AppraiseActionHandler : RegionAction
		{
			/// <summary>
			/// The item slot
			/// </summary>
			protected readonly int m_slot;

			/// <summary>
			/// Constructs a new AppraiseAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="slot">The item slot</param>
			public AppraiseActionHandler(GamePlayer actionSource, int slot) : base(actionSource)
			{
				m_slot = slot;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;

				if (player.TargetObject == null)
					return;

				InventoryItem item = player.Inventory.GetItem((InventorySlot) m_slot);

				if (player.TargetObject is GameMerchant)
				{
					((GameMerchant) player.TargetObject).OnPlayerAppraise(player, item, false);
				}
				else if (player.TargetObject is GameLotMarker)
				{
					((GameLotMarker) player.TargetObject).OnPlayerAppraise(player, item, false);
				}
			}
		}

		#endregion
	}
}