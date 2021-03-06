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

using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.commands.Player
{
	/// <summary>
	/// Command handler to handle emotes
	/// </summary>
	[Command(
		"&emote", new string[] {"&em", "&e"},
		ePrivLevel.Player,
		"Roleplay an action or emotion", "/emote <text>")]
	public class CustomEmoteCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// Method to handle the command from the client
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "emote"))
				return;

			// no emotes if dead
			if (!client.Player.IsAlive)
			{
				client.Out.SendMessage("You can't emote while dead!", ChatType.CT_Emote, ChatLocation.CL_SystemWindow);
				return;
			}

			if (args.Length < 2)
			{
				client.Out.SendMessage("You need something to emote.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
				return;
			}

			if (client.Player.IsMuted)
			{
				client.Player.Out.SendMessage("You have been muted and cannot emote!", ChatType.CT_Staff, ChatLocation.CL_SystemWindow);
				return;
			}

			string ownRealm = string.Join(" ", args, 1, args.Length - 1);
			ownRealm = "<" + client.Player.Name + " " + ownRealm + " >";

			string diffRealm = "<" + client.Player.Name + " makes strange motions.>";

			foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
			{
				if (GameServer.ServerRules.IsAllowedToUnderstand(client.Player, player))
				{
					player.Out.SendMessage(ownRealm, ChatType.CT_Emote, ChatLocation.CL_ChatWindow);
				}
				else
				{
                    if (!player.IsIgnoring(client.Player as GameLiving))
					player.Out.SendMessage(diffRealm, ChatType.CT_Emote, ChatLocation.CL_ChatWindow);
				}
			}
		}
	}
}