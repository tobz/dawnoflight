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
using DawnOfLight.GameServer.Network.Crypto;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.CryptKeyRequest)]
	public class CryptKeyRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			int rc4 = packet.ReadByte();
			byte clientType = (byte)packet.ReadByte();
			client.ClientType = (GameClient.eClientType)(clientType & 0x0F);
			client.ClientAddons = (GameClient.eClientAddons)(clientType & 0xF0);
			byte major = (byte)packet.ReadByte();
			byte minor = (byte)packet.ReadByte();
			byte build = (byte)packet.ReadByte();

			if(rc4==1)
			{
			    var encoding = client.PacketProcessor.Encoding as PacketEncoding168;
			    if (encoding == null)
			        return;

				packet.Read(encoding.SBox, 0, 256);
				encoding.EncryptionState = eEncryptionState.PseudoRC4Encrypted;
			}
			else
			{
                client.Out.SendVersionAndCryptKey();
			}
		}
	}
}