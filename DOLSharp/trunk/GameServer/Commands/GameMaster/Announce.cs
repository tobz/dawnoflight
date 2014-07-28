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

using System.Collections.Generic;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.commands.GameMaster
{
	[Command(
		"&announce",
		ePrivLevel.GM,
	    "GMCommands.Announce.Description",
	    "GMCommands.Announce.Usage")]
	public class AnnounceCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				DisplaySyntax(client);
				return;
			}

			string message = string.Join(" ", args, 2, args.Length - 2);
			if (message == "")
				return;

			switch (args.GetValue(1).ToString().ToLower())
			{
				#region Log
				case "log":
					{
						foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
                            if(clients != null)
							    clients.Out.SendMessage(LanguageMgr.GetTranslation(clients, "GMCommands.Announce.LogAnnounce", message), ChatType.CT_Important, ChatLocation.CL_SystemWindow);
						break;
					}
				#endregion Log
				#region Window
				case "window":
					{
						var messages = new List<string>();
						messages.Add(message);

						foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
                            if(clients != null)
							    clients.Player.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(clients, "GMCommands.Announce.WindowAnnounce", client.Player.Name), messages);
						break;
					}
				#endregion Window
				#region Send
				case "send":
					{
						foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
                            if(clients != null)
							    clients.Out.SendMessage(LanguageMgr.GetTranslation(clients, "GMCommands.Announce.SendAnnounce", message), ChatType.CT_Send, ChatLocation.CL_ChatWindow);
						break;
					}
				#endregion Send
				#region Center
				case "center":
					{
                        foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
                            if (clients != null)
							    clients.Out.SendMessage(message, ChatType.CT_ScreenCenter, ChatLocation.CL_SystemWindow);
						break;
					}
				#endregion Center
				#region Confirm
				case "confirm":
					{
						foreach (GameClient clients in WorldMgr.GetAllPlayingClients())
                            if(clients != null)
							    clients.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, LanguageMgr.GetTranslation(clients, "GMCommands.Announce.ConfirmAnnounce", client.Player.Name, message));
						break;
					}
				#endregion Confirm
				#region Default
				default:
					{
						DisplaySyntax(client);
						return;
					}
				#endregion Default
			}
		}
	}
}
