using System;
using System.Reflection;

using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.GameEvents;
using DOL.GS.PacketHandler;

using log4net;

/*
 * Utility Scrolls v2.0
 * Etaew - Dawn of Light
 */

namespace DOL.GS.GameEvents
{
	public class UtilityScrollsEvent
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/*
		 * TODO: add loot generator for dropping scrolls
		 * add loot generator for dropping seals
		 * change merchant types in df to seal merchant
		 * hasteners
		 * vault keepers
		 * teleporter
		 * recharger
		 * smith
		 */
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			Spell load;
			load = TrainerSpell; load = MerchantSpell; load = HealerSpell; load = CampfireSpell;
			LootMgr.RegisterLootGenerator(new LootGeneratorUtilityScrolls(), "", "", "", 0);
			log.Info("Utility Scroll System Loaded!");
		}

		#region Spells
		#region Campfire
		protected static Spell m_CampfireSpell;
		public static Spell CampfireSpell
		{
			get
			{
				if (m_CampfireSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.ClientEffect = 0;
					spell.Icon = 14804;
					spell.Duration = 300;
					spell.Description = "The flames of this campfire will heal your mind, body and spirit while out of combat. This spell is cast when the item is used. Cannot be used in RvR.";
					spell.Name = "Comforting Flames";
					spell.Range = 0;
					spell.SpellID = 14804;
					spell.Target = "Self";
					spell.Type = "ComfortingFlames";
					spell.Value = 0;
					m_CampfireSpell = new Spell(spell, 1);
					SkillBase.GetSpellList(GlobalSpellsLines.Item_Effects).Add(m_CampfireSpell);
				}
				return m_CampfireSpell;
			}
		}
		#endregion 
		#region Trainer
		protected static Spell m_trainerSpell;
		public static Spell TrainerSpell
		{
			get
			{
				if (m_trainerSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.ClientEffect = 0;
					spell.Duration = 60;
					spell.Description = "Summons a Trainer, that anyone can use, at your location for " + spell.Duration + " seconds, to use: move to your quickbar and click it.";
					spell.Name = "Trainer Spell";
					spell.Range = 0;
					spell.SpellID = 64000;
					spell.Target = "Self";
					spell.Type = "UtilityNPC";
					spell.Value = TrainerTemplate.TemplateId;
					m_trainerSpell = new Spell(spell, 1);
					SkillBase.GetSpellList(GlobalSpellsLines.Item_Effects).Add(m_trainerSpell);
				}
				return m_trainerSpell;
			}
		}
		#endregion
		#region Merchant
		protected static Spell m_merchantSpell;
		public static Spell MerchantSpell
		{
			get
			{
				if (m_merchantSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.ClientEffect = 0;
					spell.Duration = 60;
					spell.Description = "Summons a Merchant, for selling items to, at your location for " + spell.Duration + " seconds, to use: move to your quickbar and click it.";
					spell.Name = "Merchant Spell";
					spell.Range = 0;
					spell.SpellID = 64001;
					spell.Target = "Self";
					spell.Type = "UtilityNPC";
					spell.Value = MerchantTemplate.TemplateId;
					m_merchantSpell = new Spell(spell, 1);
					SkillBase.GetSpellList(GlobalSpellsLines.Item_Effects).Add(m_merchantSpell);
				}
				return m_merchantSpell;
			}
		}
		#endregion
		#region Healer
		protected static Spell m_healerSpell;
		public static Spell HealerSpell
		{
			get
			{
				if (m_healerSpell == null)
				{
					DBSpell spell = new DBSpell();
					spell.AutoSave = false;
					spell.CastTime = 0;
					spell.ClientEffect = 0;
					spell.Duration = 60;
					spell.Description = "Summons a Healer, which cures rez illness and restores con, to your location for " + spell.Duration + " seconds, to use: move to your quickbar and click it.";
					spell.Name = "Healer Spell";
					spell.Range = 0;
					spell.SpellID = 64002;
					spell.Target = "Self";
					spell.Type = "UtilityNPC";
					spell.Value = HealerTemplate.TemplateId;
					m_healerSpell = new Spell(spell, 1);
					SkillBase.GetSpellList(GlobalSpellsLines.Item_Effects).Add(m_healerSpell);
				}
				return m_healerSpell;
			}
		}
		#endregion
		#endregion

		#region Items
		#region Campfire
		protected static ItemTemplate m_tinderbox;
		public static ItemTemplate Tinderbox
		{
			get
			{
				if (m_tinderbox == null)
				{
					m_tinderbox = new ItemTemplate();
					m_tinderbox.CanDropAsLoot = false;
					m_tinderbox.Charges = 1;
					m_tinderbox.Id_nb = "enchanted_tinderbox";
					m_tinderbox.IsDropable = true;
					m_tinderbox.IsPickable = true;
					m_tinderbox.IsTradable = true;
					m_tinderbox.Item_Type = 41;
					m_tinderbox.Level = 1;
					m_tinderbox.MaxCharges = 1;
					m_tinderbox.Model = 1347;
					m_tinderbox.Name = "Enchanted Tinderbox";
					m_tinderbox.Object_Type = (int)eObjectType.Magical;
					m_tinderbox.Realm = 0;
					m_tinderbox.SpellID = CampfireSpell.ID;
					m_tinderbox.Quality = 99;
				}
				return m_tinderbox;
			}
		}
		#endregion 
		#region Trainer
		protected static ItemTemplate m_trainerScroll;
		public static ItemTemplate TrainerScroll
		{
			get
			{
				if (m_trainerScroll == null)
				{
					m_trainerScroll = new ItemTemplate();
					m_trainerScroll.CanDropAsLoot = false;
					m_trainerScroll.Charges = 1;
					m_trainerScroll.Id_nb = "trainer_scroll";
					m_trainerScroll.IsDropable = true;
					m_trainerScroll.IsPickable = true;
					m_trainerScroll.IsTradable = false;
					m_trainerScroll.Item_Type = 41;
					m_trainerScroll.Level = 1;
					m_trainerScroll.MaxCharges = 1;
					m_trainerScroll.Model = 499;
					m_trainerScroll.Name = "Trainer Scroll";
					m_trainerScroll.Object_Type = (int)eObjectType.Magical;
					m_trainerScroll.Realm = 0;
					m_trainerScroll.SpellID = TrainerSpell.ID;
				}
				return m_trainerScroll;
			}
		}
		#endregion
		#region Merchant
		protected static ItemTemplate m_merchantScroll;
		public static ItemTemplate MerchantScroll
		{
			get
			{
				if (m_merchantScroll == null)
				{
					m_merchantScroll = new ItemTemplate();
					m_merchantScroll.CanDropAsLoot = false;
					m_merchantScroll.Charges = 1;
					m_merchantScroll.Id_nb = "merchant_scroll";
					m_merchantScroll.IsDropable = true;
					m_merchantScroll.IsPickable = true;
					m_merchantScroll.IsTradable = false;
					m_merchantScroll.Item_Type = 41;
					m_merchantScroll.Level = 1;
					m_merchantScroll.MaxCharges = 1;
					m_merchantScroll.Model = 499;
					m_merchantScroll.Name = "Merchant Scroll";
					m_merchantScroll.Object_Type = (int)eObjectType.Magical;
					m_merchantScroll.Realm = 0;
					m_merchantScroll.SpellID = MerchantSpell.ID;
				}
				return m_merchantScroll;
			}
		}
		#endregion
		#region Healer
		protected static ItemTemplate m_healerScroll;
		public static ItemTemplate HealerScroll
		{
			get
			{
				if (m_healerScroll == null)
				{
					m_healerScroll = new ItemTemplate();
					m_healerScroll.CanDropAsLoot = false;
					m_healerScroll.Charges = 1;
					m_healerScroll.Id_nb = "healer_scroll";
					m_healerScroll.IsDropable = true;
					m_healerScroll.IsPickable = true;
					m_healerScroll.IsTradable = false;
					m_healerScroll.Item_Type = 41;
					m_healerScroll.Level = 1;
					m_healerScroll.MaxCharges = 1;
					m_healerScroll.Model = 499;
					m_healerScroll.Name = "Healer Scroll";
					m_healerScroll.Object_Type = (int)eObjectType.Magical;
					m_healerScroll.Realm = 0;
					m_healerScroll.SpellID = HealerSpell.ID;
				}
				return m_healerScroll;
			}
		}
		#endregion
		#endregion

		#region NPC Template
		#region Trainer
		protected static NpcTemplate m_trainerTemplate;
		public static NpcTemplate TrainerTemplate
		{
			get
			{
				if (m_trainerTemplate == null)
				{
					m_trainerTemplate = new NpcTemplate();
					m_trainerTemplate.Flags += (byte)GameNPC.eFlags.PEACE;
					m_trainerTemplate.Flags += (byte)GameNPC.eFlags.TRANSPARENT;
					m_trainerTemplate.GuildName = "Trainer";
					m_trainerTemplate.Name = "Summoned Trainer";
					m_trainerTemplate.ClassType = "DOL.GS.Trainer.GenericTrainer";
					m_trainerTemplate.Model = "50";
					m_trainerTemplate.TemplateId = 600;
					NpcTemplateMgr.AddTemplate(m_trainerTemplate);
				}
				return m_trainerTemplate;
			}
		}
		#endregion
		#region Merchant
		protected static NpcTemplate m_merchantTemplate;
		public static NpcTemplate MerchantTemplate
		{
			get
			{
				if (m_merchantTemplate == null)
				{
					m_merchantTemplate = new NpcTemplate();
					m_merchantTemplate.Flags += (byte)GameNPC.eFlags.PEACE;
					m_merchantTemplate.Flags += (byte)GameNPC.eFlags.TRANSPARENT;
					m_merchantTemplate.GuildName = "Merchant";
					m_merchantTemplate.Name = "Summoned Merchant";
					m_merchantTemplate.ClassType = "DOL.GS.GameMerchant";
					m_merchantTemplate.Model = "50";
					m_merchantTemplate.TemplateId = 601;
					NpcTemplateMgr.AddTemplate(m_merchantTemplate);
				}
				return m_merchantTemplate;
			}
		}
		#endregion
		#region Healer
		protected static NpcTemplate m_healerTemplate;
		public static NpcTemplate HealerTemplate
		{
			get
			{
				if (m_healerTemplate == null)
				{
					m_healerTemplate = new NpcTemplate();
					m_healerTemplate.Flags += (byte)GameNPC.eFlags.PEACE;
					m_healerTemplate.Flags += (byte)GameNPC.eFlags.TRANSPARENT;
					m_healerTemplate.GuildName = "Healer";
					m_healerTemplate.Name = "Summoned Healer";
					m_healerTemplate.ClassType = "DOL.GS.Scripts.GameHealer";
					m_healerTemplate.Model = "50";
					m_healerTemplate.TemplateId = 602;
					NpcTemplateMgr.AddTemplate(m_healerTemplate);
				}
				return m_healerTemplate;
			}
		}
		#endregion
		#endregion
	}
}

namespace DOL.GS
{
	public class LootGeneratorUtilityScrolls : LootGeneratorBase
	{
		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList list = base.GenerateLoot(mob, killer); 
			#region Campfire
            if (Util.Chance(2))
                list.AddFixed(UtilityScrollsEvent.Tinderbox);
            #endregion
			#region Trainer
			//if (Util.Chance(Math.Max(1, (int)(100 / Math.Max(1, (GameServer.ServerRules.GetExperienceForLevel(killer.Level) / mob.ExperienceValue)) / 4))))
			if (Util.Chance(1))
				list.AddFixed(UtilityScrollsEvent.TrainerScroll);
			#endregion
			#region Merchant
			if (Util.Chance(2))
				list.AddFixed(UtilityScrollsEvent.MerchantScroll);
			#endregion
			#region Healer
			if (Util.Chance((int)Math.Max(1, killer.GetConLevel(mob) + 1 / 2)))
				list.AddFixed(UtilityScrollsEvent.HealerScroll);
			#endregion
			return list;
		}
	}
}

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("UtilityNPC")]
	public class UtilityNPCSpellHandler : SpellHandler
	{
		public UtilityNPCSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{ }

		protected GameNPC npc = null;

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);

			NpcTemplate template = NpcTemplateMgr.GetTemplate((int)m_spell.Value);

			if (template.ClassType == "")
				npc = new GameNPC();
			else
			{
				try
				{
					npc = (GameNPC)Assembly.GetAssembly(typeof(GameServer)).CreateInstance(template.ClassType, false);
				}
				catch (Exception e)
				{
					log.Error(e);
				}
				if (npc == null)
				{
					try
					{
						npc = (GameNPC)Assembly.GetExecutingAssembly().CreateInstance(template.ClassType, false);
					}
					catch (Exception e)
					{
						log.Error(e);
					}
				}
				if (npc == null)
				{
					MessageToCaster("There was an error creating an instance of " + template.ClassType + "!", DOL.GS.PacketHandler.eChatType.CT_System);
					return;
				}
				npc.LoadTemplate(template);
			}
			GameSpellEffect effect = CreateSpellEffect(npc, effectiveness);
			int x, y;
			m_caster.GetSpotFromHeading(64, out x, out y);
			npc.X = x;
			npc.Y = y;
			npc.Z = m_caster.Z;
			npc.CurrentRegion = m_caster.CurrentRegion;
			npc.Heading = (ushort)((m_caster.Heading + 2048) % 4096);
			npc.Realm = m_caster.Realm;
			npc.CurrentSpeed = 0;
			npc.Level = 1;
			npc.SetOwnBrain(new AI.Brain.BlankBrain());
			npc.AddToWorld();
			effect.Start(npc);
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (npc != null)
				npc.Delete();
			return base.OnEffectExpires(effect, noMessages);
		}

		public override bool IsOverwritable(GameSpellEffect compare)
		{
			return false;
		}
	}

	[SpellHandlerAttribute("ComfortingFlames")]
	public class CampfireSpellHandler : DoTSpellHandler
	{
		private GameObject m_campfire;

		public CampfireSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{ }

		public override void OnEffectPulse(GameSpellEffect effect)
		{
			if (m_campfire == null) return;

			foreach (GamePlayer player in m_campfire.GetPlayersInRadius(500))
			{
				if (player.IsAlive == false) continue;

				if ((GameServer.ServerRules.IsSameRealm(Caster, player, true)) && (player.InCombat == false))
				{
					int mr = player.MaxMana / 20;
					int hr = player.MaxHealth / 20;
					int er = player.MaxEndurance / 20;

					// Don't stack
					int stack = 0;
					foreach (GameObject obj in player.GetItemsInRadius(500))
					{
						if (obj.Model == 3460) stack++;
					}

					if (stack > 1)
					{
						// Divide the regs by the number of campfires so that they heal with double frequency and half amount
						mr = mr / stack;
						hr = hr / stack;
						er = er / stack;
					}

					if (mr > (player.MaxMana - player.Mana)) mr = player.MaxMana - player.Mana;
					if (hr > (player.MaxHealth - player.Health)) hr = player.MaxHealth - player.Health;
					if (er > (player.MaxEndurance - player.Endurance)) er = player.MaxEndurance - player.Endurance;

					if (hr > 0)
					{
						player.Health += hr;
						player.Out.SendMessage("You regain " + hr.ToString() + " hit points from the campfire!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
					if (mr > 0)
					{
						player.Mana += mr;
						player.Out.SendMessage("You regain " + mr.ToString() + " power from the campfire!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
					if (er > 0)
					{
						player.Endurance += mr;
						player.Out.SendMessage("You regain " + er.ToString() + " endurance from the campfire!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			if (Caster.CurrentRegion.IsRvR == true)
			{
				if (Caster is GamePlayer)
				{
					GamePlayer player = Caster as GamePlayer;
					player.Out.SendMessage("After a short blaze up the flames expire. This spell does not work in RvR areas.", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
				return;
			}
			m_campfire = new GameStaticItem();
			m_campfire.X = Caster.X;
			m_campfire.Y = Caster.Y;
			m_campfire.Z = Caster.Z;
			m_campfire.Heading = Caster.Heading;
			m_campfire.CurrentRegionID = Caster.CurrentRegionID;
			m_campfire.Realm = Caster.Realm;
			m_campfire.Model = 3460;
			m_campfire.Name = "Campfire";
			m_campfire.AddToWorld();
		}

		public override bool IsOverwritable(GameSpellEffect compare)
		{
			return false;
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			if (m_campfire == null) return 0;
			m_campfire.Delete();
			return 0;
		}
	}
}

namespace DOL.GS.Scripts
{
	[CmdAttribute("&scroll", //command to handle
		2, //minimum privelege level
		"creates a test scroll.", //command description
		"/scroll")] //usage
	public class ScrollCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, new InventoryItem(UtilityScrollsEvent.TrainerScroll));
			client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, new InventoryItem(UtilityScrollsEvent.MerchantScroll));
			client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, new InventoryItem(UtilityScrollsEvent.HealerScroll));
			client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, new InventoryItem(UtilityScrollsEvent.Tinderbox));
			return 1;
		}
	}
}

namespace DOL.GS.Trainer
{
	public class GenericTrainer : GameTrainer
	{
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			player.Out.SendTrainerWindow();

			return true;
		}
	}
}