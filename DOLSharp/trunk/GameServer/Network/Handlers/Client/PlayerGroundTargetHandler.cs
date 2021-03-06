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
	[PacketHandler(PacketType.TCP, ClientPackets.PlayerGroundTarget, ClientStatus.PlayerInGame)]
	public class PlayerGroundTargetHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			var groundX = (int) packet.ReadInt();
			var groundY = (int) packet.ReadInt();
			var groundZ = (int) packet.ReadInt();
			ushort flag = packet.ReadShort();
//			ushort unk2 = packet.ReadShort();

			new ChangeGroundTargetHandler(client.Player, groundX, groundY, groundZ, flag).Start(1);
		}

		#endregion

		#region Nested type: ChangeGroundTargetHandler

		/// <summary>
		/// Handles ground target changes
		/// </summary>
		protected class ChangeGroundTargetHandler : RegionAction
		{
			protected readonly ushort m_flag;

			/// <summary>
			/// The new ground X
			/// </summary>
			protected readonly int m_x;

			/// <summary>
			/// The new ground Y
			/// </summary>
			protected readonly int m_y;

			/// <summary>
			/// The new ground Z
			/// </summary>
			protected readonly int m_z;

			/// <summary>
			/// Constructs a new ChangeGroundTargetHandler
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="x">The new ground X</param>
			/// <param name="y">The new ground Y</param>
			/// <param name="z">The new ground Z</param>
			/// <param name="flag"></param>
			public ChangeGroundTargetHandler(GamePlayer actionSource, int x, int y, int z, ushort flag) : base(actionSource)
			{
				m_x = x;
				m_y = y;
				m_z = z;
				m_flag = flag;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;
				player.GroundTargetInView = ((m_flag & 0x100) != 0);
				player.SetGroundTarget(m_x, m_y, (ushort) m_z);

				if (!player.GroundTargetInView)
					player.Out.SendMessage("Your ground target is not visible!", ChatType.CT_System, ChatLocation.CL_SystemWindow);

				if (player.SiegeWeapon != null)
				{
					player.SiegeWeapon.Move();
					return;
				}
				if (player.Steed != null && player.Steed.MAX_PASSENGERS >= 1)
				{
					if (player.Steed is GameTaxiBoat) return;
					if (player.Steed is GameBoat)
						// Ichi - && player.GroundTarget.Z > player.CurrentZone.ZoneRegion.WaterLevel) return;
					{
						if (player.Steed.OwnerID == player.InternalID)
						{
							player.Out.SendMessage("You usher your boat forward.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							player.Steed.WalkTo(player.GroundTarget, player.Steed.MaxSpeed);
							return;
						}
					}

					if (player.Steed.MAX_PASSENGERS > 8 && player.Steed.CurrentRiders.Length < player.Steed.REQUIRED_PASSENGERS)
					{
						player.Out.SendMessage("The " + player.Steed.Name + " does not yet have enough passengers to move!",
						                       ChatType.CT_System, ChatLocation.CL_SystemWindow);
						return;
					}
					player.Steed.WalkTo(player.GroundTarget, player.Steed.MaxSpeed);
					return;
				}
			}
		}

		#endregion
	}
}