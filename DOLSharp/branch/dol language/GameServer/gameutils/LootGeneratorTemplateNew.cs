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
/*
 * Credits go to: 
 * - Echostorm's Mob Drop Loot System
 * - Roach's modifications to add loottemplate base mobdrops  
 */
using System;
using System.Collections.Generic;
using DOL.Database;
using DOL.AI.Brain;

namespace DOL.GS
{
	/// <summary>
	/// TemplateLootGenerator
	/// This implementation uses LootTemplates to relate loots to a specific mob type.
	/// Used DB Tables: 
	///				MobxLootTemplate  (Relation between Mob and loottemplate
	///				LootTemplate	(loottemplate containing possible loot items)
	/// </summary>
	public class LootGeneratorTemplateNew : LootGeneratorBase
	{
		/// <summary>
		/// Map holding a list of ItemTemplateIDs for each TemplateName
		/// 1:n mapping between loottemplateName and loottemplate entries
		/// </summary>
		protected static Dictionary<string, Dictionary<string, LootTemplate>> m_lootTemplates;

		/// <summary>
		/// Map holding the corresponding LootTemplateName for each MobName
		/// 1:n Mapping between Mob and LootTemplate
		/// </summary>
		protected static Dictionary<string, List<MobXLootTemplateNew>> m_mobXLootTemplates;

		/// <summary>
		/// Construct a new templategenerate and load its values from database.
		/// </summary>
		public LootGeneratorTemplateNew()
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
		/// Loads the loottemplates
		/// </summary>
		/// <returns></returns>
		protected static bool PreloadLootTemplates()
		{
			if (m_lootTemplates == null)
			{
				m_lootTemplates = new Dictionary<string, Dictionary<string, LootTemplate>>();

				lock (m_lootTemplates)
				{
					IList<LootTemplate> dbLootTemplates;

					try
					{
						// TemplateName (typically the mob name + "_new"), ItemTemplateID, Chance
						dbLootTemplates = GameServer.Database.SelectAllObjects<LootTemplate>();
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("LootGeneratorTemplate: LootTemplates could not be loaded:", e);
						}
						return false;
					}

					if (dbLootTemplates != null)
					{
						Dictionary<string, LootTemplate> loot;

						foreach (LootTemplate dbTemplate in dbLootTemplates)
						{
							if (!m_lootTemplates.TryGetValue(dbTemplate.TemplateName.ToLower(), out loot))
							{
								loot = new Dictionary<string, LootTemplate>();
								m_lootTemplates[dbTemplate.TemplateName.ToLower()] = loot;
							}

							ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(dbTemplate.ItemTemplateID);

							if (drop == null)
							{
								if (log.IsErrorEnabled)
									log.Error("ItemTemplate: " + dbTemplate.ItemTemplateID + " is not found, it is referenced from loottemplate: " + dbTemplate.TemplateName);
							}
							else
							{
								if (!loot.ContainsKey(dbTemplate.ItemTemplateID.ToLower()))
									loot.Add(dbTemplate.ItemTemplateID.ToLower(), dbTemplate);
							}
						}
					}
				}

				log.Info("LootTemplates pre-loaded.");
			}

			if (m_mobXLootTemplates == null)
			{
				m_mobXLootTemplates = new Dictionary<string, List<MobXLootTemplateNew>>();

				lock (m_mobXLootTemplates)
				{
					IList<MobXLootTemplateNew> dbMobXLootTemplates;

					try
					{
						// MobName, LootTemplateName, DropCount
						dbMobXLootTemplates = GameServer.Database.SelectAllObjects<MobXLootTemplateNew>();
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
						{
							log.Error("LootGeneratorTemplate: MobXLootTemplates could not be loaded", e);
						}
						return false;
					}

					if (dbMobXLootTemplates != null)
					{
						foreach (MobXLootTemplateNew dbMobXTemplate in dbMobXLootTemplates)
						{
							// There can be multiple MobXLootTemplates for a mob, each pointing to a different loot template
							List<MobXLootTemplateNew> mobxLootTemplates;
							if (!m_mobXLootTemplates.TryGetValue(dbMobXTemplate.MobName.ToLower(), out mobxLootTemplates))
							{
								mobxLootTemplates = new List<MobXLootTemplateNew>();
								m_mobXLootTemplates[dbMobXTemplate.MobName.ToLower()] = mobxLootTemplates;
							}
							mobxLootTemplates.Add(dbMobXTemplate);
						}
					}
				}

				log.Info("MobXLootTemplates pre-loaded.");
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

			IList<MobXLootTemplateNew> mxlts = GameServer.Database.SelectObjects<MobXLootTemplateNew>("MobName = '" + GameServer.Database.Escape(mob.Name) + "'");

			if (mxlts != null)
			{
				lock (m_mobXLootTemplates)
				{
					foreach (MobXLootTemplateNew mxlt in mxlts)
					{
						List<MobXLootTemplateNew> mobxLootTemplates;
						if (!m_mobXLootTemplates.TryGetValue(mxlt.MobName.ToLower(), out mobxLootTemplates))
						{
							mobxLootTemplates = new List<MobXLootTemplateNew>();
							m_mobXLootTemplates[mxlt.MobName.ToLower()] = mobxLootTemplates;
						}
						mobxLootTemplates.Add(mxlt);

						RefreshLootTemplate(mxlt.LootTemplateName);


						if (mxlt.LootTemplateName.ToLower() + "_new" == mob.Name.ToLower() + "_new")
							isDefaultLootTemplateRefreshed = true;
					}
				}
			}

			// now force a refresh of the mobs default loot template

			if (isDefaultLootTemplateRefreshed == false)
				RefreshLootTemplate(mob.Name + "_new");
		}

		protected void RefreshLootTemplate(string templateName)
		{
			lock (m_lootTemplates)
			{
				if (m_lootTemplates.ContainsKey(templateName.ToLower()))
				{
					m_lootTemplates.Remove(templateName.ToLower());
				}
			}

			IList<LootTemplate> lootTemplates = GameServer.Database.SelectObjects<LootTemplate>("TemplateName = '" + GameServer.Database.Escape(templateName) + "'");

			if (lootTemplates != null)
			{
				lock (m_lootTemplates)
				{
					if (m_lootTemplates.ContainsKey(templateName.ToLower()))
					{
						m_lootTemplates.Remove(templateName.ToLower());
					}

					Dictionary<string, LootTemplate> lootList = new Dictionary<string, LootTemplate>();

					foreach (LootTemplate lt in lootTemplates)
					{
						if (lootList.ContainsKey(lt.ItemTemplateID.ToLower()) == false)
						{
							lootList.Add(lt.ItemTemplateID.ToLower(), lt);
						}
					}

					m_lootTemplates.Add(templateName.ToLower(), lootList);
				}
			}
		}

		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);

			try
			{
				GamePlayer player = killer as GamePlayer;
				if (killer is GameNPC && ((GameNPC)killer).Brain is IControlledBrain)
					player = ((ControlledNpcBrain)((GameNPC)killer).Brain).GetPlayerOwner();
				if (player == null)
					return loot;

				// allow the leader to decide the loot realm
				if (player.Group != null)
					player = player.Group.Leader;

				List<MobXLootTemplateNew> killedMobXLootTemplates;
				// MobXLootTemplate contains a loot template name and the max number of drops allowed for that template.
				// We don't need an entry in MobXLootTemplate in order to drop loot, only to control the max number of drops.

				// LootTemplate contains a template name and an itemtemplateid (id_nb).
				// TemplateName usually equals Mob name, so if you want to know what drops for a mob:
				// select * from LootTemplate where templatename = CONCAT('mob name', '_new');
				// It is possible to have TemplateName != MobName + "_new" but this works only if there is an entry in MobXLootTemplate for the MobName.
				if (!m_mobXLootTemplates.TryGetValue(mob.Name.ToLower(), out killedMobXLootTemplates))
				{
					Dictionary<string, LootTemplate> lootTemplatesToDrop;
					// We can use LootTemplate.Count to determine how many of a item can drop
					if (m_lootTemplates.TryGetValue(mob.Name.ToLower() + "_new", out lootTemplatesToDrop))
					{
						foreach (LootTemplate lootTemplate in lootTemplatesToDrop.Values)
						{
							ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

							if (drop.Realm == (int)player.Realm || drop.Realm == 0 || player.CanUseCrossRealmItems)
							{
								if (lootTemplate.Chance == 100)
									loot.AddFixed(drop, lootTemplate.Count);
								else
									loot.AddRandom(lootTemplate.Chance, drop, lootTemplate.Count);
							}
						}
					}
				}
				else
				{
					// MobXLootTemplate exists and tells us the max number of items that can drop.
					// Because we are restricting the max number of items to drop we need to traverse the list
					// and add every 100% chance items to the loots Fixed list and add the rest to the Random list
					// due to the fact that 100% items always drop regardless of the drop limit

					List<LootTemplate> lootTemplatesToDrop = new List<LootTemplate>();
					foreach (MobXLootTemplateNew mobXLootTemplate in killedMobXLootTemplates)
					{
						loot = GenerateLootFromMobXLootTemplates(mobXLootTemplate, lootTemplatesToDrop, loot, player);

						foreach (LootTemplate lootTemplate in lootTemplatesToDrop)
						{
							ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

							if (drop.Realm == (int)player.Realm || drop.Realm == 0 || player.CanUseCrossRealmItems)
								loot.AddRandom(lootTemplate.Chance, drop, lootTemplate.Count);
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

		/// <summary>
		/// Add all loot templates specified in MobXLootTemplate for an entry in LootTemplates
		/// If the item has a 100% drop chance add it as a fixed drop to the loot list.  
		/// </summary>
		/// <param name="mobXLootTemplate">Entry in MobXLootTemplate.</param>
		/// <param name="lootTemplates">List of all itemtemplates this mob can drop and the chance to drop</param>
		/// <param name="lootList">List to hold loot.</param>
		/// <param name="player">Player used to determine realm</param>
		/// <returns>lootList (for readability)</returns>
		private static LootList GenerateLootFromMobXLootTemplates(MobXLootTemplateNew mobXLootTemplates, List<LootTemplate> lootTemplates, LootList lootList, GamePlayer player)
		{
			if (mobXLootTemplates == null || lootTemplates == null || player == null)
				return lootList;

			Dictionary<string, LootTemplate> templateList = null;
			if (m_lootTemplates.ContainsKey(mobXLootTemplates.LootTemplateName.ToLower()))
				templateList = m_lootTemplates[mobXLootTemplates.LootTemplateName.ToLower()];

			if (templateList != null)
			{
				foreach (LootTemplate lootTemplate in templateList.Values)
				{
					ItemTemplate drop = GameServer.Database.FindObjectByKey<ItemTemplate>(lootTemplate.ItemTemplateID);

					if (drop.Realm == (int)player.Realm || drop.Realm == 0 || player.CanUseCrossRealmItems)
					{
						if (lootTemplate.Chance == 100)
							lootList.AddFixed(drop, lootTemplate.Count);
						else
							lootTemplates.Add(lootTemplate);
					}
				}
			}

			return lootList;
		}

	}
}
