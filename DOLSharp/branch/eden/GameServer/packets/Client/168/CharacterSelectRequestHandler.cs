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
using log4net;
using System;
namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0xB8 ^ 168, "Handles setting SessionID and the active character")]
	public class CharacterSelectRequestHandler : IPacketHandler
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int packetVersion;
			switch (client.Version)
			{
				case GameClient.eClientVersion.Version168:
				case GameClient.eClientVersion.Version169:
				case GameClient.eClientVersion.Version170:
				case GameClient.eClientVersion.Version171:
				case GameClient.eClientVersion.Version172:
				case GameClient.eClientVersion.Version173:
					packetVersion = 168;
					break;
				default:
					packetVersion = 174;
					break;
			}

			ushort sessionID = packet.ReadShort();
			ushort type = packet.ReadShortLowEndian();

			if (packetVersion == 174)
			{
				packet.Skip(1);
			}

			string charName = packet.ReadString(28);

			if (packetVersion == 174)
				packet.Skip(3); //unk

			string loginName = packet.ReadString(20);
			uint clientSignature = packet.ReadIntLowEndian();
			packet.Skip(24);

			if (client.Version >= GameClient.eClientVersion.Version1104)
			{
				packet.Skip(4);
			}

			uint flag = packet.ReadIntLowEndian();
			//int socket = packet.ReadShort();


			if (flag != 0)
			{
				switch (type) //type of check
				{
					case 0xFF: //cheat protection
						{
							try
							{
								List<string> flags = Util.SplitCSV(client.Account.HackFlags);
								if (!flags.Contains(flag.ToString("X8")))
									flags.Add(flag.ToString("X8"));
								string datas = "";
								foreach (string f in flags)
									datas += ";" + f;
								if (datas.StartsWith(";")) datas = datas.Substring(1);
								client.Account.HackFlags = datas;

								foreach (GameClient c in WorldMgr.GetAllPlayingClients())
								{
									if (c.Account.PrivLevel < 2) continue;
									c.Out.SendMessage("[Hack] Hook detected account [" + client.Account.Name + "]" + (client.Player != null ? " player [" + client.Player.Name + "]" : "") + " flag=" + flag.ToString("X8"), eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
								}
							}
							catch (Exception ex)
							{
								log.Error("HackFlag", ex);
							}
						}
						break;
				}
			}


			//TODO Character handling 
			if (charName.Equals("noname"))
			{
				client.Out.SendSessionID();
			}
			else
			{
				// SH: Also load the player if client player is NOT null but their charnames differ!!!
				// only load player when on charscreen and player is not loaded yet
				// packet is sent on every region change (and twice after "play" was pressed)
				if (
					(
						(client.Player == null && client.Account.Characters != null)
						|| (client.Player != null && client.Player.Name.ToLower() != charName.ToLower())
					) && client.ClientState == GameClient.eClientState.CharScreen)
				{
					bool charFound = false;
					for (int i = 0; i < client.Account.Characters.Length; i++)
					{
						if (client.Account.Characters[i] != null
						    && client.Account.Characters[i].Name == charName)
						{
							charFound = true;
							client.LoadPlayer(i);
							break;
						}
					}
					if (charFound == false)
					{
						client.Player = null;
						client.ActiveCharIndex = -1;
					}
					else
					{
						// Log character play
						AuditMgr.AddAuditEntry(client, AuditType.Character, AuditSubtype.CharacterLogin, "", charName);
					}
				}

				if (client.Player == null)
				{
					// client keeps sending the name of the deleted char even if new one was created, correct name only after "play" button pressed
					//client.Server.Error(new Exception("ERROR, active character not found!!! name="+charName));
				}

				client.Out.SendSessionID();
			}
		}

		#endregion
	}
}