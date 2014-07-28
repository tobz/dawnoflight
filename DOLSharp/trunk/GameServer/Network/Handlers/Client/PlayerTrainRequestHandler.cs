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
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects.CustomNPC;
using DawnOfLight.GameServer.RealmAbilities.handlers;
using DawnOfLight.GameServer.Utilities;
using log4net;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
	/// <summary>
	/// handles Train clicks from Trainer Window
	/// D4 is up to 1.104
	/// </summary>
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerTrainRequest, ClientStatus.PlayerInGame)]
	public class PlayerTrainRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			var trainer = client.Player.TargetObject as GameTrainer;
			if (trainer == null || (trainer.CanTrain(client.Player) == false && trainer.CanTrainChampionLevels(client.Player) == false))
			{
				client.Out.SendMessage("You must select a valid trainer for your class.", ChatType.CT_Important, ChatLocation.CL_ChatWindow);
				return;
			}

			uint x = packet.ReadInt();
			uint y = packet.ReadInt();
			int idLine = packet.ReadByte();
			int unk = packet.ReadByte();
			int row = packet.ReadByte();
			int skillIndex = packet.ReadByte();

			// idline not null so this is a Champion level training window
			if (idLine > 0)
			{
				if (row > 0 && skillIndex > 0)
				{
					ChampSpec spec = ChampSpecMgr.GetAbilityFromIndex(idLine, row, skillIndex);
					if (spec != null)
					{
						if (client.Player.HasChampionSpell(spec.SpellID))
						{
							client.Out.SendMessage("You already have that ability!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							return;
						}
						if (!client.Player.CanTrainChampionSpell(idLine, row, skillIndex))
						{
							client.Out.SendMessage("You do not meet the requirements for that ability!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							return;
						}
						if ((client.Player.ChampionSpecialtyPoints - spec.Cost) < 0)
						{
							client.Out.SendMessage("You do not have enough champion specialty points for that ability!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							return;
						}

						client.Player.ChampionSpecialtyPoints -= spec.Cost;
						SpellLine championPlayerSpellLine = client.Player.GetChampionSpellLine();

						if (championPlayerSpellLine != null)
						{
							SkillBase.AddSpellToSpellLine(client.Player.ChampionSpellLineName, spec.SpellID);
							client.Player.ChampionSpells += spec.SpellID.ToString() + "|1;";
							client.Player.UpdateSpellLineLevels(false);
							client.Player.RefreshSpecDependantSkills(true);
							client.Out.SendMessage("You gain a Champion ability!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							client.Out.SendChampionTrainerWindow(idLine);
							client.Out.SendUpdatePlayerSkills();
						}
						else
						{
							client.Out.SendMessage("Could not find Champion Spell Line!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							log.ErrorFormat("Could not find Champion Spell Line for player {0}", client.Player.Name);
						}
					}
					else
					{
						client.Out.SendMessage("Could not find Champion Spec!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
						log.ErrorFormat("Could not find Champion Spec idline {0}, row {1}, skillindex {2}", idLine, row, skillIndex);
					}
				}
			}
			else
			{
				IList speclist = client.Player.GetSpecList();

				if (skillIndex < speclist.Count)
				{
					Specialization spec = (Specialization)speclist[skillIndex];
					if (spec.Level >= client.Player.BaseLevel)
					{
						client.Out.SendMessage("You can't train in this specialization again this level!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
						return;
					}

					// Graveen - autotrain 1.87 - allow players to train their AT specs even if no pts left
					client.Player.SkillSpecialtyPoints += client.Player.GetAutoTrainPoints(spec, 2);

					if (client.Player.SkillSpecialtyPoints >= spec.Level + 1)
					{
						client.Player.SkillSpecialtyPoints -= (ushort)(spec.Level + 1);
						spec.Level++;
						client.Player.OnSkillTrained(spec);

						client.Out.SendUpdatePoints();
						client.Out.SendTrainerWindow();
						return;
					}
					else
					{
						client.Out.SendMessage("That specialization costs " + (spec.Level + 1) + " specialization points!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
						client.Out.SendMessage("You don't have that many specialization points left for this level.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
						return;
					}
				}
				else if (skillIndex >= 100)
				{
					IList offeredRA = (IList)client.Player.TempProperties.getProperty<object>("OFFERED_RA", null);
					if (offeredRA != null && skillIndex < offeredRA.Count + 100)
					{
						RealmAbility ra = (RealmAbility)offeredRA[skillIndex - 100];
						int cost = ra.CostForUpgrade(ra.Level - 1);
						if (client.Player.RealmSpecialtyPoints < cost)
						{
							client.Out.SendMessage(ra.Name + " costs " + (cost) + " realm ability points!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							client.Out.SendMessage("You don't have that many realm ability points left to get this.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							return;
						}
						if (!ra.CheckRequirement(client.Player))
						{
							client.Out.SendMessage("You are not experienced enough to get " + ra.Name + " now. Come back later.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							return;
						}
						// get a copy of the ability since we use prototypes
						RealmAbility ability = SkillBase.GetAbility(ra.KeyName, ra.Level) as RealmAbility;
						if (ability != null)
						{
							client.Player.RealmSpecialtyPoints -= cost;
							client.Player.AddAbility(ability);
							client.Out.SendUpdatePoints();
							client.Out.SendUpdatePlayer();
							client.Out.SendUpdatePlayerSkills();
							client.Out.SendTrainerWindow();
						}
						else
						{
							client.Out.SendMessage("Unfortunately your training failed. Please report that to admins or game master. Thank you.", ChatType.CT_System, ChatLocation.CL_SystemWindow);
							log.Error("Realm Ability " + ra.Name + "(" + ra.KeyName + ") unexpected not found");
						}
						return;
					}
				}

				if (log.IsErrorEnabled)
					log.Error("Player <" + client.Player.Name + "> requested to train incorrect skill index");
			}
		}
	}
}
