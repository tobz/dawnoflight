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
using System.Collections.Generic;
using DOL.Database;
using DOL.AI.Brain;

namespace DOL.GS
{
	/// <summary>
	/// LootGeneratorMobTemplate
	/// This implementation uses LootTemplates to relate loots to a specific mob type.
	/// Used DB Tables: 
	///				MobDropTemplate  (Relation between Mob and loottemplate
	///				DropTemplateXItemTemplate	(loottemplate containing possible loot items)
	/// </summary>
	public class LootGeneratorMobTemplate : LootGeneratorBase
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Map holding a list of ItemTemplateIDs for each TemplateName
		/// 1:n mapping between loottemplateName and loottemplate entries
		/// </summary>
		protected static Dictionary<string, List<Tuple<DropTemplateXItemTemplate, ItemTemplate>>> m_lootTemplates;

		/// <summary>
		/// Map holding the corresponding LootTemplateName for each MobName
		/// 1:n Mapping between Mob and LootTemplate
		/// </summary>
		protected static Dictionary<string, List<MobDropTemplate>> m_mobXLootTemplates;

		/// <summary>
		/// Construct a new templategenerate and load its values from database.
		/// </summary>
		public LootGeneratorMobTemplate()
		{
			PreloadLootTemplates();
		}

		public static void ReloadLootTemplates()
		{
			m_lootTemplates = null;
			m_mobXLootTemplates = null;
			PreloadLootTemplates();
		}

		/// <summary>
		/// Loads the DropTemplateXItemTemplate
		/// </summary>
		/// <returns></returns>
		protected static bool PreloadLootTemplates()
		{
			if (m_lootTemplates == null)
			{
				m_lootTemplates = new Dictionary<string, List<Tuple<DropTemplateXItemTemplate, ItemTemplate>>>();

				lock (m_lootTemplates)
				{
					IList<DropTemplateXItemTemplate> dbLootTemplates;

					try
					{
						// TemplateName (typically the mob name), ItemTemplateID, Chance
						dbLootTemplates = GameServer.Database.SelectAllObjects<DropTemplateXItemTemplate>();
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("LootGeneratorMobTemplate: DropTemplateXItemTemplate could not be loaded:", e);
						}
						return false;
					}

					if (dbLootTemplates != null)
					{

						foreach (DropTemplateXItemTemplate dbTemplate in dbLootTemplates)
						{
							if(!m_lootTemplates.ContainsKey(dbTemplate.TemplateName.ToLower()))
							{
								m_lootTemplates[dbTemplate.TemplateName.ToLower()] = new List<Tuple<DropTemplateXItemTemplate, ItemTemplate>>();
							}

							ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(dbTemplate.ItemTemplateID);

							if (drop == null)
							{
								if (log.IsErrorEnabled)
									log.Error("ItemTemplate: " + dbTemplate.ItemTemplateID + " is not found, it is referenced from DropTemplateXItemTemplate(Preload): " + dbTemplate.TemplateName);
							}
							else
							{
								m_lootTemplates[dbTemplate.TemplateName.ToLower()].Add(new Tuple<DropTemplateXItemTemplate, ItemTemplate>(dbTemplate, drop));
							}
						}
					}
				}

				log.Info("DropTemplateXItemTemplates pre-loaded.");
			}

			if (m_mobXLootTemplates == null)
			{
				m_mobXLootTemplates = new Dictionary<string, List<MobDropTemplate>>();

				lock (m_mobXLootTemplates)
				{
					IList<MobDropTemplate> dbMobXLootTemplates;

					try
					{
						// MobName, LootTemplateName, DropCount
						dbMobXLootTemplates = GameServer.Database.SelectAllObjects<MobDropTemplate>();
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("LootGeneratorMobTemplate: MobDropTemplate could not be loaded", e);
						}
						return false;
					}

					if (dbMobXLootTemplates != null)
					{
						foreach (MobDropTemplate dbMobXTemplate in dbMobXLootTemplates)
						{
							// There can be multiple MobDropTemplate for a mob, each pointing to a different loot template
							
							if (!m_mobXLootTemplates.ContainsKey(dbMobXTemplate.MobName.ToLower()))
							{
								m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()] = new List<MobDropTemplate>();
							}
							
							m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()].Add(dbMobXTemplate);
						}
					}
				}

				log.Info("MobDropTemplates pre-loaded.");
			}

			return true;
		}

		/// <summary>
		/// Reload the loot templates for this mob
		/// </summary>
		/// <param name="mob"></param>
		public override void Refresh(GameNPC mob)
		{
			if (mob == null)
				return;

			bool isDefaultLootTemplateRefreshed = false;

			// First see if there are any MobXLootTemplates associated with this mob
			IList<MobDropTemplate> mxlts;

			if(mob.NPCTemplate != null && mob.NPCTemplate.TemplateId > 0)
				mxlts = GameServer.Database.SelectObjects<MobDropTemplate>("MobName = '" + GameServer.Database.Escape(mob.Name) + "' OR MobName = '" + GameServer.Database.Escape(mob.NPCTemplate.TemplateId.ToString()) + "'");
			else
				mxlts = GameServer.Database.SelectObjects<MobDropTemplate>("MobName = '" + GameServer.Database.Escape(mob.Name) + "'");

			if (mxlts != null)
			{
				lock (m_mobXLootTemplates)
				{
					foreach (MobDropTemplate mxlt in mxlts)
						m_mobXLootTemplates.Remove(mxlt.MobName.ToLower());
					
					foreach (MobDropTemplate mxlt in mxlts)
					{
						List<MobDropTemplate> mobxLootTemplates;
						if (!m_mobXLootTemplates.TryGetValue(mxlt.MobName.ToLower(), out mobxLootTemplates))
						{
							mobxLootTemplates = new List<MobDropTemplate>();
							m_mobXLootTemplates[mxlt.MobName.ToLower()] = mobxLootTemplates;
						}
						mobxLootTemplates.Add(mxlt);

						RefreshLootTemplate(mxlt.LootTemplateName);


						if (mxlt.LootTemplateName.ToLower() == mob.Name.ToLower())
							isDefaultLootTemplateRefreshed = true;
					}
				}
			}

			// now force a refresh of the mobs default loot template
			if (isDefaultLootTemplateRefreshed == false)
				RefreshLootTemplate(mob.Name);
		}

		protected void RefreshLootTemplate(string templateName)
		{
			var lootTemplates = GameServer.Database.SelectObjects<DropTemplateXItemTemplate>("TemplateName = '" + GameServer.Database.Escape(templateName) + "'");

			if (lootTemplates != null)
			{
				lock (m_lootTemplates)
				{
					if(m_lootTemplates.ContainsKey(templateName.ToLower()))
						m_lootTemplates.Remove(templateName.ToLower());
						
					m_lootTemplates[templateName.ToLower()] = new List<Tuple<DropTemplateXItemTemplate, ItemTemplate>>();
					
					foreach (DropTemplateXItemTemplate lt in lootTemplates)
					{
						ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(lt.ItemTemplateID);

						if (drop == null)
						{
							if (log.IsErrorEnabled)
								log.Error("ItemTemplate: " + lt.ItemTemplateID + " is not found, it is referenced from DropTemplateXItemTemplate(Refresh): " + templateName);
						}
						else
						{
							m_lootTemplates[templateName.ToLower()].Add(new Tuple<DropTemplateXItemTemplate, ItemTemplate>(lt, drop));
						}
					}
				}
			}
		}

		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);

			try
			{
				GamePlayer player = killer as GamePlayer;
				
				if (killer is GameNPC && ((GameNPC)killer).Brain != null && ((GameNPC)killer).Brain is IControlledBrain)
					player = ((ControlledNpcBrain)((GameNPC)killer).Brain).GetPlayerOwner();
				
				if (player == null)
					return loot;

				// allow the leader to decide the loot realm
				if (player.Group != null)
					player = player.Group.Leader;

				List<MobDropTemplate> killedMobXLootTemplates = new List<MobDropTemplate>();
				// MobDropTemplate contains a loot template name and the max number of drops allowed for that template.
				// We don't need an entry in MobDropTemplate in order to drop loot, only to control the max number of drops.
				if(m_mobXLootTemplates.ContainsKey(mob.Name.ToLower()))
					foreach(MobDropTemplate tpl in m_mobXLootTemplates[mob.Name.ToLower()])
						killedMobXLootTemplates.Add(tpl);
				
				if(mob.NPCTemplate != null && mob.NPCTemplate.TemplateId > 0 && m_mobXLootTemplates.ContainsKey(mob.NPCTemplate.TemplateId.ToString()))
					foreach(MobDropTemplate tpl in m_mobXLootTemplates[mob.NPCTemplate.TemplateId.ToString()])
						killedMobXLootTemplates.Add(tpl);
										
				if (killedMobXLootTemplates.Count > 0)
				{
					// MobDropTemplate exists and tells us the max number of items that can drop.
					// Because we are restricting the max number of items to drop we need to traverse the list
					// and add every 100% chance items to the loots Fixed list and add the rest to the Random list
					// due to the fact that 100% items always drop regardless of the drop limit
					foreach(MobDropTemplate tpl in killedMobXLootTemplates) 
					{
						if(m_lootTemplates.ContainsKey(tpl.LootTemplateName.ToLower()) && m_lootTemplates[tpl.LootTemplateName.ToLower()].Count > 0)
						{
							// Update total dropcount
							loot.DropCount += tpl.DropCount;
							
							foreach (Tuple<DropTemplateXItemTemplate, ItemTemplate> lootlist in m_lootTemplates[tpl.LootTemplateName.ToLower()])
							{
								if (lootlist.Item2.Realm == (int)player.Realm || lootlist.Item2.Realm == 0 || player.CanUseCrossRealmItems)
								{
									if (lootlist.Item1.Chance == 100)
										loot.AddFixed(lootlist.Item2, Math.Max(1, lootlist.Item1.Count));
									else
										loot.AddRandom(lootlist.Item1.Chance, lootlist.Item2, Math.Max(1, lootlist.Item1.Count));
								}
							}
						}
						
					}
				}
				else
				{
					// DropTemplateXItemTemplate contains a template name and an itemtemplateid (id_nb).
					// TemplateName usually equals Mob name, so if you want to know what drops for a mob:
					// select * from DropTemplateXItemTemplate where templatename = 'mob name';
					// It is possible to have TemplateName != MobName but this works only if there is an entry in MobDropTemplate for the MobName.

					if(m_lootTemplates.ContainsKey(mob.Name.ToLower()) && m_lootTemplates[mob.Name.ToLower()].Count > 0)
				  	{
				   		foreach (Tuple<DropTemplateXItemTemplate, ItemTemplate> lootlist in m_lootTemplates[mob.Name.ToLower()])
				   		{
				   			if (lootlist.Item2.Realm == (int)player.Realm || lootlist.Item2.Realm == 0 || player.CanUseCrossRealmItems)
							{
								if (lootlist.Item1.Chance == 100)
								{
									loot.AddFixed(lootlist.Item2, lootlist.Item1.Count);
								}
								else
								{
									loot.DropCount += lootlist.Item1.Count;
									loot.AddRandom(lootlist.Item1.Chance, lootlist.Item2, lootlist.Item1.Count);
								}
							}
				   		}
					}
					
					if(mob.NPCTemplate != null && mob.NPCTemplate.TemplateId > 0 && m_lootTemplates.ContainsKey(mob.NPCTemplate.TemplateId.ToString()) && m_lootTemplates[mob.NPCTemplate.TemplateId.ToString()].Count > 0)
				  	{
				   		foreach (Tuple<DropTemplateXItemTemplate, ItemTemplate> lootlist in m_lootTemplates[mob.NPCTemplate.TemplateId.ToString()])
				   		{
				   			if (lootlist.Item2.Realm == (int)player.Realm || lootlist.Item2.Realm == 0 || player.CanUseCrossRealmItems)
							{
								if (lootlist.Item1.Chance == 100)
								{
									loot.AddFixed(lootlist.Item2, lootlist.Item1.Count);
								}
								else
								{
									loot.DropCount += lootlist.Item1.Count;
									loot.AddRandom(lootlist.Item1.Chance, lootlist.Item2, lootlist.Item1.Count);
								}
							}
				   		}
					}
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Error in LootGeneratorTemplate for mob {0}.  Exception: {1}", mob.Name, ex.Message);
			}
			
			return loot;
		}
	}
}
