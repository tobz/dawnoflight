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
using DawnOfLight.GameServer.ServerRules;

namespace DawnOfLight.GameServer.GameEvents
{
	/// <summary>
	/// Handles Tutorial Jump Point Exits
	/// </summary>
	public class TutorialJumpPointHandler : IJumpPointHandler
	{
		/// <summary>
		/// Decides whether player can jump to the target point.
		/// All messages with reasons must be sent here.
		/// Can change destination too.
		/// </summary>
		/// <param name="targetPoint">The jump destination</param>
		/// <param name="player">The jumping player</param>
		/// <returns>True if allowed</returns>
		public bool IsAllowedToJump(ZonePoint targetPoint, GamePlayer player)
		{
			StartupLocations.StartLocation loc = StartupLocations.MainTownStartingLocations[player.CharacterClass.ID] as StartupLocations.StartLocation;

			if (loc != null)
			{
				targetPoint.TargetX = loc.X;
				targetPoint.TargetY = loc.Y;
				targetPoint.TargetZ = loc.Z;
				targetPoint.TargetHeading = (ushort)loc.Heading;
				return true;
			}

			return false;
		}
	}
}
