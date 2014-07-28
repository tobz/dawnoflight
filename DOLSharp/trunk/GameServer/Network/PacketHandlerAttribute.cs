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

namespace DawnOfLight.GameServer.Network
{
	/// <summary>
	/// Denotes a class as a packet handler
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PacketHandlerAttribute : Attribute 
	{
        public PacketHandlerAttribute(PacketType type, ClientPackets code)
        {
            Type = type;
            Code = code;
            Description = "";
            PreprocessorID = ClientStatus.None;
        }

        public PacketHandlerAttribute(PacketType type, ClientPackets code, string description)
        {
            Type = type;
            Code = code;
            Description = description;
            PreprocessorID = ClientStatus.None;
        }

		public PacketHandlerAttribute(PacketType type, ClientPackets code, ClientStatus preprocessorId)
		{
            Type = type;
            Code = code;
            Description = "";
		    PreprocessorID = preprocessorId;
		}

        public PacketHandlerAttribute(PacketType type, ClientPackets code, ClientStatus preprocessorId, string description)
        {
            Type = type;
            Code = code;
            Description = description;
            PreprocessorID = preprocessorId;
        }

		/// <summary>
		/// The packet type (TCP, UDP) that this handler works for.
		/// </summary>
		public PacketType Type { get; private set; }

		/// <summary>
		/// The packet ID that this handler maps to.
		/// </summary>
		public ClientPackets Code { get; private set; }

		/// <summary>
		/// The description of the handler and what it does.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// The preprocessor IDs, if any, to run before invoking this handler.
		/// </summary>
		public ClientStatus PreprocessorID { get; private set; }
	}
}
