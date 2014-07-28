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

using System.Reflection;
using DawnOfLight.GameServer.commands;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.Commands
{
	[Command("&serverinfo", //command to handle
		ePrivLevel.Player, //minimum privelege level
		"Shows information about the server", //command description
		"/serverinfo")] //usage
	public class ServerInfoCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			client.Out.SendMessage(GameServer.Instance.Configuration.ServerName, ChatType.CT_Important, ChatLocation.CL_SystemWindow);
			AssemblyName an = Assembly.GetAssembly(typeof(GameServer)).GetName();
			client.Out.SendMessage("version: " + an.Version, ChatType.CT_System, ChatLocation.CL_SystemWindow);
			client.Out.SendMessage("type: " + GameServer.Instance.Configuration.ServerType + " (" + GameServer.ServerRules.RulesDescription() + ")", ChatType.CT_System, ChatLocation.CL_SystemWindow);
			client.Out.SendMessage("playing: " + WorldMgr.GetAllPlayingClientsCount(), ChatType.CT_System, ChatLocation.CL_SystemWindow);
			if (client.Player != null)
			{
				long sec = client.Player.CurrentRegion.Time / 1000;
				long min = sec / 60;
				long hours = min / 60;
				long days = hours / 24;
				DisplayMessage(client, string.Format("uptime: {0}d {1}h {2}m {3:00}s", days, hours % 24, min % 60, sec % 60));
			}
		}
	}
}