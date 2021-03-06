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
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x62^168,"Handles player bonuses button clicks")]
	public class PlayerBonusesListRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int code = packet.ReadByte();
			if (code != 0)
			{
				if (log.IsWarnEnabled)
					log.Warn("bonuses button: code is other than zero ("+code+")");
			}

			/*
			<Begin Info: Bonuses (snapshot)>
			Resistances
			 Crush: +25%/+0%
			 Slash: +28%/+0%
			 Thrust: +28%/+0%
			 Heat: +25%/+0%
			 Cold: +25%/+0%
			 Matter: +26%/+0%
			 Body: +31%/+0%
			 Spirit: +21%/+0%
			 Energy: +26%/+0%
 
			Special Item Bonuses
			 +20% to Power Pool
			 +3% to all Melee Damage
			 +6% to all Stat Buff Spells
			 +23% to all Heal Spells
			 +3% to Melee Combat Speed
			 +10% to Casting Speed
 
			Realm Rank Bonuses
			 +7 to ALL Specs
 
			Relic Bonuses
			 +20% to all Melee Damage
 
			Outpost Bonuses
			 none
 
			<End Info>
			*/

			ArrayList info = new ArrayList();
			info.Add("Resistances");
			info.Add(string.Format(" Crush: {0:+0;-0}%/{1:+0;-0}%",  client.Player.GetModified(eProperty.Resist_Crush),  0));
			info.Add(string.Format(" Slash: {0:+0;-0}%/{1:+0;-0}%",  client.Player.GetModified(eProperty.Resist_Slash),  0));
			info.Add(string.Format(" Thrust: {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Thrust), 0));
			info.Add(string.Format(" Heat: {0:+0;-0}%/{1:+0;-0}%",   client.Player.GetModified(eProperty.Resist_Heat),   0));
			info.Add(string.Format(" Cold: {0:+0;-0}%/{1:+0;-0}%",   client.Player.GetModified(eProperty.Resist_Cold),   0));
			info.Add(string.Format(" Matter: {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Matter), 0));
			info.Add(string.Format(" Body: {0:+0;-0}%/{1:+0;-0}%",   client.Player.GetModified(eProperty.Resist_Body),   0));
			info.Add(string.Format(" Spirit: {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Spirit), 0));
			info.Add(string.Format(" Energy: {0:+0;-0}%/{1:+0;-0}%", client.Player.GetModified(eProperty.Resist_Energy), 0));
			info.Add(" ");
			info.Add("Special Item Bonuses");
			info.Add(" none");
			info.Add(" ");
			info.Add("Realm Rank Bonuses");
			if (client.Player.RealmLevel>10)
				info.Add(string.Format(" +{0} to ALL Specs", client.Player.RealmLevel/10));
			else
				info.Add(" none");
			info.Add(" ");
			info.Add("Relic Bonuses");
			info.Add(" none");
			info.Add(" ");
			info.Add("Outpost Bonuses");
			info.Add(string.Format("{0:+0;-0}%", client.Player.GetKeepBonuses()*100));
			client.Out.SendCustomTextWindow("Bonuses (snapshot)", info);

			return 1;
		}
	}
}
