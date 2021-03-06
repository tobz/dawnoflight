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

namespace DOL
{
	/// <summary>
	/// This class is used to hold statistics about DOL usage
	/// </summary>
	public class Statistics
	{
		/// <summary>
		/// The total mob count
		/// </summary>
		public static int MemMobCount = 0;
		/// <summary>
		/// The total player count
		/// </summary>
		public static int MemPlayerCount = 0;
		/// <summary>
		/// The total account count
		/// </summary>
		public static int MemAccCount = 0;
		/// <summary>
		/// The total character count
		/// </summary>
		public static int MemCharCount = 0;
		/// <summary>
		/// The total incoming packet objects count
		/// </summary>
		public static int MemPacketInObj = 0;
		/// <summary>
		/// The total outgoing packet objects count
		/// </summary>
		public static int MemPacketOutObj = 0;
		/// <summary>
		/// The total spellhandler objects
		/// </summary>
		public static int MemSpellHandlerObj = 0;
		/// <summary>
		/// The total bytes sent
		/// </summary>
		public static long BytesOut = 0;
		/// <summary>
		/// The total bytes received
		/// </summary>
		public static long BytesIn = 0;
		/// <summary>
		/// The total outgoing packets
		/// </summary>
		public static long PacketsOut = 0;
		/// <summary>
		/// The total incoming packets
		/// </summary>
		public static long PacketsIn = 0;
	}
}
