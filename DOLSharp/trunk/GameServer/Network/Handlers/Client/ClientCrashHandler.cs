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
using DawnOfLight.Base.Network;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Utilities;
using log4net;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.ClientCrash)]
	public class ClientCrashHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			lock (this)
			{
				var dllName = packet.ReadString(16);
				packet.Position = 0x50;
				var uptime = packet.ReadInt();
				if (log.IsInfoEnabled)
                    log.InfoFormat("Client crash ({0}) dll:{1} clientUptime:{2}sec", client, dllName, uptime);

				if (log.IsDebugEnabled)
				{
					log.Debug("Last client sent/received packets (most recent last):");
					
					foreach (var previousPacket in client.PacketProcessor.GetLastPackets())
					{
						log.Info(previousPacket.ToHumanReadable());
					}
				}
					
				if(client.Player != null)
				{
					client.Out.SendPlayerQuit(true);
					client.Player.SaveIntoDatabase();
					client.Player.Quit(true);
					client.Disconnect();
				}
			}
		}
	}
}
