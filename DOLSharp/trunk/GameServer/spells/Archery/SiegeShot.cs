//Andraste v2.0 -Vico

using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.GameObjects.Keeps;
using DawnOfLight.GameServer.Keeps;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Spells.Archery
{
	[SpellHandler("SiegeArrow")]
	public class SiegeArrow : BoltSpellHandler
	{
		/// <summary>
		/// Does this spell break stealth on start?
		/// </summary>
		public override bool UnstealthCasterOnStart
		{
			get { return false; }
		}

		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (Caster != null && Caster is GamePlayer && Caster.AttackWeapon != null && (Caster.AttackWeapon.Object_Type == 15 || Caster.AttackWeapon.Object_Type == 18 || Caster.AttackWeapon.Object_Type == 9))
			{
				if (!(selectedTarget is GameKeepComponent || selectedTarget is GameKeepDoor))
				{
					MessageToCaster("You must have a Keep Component targeted for this spell!", ChatType.CT_Spell);
					return false;
				}
				return base.CheckBeginCast(selectedTarget);
				//if (!Caster.IsWithinRadius( selectedTarget, Spell.Range )) { MessageToCaster("That target is too far away!", ChatType.CT_Spell); return false; }
			}
			return false;
		}
		
		public override void FinishSpellCast(GameLiving target)
		{
			if (!(target is GameKeepComponent || target is GameKeepDoor))
			{
				MessageToCaster("Your target must be a Keep Component!", ChatType.CT_SpellResisted);
				return;
			}

			target.LastAttackedByEnemyTickPvP = target.CurrentRegion.Time;
			Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;

			base.FinishSpellCast(target);
		}

		public override void SendSpellMessages() { MessageToCaster("You prepare " + Spell.Name, ChatType.CT_Spell); }
		public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
			ad.Damage *= 30;  // actual value unknown
			return ad;
		}

		public override int CalculateToHitChance(GameLiving target)
		{
			if ((target is GameKeepComponent || target is GameKeepDoor))
				return 100;

			return 0;
		}

		public override int PowerCost(GameLiving target) { return 0; }

		public override int CalculateEnduranceCost() { return (int)(Caster.MaxEndurance * (Spell.Power * .01)); }

		public SiegeArrow(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}
