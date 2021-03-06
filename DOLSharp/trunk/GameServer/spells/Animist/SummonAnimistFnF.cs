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
using DawnOfLight.GameServer.AI.Brain;
using DawnOfLight.GameServer.AI.Brain.Animist;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.GameServer.Events;
using DawnOfLight.GameServer.Events.GameObjects;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.GameObjects.Animist;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.ServerProperties;
using DawnOfLight.GameServer.Utilities;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.Spells.Animist
{
	/// <summary>
	/// Summon a fnf animist pet.
	/// </summary>
	[SpellHandler("SummonAnimistFnF")]
	public class SummonAnimistFnF : SummonAnimistPet
	{
		public SummonAnimistFnF(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			int nCount = 0;

			Region rgn = WorldMgr.GetRegion(Caster.CurrentRegion.ID);

			if (rgn == null || rgn.GetZone(Caster.GroundTarget.X, Caster.GroundTarget.Y) == null)
			{
                if (Caster is GamePlayer)
                    MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonAnimistFnF.CheckBeginCast.NoGroundTarget"), ChatType.CT_SpellResisted);
                return false;
			}

			foreach (GameNPC npc in Caster.CurrentRegion.GetNPCsInRadius(Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Properties.TURRET_AREA_CAP_RADIUS, false, true))
				if (npc.Brain is TurretFNFBrain)
					nCount++;

			if (nCount >= Properties.TURRET_AREA_CAP_COUNT)
			{
                if (Caster is GamePlayer)
                    MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonAnimistFnF.CheckBeginCast.TurretAreaCap"), ChatType.CT_SpellResisted);
                return false;
			}

			if (Caster.PetCount >= Properties.TURRET_PLAYER_CAP_COUNT)
			{
                if (Caster is GamePlayer)
                    MessageToCaster(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SummonAnimistFnF.CheckBeginCast.TurretPlayerCap"), ChatType.CT_SpellResisted);
                return false;
			}

			return base.CheckBeginCast(selectedTarget);
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			base.ApplyEffectOnTarget(target, effectiveness);

			if (Spell.SubSpellID > 0 && SkillBase.GetSpellByID(Spell.SubSpellID) != null)
			{
				m_pet.Spells.Add(SkillBase.GetSpellByID(Spell.SubSpellID));
			}

			(m_pet.Brain as TurretBrain).IsMainPet = false;

			(m_pet.Brain as IOldAggressiveBrain).AddToAggroList(target, 1);
			(m_pet.Brain as TurretBrain).Think();
			//[Ganrod] Nidel: Set only one spell.
			(m_pet as TurretPet).TurretSpell = m_pet.Spells[0] as Spell;
			Caster.PetCount++;
		}

		protected override void SetBrainToOwner(IControlledBrain brain)
		{
		}

		/// <summary>
		/// [Ganrod] Nidel: Can remove TurretFNF
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
			m_pet = sender as GamePet;
			if (m_pet == null)
				return;

			if ((m_pet.Brain as TurretFNFBrain) == null)
				return;

			if (Caster.ControlledBrain == null)
			{
				((GamePlayer)Caster).Out.SendPetWindow(null, ePetWindowAction.Close, 0, 0);
			}

			GameEventMgr.RemoveHandler(m_pet, GameLivingEvent.PetReleased, OnNpcReleaseCommand);

			GameSpellEffect effect = FindEffectOnTarget(m_pet, this);
			if (effect != null)
				effect.Cancel(false);
		}

		protected override byte GetPetLevel()
		{
			byte level = base.GetPetLevel();
			if (level > 44)
				level = 44;
			return level;
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			Caster.PetCount--;

			return base.OnEffectExpires(effect, noMessages);
		}

		protected override IControlledBrain GetPetBrain(GameLiving owner)
		{
			return new TurretFNFBrain(owner);
		}
	}
}
