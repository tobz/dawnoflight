using System;
using System.Collections.Generic;
using System.Text;
using DawnOfLight.GameServer.Spells;
using DawnOfLight.GameServer.PacketHandler;

namespace DawnOfLight.GameServer.Effects
{
	public class FocusShellEffect : GameSpellEffect
	{
		public FocusShellEffect(ISpellHandler handler, int duration, int pulseFreq, double effectiveness) : base(handler, duration, pulseFreq, effectiveness) { }

		/// <summary>
		/// There is no duration!
		/// </summary>
		public new int RemainingTime
		{
			get
			{
				return 1;
			}
		}
	}
}
