using System;
using System.Collections;
using System.Reflection;
using DawnOfLight.GameServer.PacketHandler;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.Events;
using log4net;

namespace DawnOfLight.GameServer.SkillHandler
{
	/// <summary>
	/// Handler for Vampiir Bolt clicks
	/// </summary>
	[SkillHandler(Abilities.VampiirBolt)]
	public class VampiirBoltAbilityHandler : SpellCastingAbilityHandler
	{
		public override long Preconditions
		{
			get
			{
				return DEAD | SITTING | MEZZED | STUNNED | TARGET;
			}
		}

		public override int SpellID
		{
			get
			{
				return 13200 + m_ability.Level;
			}
		}
	}
}
