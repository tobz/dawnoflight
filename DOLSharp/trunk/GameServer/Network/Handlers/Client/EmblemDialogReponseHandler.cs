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

using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
	/// <summary>
	/// EmblemDialogReponseHandler is the response of client wend when we close the emblem selection dialogue.
	/// </summary>
    [PacketHandler(PacketType.TCP, ClientPackets.EmblemDialogReponse, ClientStatus.PlayerInGame)]
	public class EmblemDialogReponseHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			if(client.Player.Guild == null)
				return;

			if(!client.Player.Guild.HasRank(client.Player, Guild.eRank.Leader))
				return;

			int primaryColor = packet.ReadByte() & 0x0F; //4bits
			int secondaryColor = packet.ReadByte() & 0x07; //3bits
			int pattern = packet.ReadByte() & 0x03; //2bits
			int logo = packet.ReadByte(); //8bits

			int oldEmblem = client.Player.Guild.Emblem;
			int newEmblem = ((logo << 9) | (pattern << 7) | (primaryColor << 3) | secondaryColor);
			if (GuildMgr.IsEmblemUsed(newEmblem))
			{
				client.Player.Out.SendMessage("This emblem is already in use by another guild, please choose another!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
				return;
			}

			GuildMgr.ChangeEmblem(client.Player, oldEmblem, newEmblem);
		}
	}
}
