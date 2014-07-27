//Andraste v2.0 - Vico

using System;
using DawnOfLight.GameServer;
using DawnOfLight.GameServer.PacketHandler;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.Events;

namespace DawnOfLight.GameServer.Spells
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