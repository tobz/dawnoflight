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
using DawnOfLight.GameServer.Housing;
using DawnOfLight.GameServer.Network.Packets;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.HousingDecorationRotateRequest, ClientStatus.PlayerInGame)]
	public class HousingDecorationRotateRequestHandler : IPacketHandler
	{
	    public void HandlePacket(GameClient client, GamePacketIn packet)
	    {
	        ushort houseNumber = packet.ReadShort();
	        var index = (byte) packet.ReadByte();
	        var unk1 = (byte) packet.ReadByte();

	        // house is null, return
	        var house = HouseMgr.GetHouse(houseNumber);
	        if (house == null)
	            return;

	        // rotation only works for inside items
	        if (!client.Player.InHouse)
	            return;

	        // no permission to change the interior, return
	        if (!house.CanChangeInterior(client.Player, DecorationPermissions.Add))
	            return;

	        using (var pak = new GameTCPPacketOut(client.Out.GetPacketCode(ServerPackets.HouseDecorationRotate)))
	        {
	            pak.WriteShort(houseNumber);
	            pak.WriteByte(index);
	            pak.WriteByte(0x01);

	            client.Out.SendTCP(pak);
	        }
	    }
	}
}