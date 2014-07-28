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

using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.commands.GameMaster
{
	[Command(
	"&team",
	new string[] { "&te" },
   ePrivLevel.GM,
	"Server broadcast message for administrators",
	"/team <message>")]

	public class TeamCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				client.Out.SendMessage("Use: /team <message>", ChatType.CT_System, ChatLocation.CL_SystemWindow);
				return;
			}

			string msg = string.Join(" ", args, 1, args.Length - 1);

			foreach (GameClient player in WorldMgr.GetAllPlayingClients())
			{
				if (player.Account.PrivLevel > 1)
				{
					player.Out.SendMessage("[StaffInformation-" + client.Player.Name + "]:\n " + msg, ChatType.CT_Staff, ChatLocation.CL_ChatWindow);
				}
			}
		}
	}
}