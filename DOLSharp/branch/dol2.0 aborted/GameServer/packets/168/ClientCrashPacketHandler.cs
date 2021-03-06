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
using DOL;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute (PacketHandlerType.TCP, 0x9F^168, "Handles client crash packets")]
	public class ClientCrashPacketHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket (GameClient client, GSPacketIn packet)
		{
			lock (this)
			{
				string dllName = packet.ReadString(16);
				packet.Position = 0x50;
				uint upTime = packet.ReadInt();
				string text = string.Format("Client crash ({0}) dll:{1} clientUptime:{2}sec", client.ToString(), dllName, upTime);
				if (log.IsErrorEnabled)
					log.Error(text);

//				if(log.IsDebugEnabled)
//				{
//					log.Debug("Last Server => Client packets (from older to newer, first 3 bytes are size and code):");
//					foreach (PacketOut pak in client.PacketProcessor.GetSentPackets())
//					{
//						byte[] buff = pak.GetBuffer();
//						log.Debug(Marshal.ToHexDump(string.Format("Sent packet code=0x{0:X2} pak type:{1}", buff[2], pak.GetType().Name), buff));
//					}
//					log.Debug("Last Client => Server packets (from older to newer):");
//					foreach (GSPacketIn pak in client.PacketProcessor.GetReceivedPackets())
//					{
//						pak.LogDump();
//					}
//				}
				
				if (log.IsDebugEnabled)
				{
					log.Debug("Last client sent/received packets (from older to newer):");
					foreach (object obj in client.PacketProcessor.GetLastPackets())
					{
						PacketOut outPak = obj as PacketOut;
						GSPacketIn inPak = obj as GSPacketIn;
						if (outPak != null)
						{
							log.Debug(Marshal.ToHexDump(outPak.GetType().FullName + ":", outPak.GetBuffer()));
						}
						else if (inPak != null)
						{
							inPak.LogDump();
						}
						else
						{
							log.Debug("Unknown packet type: " + obj.GetType().FullName);
						}
					}
				}
				return 1;
			}
		}
	}
}
