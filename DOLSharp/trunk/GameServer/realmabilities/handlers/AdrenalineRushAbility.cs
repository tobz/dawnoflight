using System.Collections.Generic;
using DawnOfLight.Database;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.RealmAbilities.effects;

namespace DawnOfLight.GameServer.RealmAbilities.handlers
{
    public class AdrenalineRushAbility : TimedRealmAbility
    {
        int m_duration = 20000;
        int m_value = 100;

        public AdrenalineRushAbility(DBAbility dba, int level) : base(dba, level) { }

        public override void Execute(GameLiving living)
        {
            GamePlayer player = living as GamePlayer;
            if (CheckPreconditions(living, DEAD | SITTING | STEALTHED)) return;
			if (player.EffectList.CountOfType<AdrenalineRushEffect>() > 0)
            {
                player.Out.SendMessage("You already an effect of that type!", ChatType.CT_SpellResisted, ChatLocation.CL_SystemWindow);
            }
            SendCasterSpellEffectAndCastMessage(living, 7002, true);
            if (player != null)
            {
                new AdrenalineRushEffect(m_duration, m_value).Start(living);
            }
            DisableSkill(living);
        }
        public override int GetReUseDelay(int level)
        {
        	if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
        	{
	            switch (level)
	            {
	                case 1: return 1200;
	                case 2: return 900;
	                case 3: return 600;
	                case 4: return 450;
	                case 5: return 300;
	            }       		        		
        	}
        	else
        	{
	            switch (level)
	            {
	                case 1: return 1200;
	                case 2: return 600;
	                case 3: return 300;
	            }       		
	        }

            return 600;
        }

        public override void AddEffectsInfo(IList<string> list)
        {
        	// TODO translate
        	if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
        	{
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info1"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info2"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info3"));
	            list.Add("");
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info4"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info5"));
        	}
        	else
        	{
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info1"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info2"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info3"));
	            list.Add("");
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info4"));
	            list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "AdrenalineRushAbility.AddEffectsInfo.Info5"));
        	}

        }

    }
}
