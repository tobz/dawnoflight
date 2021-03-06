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

using DawnOfLight.Base.Network;

namespace DawnOfLight.GameServer.Network.Packets
{
	/// <summary>
	/// An outgoing TCP packet
	/// </summary>
	public class GameTCPPacketOut : PacketOut
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="packetCode">ID of the packet</param>
		public GameTCPPacketOut(byte packetCode)
		{
			base.WriteShort(0x00); //reserved for size
			base.WriteByte(packetCode);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="packetCode">ID of the packet</param>
		public GameTCPPacketOut(byte packetCode, int startingSize) : base(startingSize + 3)
		{
			base.WriteShort(0x00); //reserved for size
			base.WriteByte(packetCode);
		}
	}
}