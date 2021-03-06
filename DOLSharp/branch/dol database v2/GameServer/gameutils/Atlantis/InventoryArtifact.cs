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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DOL.GS;
using log4net;
using System.Reflection;
using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// An artifact inside an inventory.
	/// </summary>
	/// <author>Aredhel</author>
	public class InventoryArtifact : InventoryItem
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private String m_artifactID;
		private int m_artifactLevel;
		private int[] m_levelRequirements;

		/// <summary>
		/// This constructor shouldn't be called, so we prevent anyone
		/// from using it.
		/// </summary>
		private InventoryArtifact() { }

		/// <summary>
		/// Create a new inventory artifact from an item template.
		/// </summary>
		/// <param name="template"></param>
		public InventoryArtifact(ItemTemplate template)
			: base(template)
		{
			ArtifactID = ArtifactMgr.GetArtifactIDFromItemID(template.Id_nb);
			ArtifactLevel = 0;
			m_levelRequirements = ArtifactMgr.GetLevelRequirements(ArtifactID);

			for (ArtifactBonus.TypeID bonusID = ArtifactBonus.TypeID.Min; bonusID <= ArtifactBonus.TypeID.Max; ++bonusID)
			{
				// Clear all bonuses except the base (L0) bonuses.

				if (m_levelRequirements[(int)bonusID] > 0)
				{
					SetBonusType(bonusID, 0);
					SetBonusAmount(bonusID, 0);
				}
			}
		}

		/// <summary>
		/// Create a new inventory artifact from an existing inventory item.
		/// </summary>
		/// <param name="item"></param>
		public InventoryArtifact(InventoryItem item)
			: base(item)
		{
			if (item != null)
			{
				ArtifactID = ArtifactMgr.GetArtifactIDFromItemID(Id_nb);
				ArtifactLevel = ArtifactMgr.GetCurrentLevel(this);
				m_levelRequirements = ArtifactMgr.GetLevelRequirements(ArtifactID);
			}
		}

		/// <summary>
		/// The ID of this artifact.
		/// </summary>
		public String ArtifactID
		{
			get { return m_artifactID; }
			protected set { m_artifactID = value; }
		}

		/// <summary>
		/// The level of this artifact.
		/// </summary>
		public int ArtifactLevel
		{
			get { return m_artifactLevel; }
			protected set { m_artifactLevel = value; }
		}

		/// <summary>
		/// Called from ArtifactMgr when this artifact has gained a level.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="artifactLevel"></param>
		public void OnLevelGained(GamePlayer player, int artifactLevel)
		{
			if (artifactLevel > ArtifactLevel && artifactLevel <= 10)
			{
				ArtifactLevel = artifactLevel;
				if (AddAbilities(player, ArtifactLevel) && player != null)
					player.Out.SendMessage(String.Format("Your {0} has gained a new ability!", Name),
						eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Add all abilities for this level.
		/// </summary>
		/// <param name="artifactLevel">The level to add abilities for.</param>
		/// <param name="player"></param>
		/// <returns>True, if artifact gained 1 or more abilities, else false.</returns>
		private bool AddAbilities(GamePlayer player, int artifactLevel)
		{
			ItemTemplate template = (ItemTemplate)GameServer.Database.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), 
				Id_nb);

			if (template == null)
			{
				log.Warn(String.Format("Item template missing for artifact '{0}'", Name));
				return false;
			}

			bool abilityGained = false;

			for (ArtifactBonus.TypeID bonusID = ArtifactBonus.TypeID.Min; bonusID <= ArtifactBonus.TypeID.Max; ++bonusID)
			{
				if (m_levelRequirements[(int)bonusID] == artifactLevel)
				{
					SetBonusType(bonusID, template.GetBonusType(bonusID));
					SetBonusAmount(bonusID, template.GetBonusAmount(bonusID));

					if (bonusID <= ArtifactBonus.TypeID.MaxStat)
						player.Notify(PlayerInventoryEvent.ItemBonusChanged, this,
							new ItemBonusChangedEventArgs(GetBonusType(bonusID), GetBonusAmount(bonusID)));

					abilityGained = true;
				}
			}

			return abilityGained;
		}

		#region Delve

		/// <summary>
		/// Artifact delve information.
		/// </summary>
		public virtual void Delve(List<String> delve, GamePlayer player)
		{
			if (player == null)
				return;

			// Artifact specific information.

			if (ArtifactLevel < 10)
			{
				delve.Add(string.Format("Artifact (Current level: {0})", ArtifactLevel));
				delve.Add(string.Format("- {0}% exp earned towards level {1}",
					ArtifactMgr.GetXPGainedForLevel(this), ArtifactLevel + 1));
				delve.Add(string.Format("- Artifact will gain new abilities at level {0}",
					GainsNewAbilityAtLevel()));
				delve.Add(string.Format("(Earns exp: {0})",
					ArtifactMgr.GetEarnsXP(this)));
			}
			else
			{
				delve.Add("Artifact:");
				delve.Add("Current level: 10");
			}

			// Item bonuses.

			delve.Add("");
			delve.Add("Magical Bonuses:");

			for (ArtifactBonus.TypeID bonusID = ArtifactBonus.TypeID.MinStat; bonusID <= ArtifactBonus.TypeID.MaxStat; ++bonusID)
				DelveMagicalBonus(delve, GetBonusAmount(bonusID), GetBonusType(bonusID),
					m_levelRequirements[(int)bonusID]);

			delve.Add("");

			for (ArtifactBonus.TypeID bonusID = ArtifactBonus.TypeID.MinStat; bonusID <= ArtifactBonus.TypeID.MaxStat; ++bonusID)
				DelveBonus(delve, GetBonusAmount(bonusID), GetBonusType(bonusID),
					m_levelRequirements[(int)bonusID]);

			// Spells & Procs

			DelveMagicalAbility(delve, ArtifactBonus.TypeID.Spell, m_levelRequirements[(int)ArtifactBonus.TypeID.Spell]);
			DelveMagicalAbility(delve, ArtifactBonus.TypeID.ProcSpell, m_levelRequirements[(int)ArtifactBonus.TypeID.ProcSpell]);
			DelveMagicalAbility(delve, ArtifactBonus.TypeID.Spell1, m_levelRequirements[(int)ArtifactBonus.TypeID.Spell1]);
			DelveMagicalAbility(delve, ArtifactBonus.TypeID.ProcSpell1, m_levelRequirements[(int)ArtifactBonus.TypeID.ProcSpell1]);

			delve.Add("");

			// Weapon & Armor Stats.

			if ((Object_Type >= (int)eObjectType.GenericWeapon) &&
				(Object_Type <= (int)eObjectType.MaulerStaff) ||
				(Object_Type == (int)eObjectType.Instrument))
				DelveWeaponStats(delve, player);
			else if (Object_Type >= (int)eObjectType.Cloth &&
				Object_Type <= (int)eObjectType.Scale)
				DelveArmorStats(delve, player);

			// Reuse Timer

			int reuseTimer = ArtifactMgr.GetReuseTimer(this);
			
			if (reuseTimer > 0)
			{
				TimeSpan reuseTimeSpan = new TimeSpan(0, 0, reuseTimer);
				delve.Add("");
				delve.Add(String.Format("Can use item every {0} min", 
					reuseTimeSpan.ToString().Substring(3)));
			}
		}

		/// <summary>
		/// Artifact classic magical bonus delve information.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="bonusAmount"></param>
		/// <param name="bonusType"></param>
		/// <param name="levelRequirement"></param>
		protected virtual void DelveMagicalBonus(List<String> delve, int bonusAmount, int bonusType,
			int levelRequirement)
		{
			String levelTag = (levelRequirement > 0) 
				? String.Format("[L{0}]: ", levelRequirement) 
				: "";

			if (IsStatBonus(bonusType) || IsSkillBonus(bonusType))
				delve.Add(String.Format("- {0}{1}: {2} pts",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount.ToString("+0;-0;0")));
			else if (IsResistBonus(bonusType))
				delve.Add(String.Format("- {0}{1}: {2}%",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount.ToString("+0;-0;0")));
			else if (bonusType == (int)eProperty.PowerPool)
				delve.Add(String.Format("- {0}{1}: {2}% of power pool.",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount.ToString("+0;-0;0")));
		}

		/// <summary>
		/// Artifact ToA magical bonus delve information.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="bonusAmount"></param>
		/// <param name="bonusType"></param>
		/// <param name="levelRequirement"></param>
		protected virtual void DelveBonus(List<String> delve, int bonusAmount, int bonusType,
			int levelRequirement)
		{
			if (!IsToABonus(bonusType))
				return;

			String levelTag = (levelRequirement > 0)
				? String.Format("[L{0}]: ", levelRequirement)
				: "";

			if (IsCapIncreaseBonus(bonusType) || bonusType == (int)eProperty.ArmorFactor)
				delve.Add(String.Format("{0}{1}: {2}",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount));
			else
				delve.Add(String.Format("{0}{1}: {2}%",
					levelTag,
					SkillBase.GetPropertyName((eProperty)bonusType),
					bonusAmount));
		}

		/// <summary>
		/// Artifact Magical Ability delve information (spells, procs).
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="bonusID"></param>
		/// <param name="levelRequirement"></param>
		public virtual void DelveMagicalAbility(List<String> delve, ArtifactBonus.TypeID bonusID, 
			int levelRequirement)
		{
			String levelTag = (levelRequirement > 0)
				? String.Format("[L{0}]: ", levelRequirement)
				: "";

			bool isProc = false;
			bool isSecondary = false;
			int spellID = 0;

			switch (bonusID)
			{
				case ArtifactBonus.TypeID.Spell:
					spellID = SpellID;
					isProc = false;
					isSecondary = false;
					break;
				case ArtifactBonus.TypeID.Spell1:
					spellID = SpellID1;
					isProc = false;
					isSecondary = true;
					break;
				case ArtifactBonus.TypeID.ProcSpell:
					spellID = ProcSpellID;
					isProc = true;
					isSecondary = false;
					break;
				case ArtifactBonus.TypeID.ProcSpell1:
					spellID = ProcSpellID1;
					isProc = true;
					isSecondary = true;
					break;
			}

			if (spellID == 0)
				return;

			delve.Add("");
			delve.Add(String.Format("{0}{1}Magical Ability:", levelTag,
				(isSecondary) ? "Secondary " : ""));

			SpellLine spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
			if (spellLine != null)
			{
				IList spells = SkillBase.GetSpellList(spellLine.KeyName);
				if (spells != null)
					foreach (Spell spell in spells)
						if (spell.ID == spellID)
							spell.Delve(delve);
			}

			if (isProc)
				delve.Add(String.Format("- Spell has a chance of casting when this {0} enemy.",
					(GlobalConstants.IsWeapon(Object_Type))
					? "weapon strikes an" : "armor is hit by"));
			else
				delve.Add("- This spell is cast when the item is used.");
		}

		/// <summary>
		/// Artifact weapon stats delve info.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="player"></param>
		public virtual void DelveWeaponStats(List<String> delve, GamePlayer player)
		{
			double itemDPS = DPS_AF / 10.0;
			double clampedDPS = Math.Min(itemDPS, 1.2 + 0.3 * player.Level);
			double itemSPD = SPD_ABS / 10.0;
			double effectiveDPS = itemDPS * Quality / 100.0 * Condition / MaxCondition;

			delve.Add("Damage Modifiers:");

			if (itemDPS != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicWeaponInfos.BaseDPS", itemDPS.ToString("0.0")));
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicWeaponInfos.ClampDPS", clampedDPS.ToString("0.0")));
			}

			if (SPD_ABS >= 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicWeaponInfos.SPD", itemSPD.ToString("0.0")));

			if (Quality != 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicWeaponInfos.Quality", Quality));

			if (Condition != 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicWeaponInfos.Condition", ConditionPercent));

			delve.Add(LanguageMgr.GetTranslation(player.Client, 
				"DetailDisplayHandler.WriteClassicWeaponInfos.DamageType",
				(Type_Damage == 0 ? "None" : GlobalConstants.WeaponDamageTypeToName(Type_Damage))));
			delve.Add(" ");

			delve.Add(LanguageMgr.GetTranslation(player.Client, 
				"DetailDisplayHandler.WriteClassicWeaponInfos.EffDamage"));
			
			if (itemDPS != 0)
				delve.Add("- " + effectiveDPS.ToString("0.0") + " DPS");
		}

		/// <summary>
		/// Artifact armor stats delve info.
		/// </summary>
		/// <param name="delve"></param>
		/// <param name="player"></param>
		public virtual void DelveArmorStats(List<String> delve, GamePlayer player)
		{
			delve.Add(LanguageMgr.GetTranslation(player.Client, "DetailDisplayHandler.WriteClassicArmorInfos.ArmorMod"));

			double af = 0;
			int afCap = player.Level;
			double effectiveAF = 0;

			if (DPS_AF != 0)
			{
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicArmorInfos.BaseFactor", DPS_AF));

				if (Object_Type != (int)eObjectType.Cloth)
					afCap *= 2;

				af = Math.Min(afCap, DPS_AF);

				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicArmorInfos.ClampFact", (int)af));
			}

			if (SPD_ABS >= 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicArmorInfos.Absorption", SPD_ABS));

			if (Quality != 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicArmorInfos.Quality", Quality));

			if (Condition != 0)
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicArmorInfos.Condition", ConditionPercent));

			delve.Add(" ");
			delve.Add(LanguageMgr.GetTranslation(player.Client, 
				"DetailDisplayHandler.WriteClassicArmorInfos.EffArmor"));

			if (DPS_AF != 0)
			{
				effectiveAF = af * Quality / 100.0 * Condition / MaxCondition * (1 + SPD_ABS / 100.0);
				delve.Add(LanguageMgr.GetTranslation(player.Client, 
					"DetailDisplayHandler.WriteClassicArmorInfos.Factor", (int)effectiveAF));
			}
		}

		/// <summary>
		/// Returns the level when this artifact will gain a new 
		/// ability.
		/// </summary>
		/// <returns></returns>
		private int GainsNewAbilityAtLevel()
		{
			for (ArtifactBonus.TypeID bonusID = ArtifactBonus.TypeID.Min; bonusID <= ArtifactBonus.TypeID.Max; ++bonusID)
				if (m_levelRequirements[(int)bonusID] > ArtifactLevel)
					return m_levelRequirements[(int)bonusID];

			return 10;
		}

		/// <summary>
		/// Check whether this type is a cap increase bonus.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsCapIncreaseBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.StatCapBonus_First) && (bonusType <= (int)eProperty.StatCapBonus_Last))
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is focus.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsFocusBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.Focus_Darkness) && (bonusType <= (int)eProperty.Focus_Arboreal))
				return true;
			if ((bonusType >= (int)eProperty.Focus_EtherealShriek) && (bonusType <= (int)eProperty.Focus_Witchcraft))
				return true;
			if (bonusType == (int)eProperty.AllFocusLevels)
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is resist.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsResistBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.Resist_First) && (bonusType <= (int)eProperty.Resist_Last))
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is a skill.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsSkillBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.Skill_First) && (bonusType <= (int)eProperty.Skill_Last))
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is a stat bonus.
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsStatBonus(int bonusType)
		{
			if ((bonusType >= (int)eProperty.Stat_First) && (bonusType <= (int)eProperty.Stat_Last))
				return true;
			if ((bonusType == (int)eProperty.MaxHealth) || (bonusType == (int)eProperty.MaxMana))
				return true;
			if ((bonusType == (int)eProperty.Fatigue) || (bonusType == (int)eProperty.PowerPool))
				return true;
			if (bonusType == (int)eProperty.Acuity)
				return true;
			if ((bonusType == (int)eProperty.AllMeleeWeaponSkills) || (bonusType == (int)eProperty.AllMagicSkills))
				return true;
			if ((bonusType == (int)eProperty.AllDualWieldingSkills) || (bonusType == (int)eProperty.AllArcherySkills))
				return true;
			return false;
		}

		/// <summary>
		/// Check whether this type is a ToA bonus (it's all mixed
		/// up in GlobalConstants.cs, but let's try anyway).
		/// </summary>
		/// <param name="bonusType"></param>
		/// <returns></returns>
		protected virtual bool IsToABonus(int bonusType)
		{
			if (IsCapIncreaseBonus(bonusType))
				return true;
			if ((bonusType >= (int)eProperty.ToABonus_First) && (bonusType <= (int)eProperty.ToABonus_Last) &&
				(bonusType != (int)eProperty.PowerPool) && (bonusType != (int)eProperty.Fatigue))
				return true;
			if ((bonusType >= (int)eProperty.MaxSpeed) && (bonusType <= (int)eProperty.MeleeSpeed))
				return true;
			if ((bonusType >= (int)eProperty.CriticalMeleeHitChance) &&
				(bonusType <= (int)eProperty.CriticalHealHitChance))
				return true;
			if ((bonusType >= (int)eProperty.EvadeChance) && (bonusType <= (int)eProperty.SpeedDecreaseDuration))
				return true;
			return false;
		}

		#endregion
	}
}
