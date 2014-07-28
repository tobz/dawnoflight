using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.GameObjects.Theurgist
{
	public class TheurgistPet : GamePet
	{
		public TheurgistPet(INpcTemplate npcTemplate) : base(npcTemplate) { }

		public override void OnAttackedByEnemy(AttackData ad) { /* do nothing */ }
		
		/// <summary>
		/// not each summoned pet 'll fire ambiant sentences
		/// let's say 10%
		/// </summary>
		protected override void BuildAmbientTexts()
		{
			base.BuildAmbientTexts();
			if (ambientTexts.Count>0)
				foreach (var at in ambientTexts)
					at.Chance /= 10;
		}
	}
}
