using System.Collections;
using DawnOfLight.GameServer.AI.Brain;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Spells.Bainshee
{
	[SpellHandler("Fear")]
	public class FearSpellHandler : SpellHandler 
	{
		//VaNaTiC->
		/*
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		*/
		//VaNaTiC<-

		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast (target);
			
			GameNPC t = target as GameNPC;
			if(t!=null)
				t.WalkToSpawn();
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

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		public override bool StartSpell(GameLiving target)
		{
			if (target == null) return false;

			IList targets = SelectTargets(target);

			foreach (GameLiving t in targets)
			{
				if(t is GameNPC && t.Level <= m_spell.Value)
				{
					((GameNPC)t).AddBrain(new FearBrain());
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


		public FearSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
