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
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&target", (uint) ePrivLevel.Player, "target a player by name", "/target <playerName>")]
	public class TargetCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			GamePlayer targetPlayer = null;
			if (args.Length == 2)
			{
				int result = 0;
				GameClient targetClient = WorldMgr.GuessClientByPlayerNameAndRealm(args[1], 0, true, out result);
				if (targetClient != null)
				{
					targetPlayer = targetClient.Player;

					int dist = WorldMgr.GetDistance(client.Player, targetPlayer);
					if (dist > WorldMgr.YELL_DISTANCE || dist < 0 || targetPlayer.IsStealthed || !GameServer.ServerRules.IsSameRealm(client.Player, targetPlayer, true))
					{
						client.Out.SendMessage("You don't see " + args[1] + " around here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 0;
					}

					client.Out.SendChangeTarget(targetPlayer);
					client.Out.SendMessage("You target " + targetPlayer.GetName(0, true) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				if (client.Account.PrivLevel > 1) {
					IEnumerator en = client.Player.GetNPCsInRadius(800).GetEnumerator();
					while (en.MoveNext()) {
						if (((GameObject)en.Current).Name == args[1]) {
							client.Out.SendChangeTarget((GameObject)en.Current);
							client.Out.SendMessage("[GM] You target " + ((GameObject)en.Current).GetName(0, true) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;						
						}
					}
				}

				client.Out.SendMessage("You don't see " + args[1] + " around here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			if (client.Account.PrivLevel > 1) {
				client.Out.SendMessage("/target <player/mobname>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			} else {
				client.Out.SendMessage("/target <playername>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return 0;
		}
	}
}