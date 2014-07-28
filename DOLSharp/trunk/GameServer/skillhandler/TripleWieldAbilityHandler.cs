using System.Reflection;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Utilities;
using log4net;

namespace DawnOfLight.GameServer.SkillHandler
{
	/// <summary>
	/// Handler for Triple Wield clicks
	/// </summary>
	[SkillHandler(Abilities.Triple_Wield)]
	public class TripleWieldAbilityHandler : IAbilityActionHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The ability reuse time in seconds
		/// </summary>
		protected const int REUSE_TIMER = 7 * 60; // 7 minutes 

		/// <summary>
		/// The ability effect duration in seconds
		/// </summary>
		public const int DURATION = 30;

		public void Execute(Ability ab, GamePlayer player)
		{
			if (player == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not retrieve player in TripleWieldAbilityHandler.");
				return;
			}

            if (!player.IsAlive)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseDead"), ChatType.CT_System, ChatLocation.CL_SystemWindow);
                return;
            }
            if (player.IsMezzed)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseMezzed"), ChatType.CT_System, ChatLocation.CL_SystemWindow);
                return;
            }
            if (player.IsStunned)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStunned"), ChatType.CT_System, ChatLocation.CL_SystemWindow);
                return;
            }
            if (player.IsSitting)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseStanding"), ChatType.CT_System, ChatLocation.CL_SystemWindow);
                return;
            }
			TripleWieldEffect tw = player.EffectList.GetOfType<TripleWieldEffect>();
			if (tw != null)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Skill.Ability.CannotUseAlreadyActive"), ChatType.CT_System, ChatLocation.CL_SystemWindow);
                return;
			}
			TripleWieldEffect twe = new TripleWieldEffect(DURATION * 1000);
			twe.Start(player);
			player.DisableSkill(ab, REUSE_TIMER * 1000);
		}
	}
}
