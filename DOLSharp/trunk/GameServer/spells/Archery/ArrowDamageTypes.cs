//Andraste v2.0 - Vico

using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Spells.Archery
{
	[SpellHandler("ArrowDamageTypes")]
	public class ArrowDamageTypes : SpellHandler
	{
		/// <summary>
		/// Does this spell break stealth on finish?
		/// </summary>
		public override bool UnstealthCasterOnFinish
		{
			get { return false; }
		}

		/// <summary>
		/// Does this spell break stealth on start?
		/// </summary>
		public override bool UnstealthCasterOnStart
		{
			get { return false; }
		}

		public ArrowDamageTypes(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}