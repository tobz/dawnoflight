using System.Collections.Generic;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Spells
{
	/// <summary>
	///
	/// </summary>
	[SpellHandler("CureAll")]
	public class CureAllSpellHandler : RemoveSpellEffectHandler
	{
		// constructor
		public CureAllSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			m_spellTypesToRemove = new List<string>();
			m_spellTypesToRemove.Add("DamageOverTime");
			m_spellTypesToRemove.Add("Nearsight");
            m_spellTypesToRemove.Add("Silence");
			m_spellTypesToRemove.Add("Disease");
            m_spellTypesToRemove.Add("StyleBleeding");
		}
	}
}