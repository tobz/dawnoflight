using DawnOfLight.GameServer.Spells;

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
