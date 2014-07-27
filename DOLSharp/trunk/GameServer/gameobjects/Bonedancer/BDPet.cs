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
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using DawnOfLight.GameServer;
using DawnOfLight.GameServer.Spells;
using DawnOfLight.AI.Brain;
using DawnOfLight.Events;
using log4net;
using DawnOfLight.GameServer.PacketHandler;
using DawnOfLight.Database;
using System.Collections;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.GameServer.Styles;

namespace DawnOfLight.GameServer
{
	public class BDPet : GamePet
	{
		/// <summary>
		/// Proc IDs for various pet weapons.
		/// </summary>
		private enum Procs
		{
			Cold = 32050,
			Disease = 32014,
			Heat = 32053,
			Poison = 32013,
			Stun = 2165
		};

		/// <summary>
		/// Create a commander.
		/// </summary>
		/// <param name="npcTemplate"></param>
		/// <param name="owner"></param>
		public BDPet(INpcTemplate npcTemplate) : base(npcTemplate) { }
	}
}