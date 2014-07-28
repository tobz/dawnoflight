using DawnOfLight.GameServer.Keeps.Managers;

namespace DawnOfLight.GameServer.AI.Brain.Guards
{
	/// <summary>
	/// Caster Guards Brain
	/// </summary>
	public class CasterBrain : KeepGuardBrain
	{
		/// <summary>
		/// Brain Think
		/// </summary>
		public override void Think()
		{
			CheckForNuking();
			base.Think();
		}
		private void CheckForNuking()
		{
			if(guard==null) return;
			if (guard.CanUseRanged)
				SpellMgr.CheckForNuke(guard);
		}
	}
}
