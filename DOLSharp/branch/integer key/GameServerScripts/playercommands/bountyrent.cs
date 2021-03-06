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
using System;
using DOL.GS.PacketHandler;
using DOL.GS.Housing;
using DOL.Language;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&bountyrent", //command to handle
		(uint)ePrivLevel.Player, //minimum privelege level
		"Pay house rent with bountypoints", //command description
		"/bountyrent <personal/guild> <amount>")] //command usage
	public class BountyRentCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return 0;
			}
			House house = HouseMgr.GetHouseByPlayer(client.Player);
			if (house == null)
			{
				DisplayError(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.NoHouseError"), new object[] { });
				return 0;
			}
			if (!client.Player.InHouse || client.Player.CurrentHouse != house)
			{
				DisplayError(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Bountyrent.InHouseError"), new object[] { });
				return 0;
			}
			switch (args[1].ToLower())
			{
				case "personal":
					{
						long BPsToAdd = 0;
						try
						{
							BPsToAdd = Int64.Parse(args[2]);
						}
						catch
						{
							return 0;
						}
						if ((client.Player.BountyPoints -= BPsToAdd) < 0)
						{
							//  DisplayError(client, "You do not have enough bps!", new object[] { });
							return 0;
						}
						BPsToAdd *= 10000;
						client.Player.TempProperties.setProperty(House.BPSFORHOUSERENT, BPsToAdd);
						client.Player.TempProperties.setProperty(House.HOUSEFORHOUSERENT, house);
						client.Player.Out.SendHousePayRentDialog("Housing07");
						break;
					}
				case "guild":
					{
						DisplayMessage(client, "This feature is not yet implemented!", new object[] { });
						break;
					}
				default:
					{
						DisplaySyntax(client);
						break;
					}
			}
			return 1;
		}
	}
}