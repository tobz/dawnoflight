using System.Collections;
using DawnOfLight.GameServer.AI.Brain;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Spells.Bainshee
{
	[SpellHandler("BeFriend")]
	public class BeFriendSpellHandler : SpellHandler 
	{
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast (target);
		}

		public override IList SelectTargets(GameObject castTarget)
		{
			ArrayList list = new ArrayList();
			GameLiving target;
			
			target=Caster;
			foreach (GameNPC npc in target.GetNPCsInRadius((ushort)Spell.Radius)) 
			{
				if(npc is GameNPC)
					list.Add(npc);
			}
			
			return list;
		}

		public virtual IList SelectRealmTargets(GameObject castTarget)
		{
			ArrayList list = new ArrayList();
			
			foreach (GamePlayer player in castTarget.GetPlayersInRadius((ushort)1000)) 
			{
				if(player.Realm == m_caster.Realm && player!=m_caster)
					list.Add(player);
			}

			list.Add(m_caster);

			return list;
		}

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override bool StartSpell(GameLiving target)
		{
			if (target == null) return false;

			IList targets = SelectTargets(target);
			IList realmtargets = SelectRealmTargets(target);

			foreach (GameLiving t in targets)
			{
				if(t.Level <= m_spell.Value)
				{
					GameNPC mob = (GameNPC)t;
					if(mob.Brain is StandardMobBrain)
					{
						StandardMobBrain sBrain = (StandardMobBrain) mob.Brain;
						//mob.StopAttack();

						foreach(GamePlayer player in realmtargets)
							sBrain.RemoveFromAggroList(player);

					}

					mob.AddBrain(new FriendBrain(this));
				}
			}

			return true;
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GameNPC mob = (GameNPC)effect.Owner;
			mob.RemoveBrain(mob.Brain);

			if(mob.Brain==null)
				mob.AddBrain(new StandardMobBrain());

			return base.OnEffectExpires (effect, noMessages);
		}


		public BeFriendSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
