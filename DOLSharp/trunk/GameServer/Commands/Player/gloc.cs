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

using DawnOfLight.GameServer.World.Instance;

namespace DawnOfLight.GameServer.commands.Player
{
	[Command("&gloc", //command to handle
		ePrivLevel.Player, //minimum privelege level
		"Show the current coordinates", //command description
		"/gloc")] //command usage
	public class GlocCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "gloc"))
				return;

			DisplayMessage(client, string.Format("You are at X:{0} Y:{1} Z:{2} Heading:{3} Region:{4} {5}",
				client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading, client.Player.CurrentRegionID,
				client.Player.CurrentRegion is BaseInstance ? string.Format("Skin:{0}", client.Player.CurrentRegion.Skin) : ""));
		}
	}
}