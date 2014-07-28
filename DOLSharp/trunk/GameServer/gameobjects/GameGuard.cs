using System.Collections;
using DawnOfLight.GameServer.AI.Brain;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.GameObjects
{
    public class GameGuard : GameNPC
    {
        public GameGuard()
            : base()
        {
            m_ownBrain = new GuardBrain();
            m_ownBrain.Body = this;
        }

        public override void DropLoot(GameObject killer)
        {
            //Guards dont drop loot when they die
        }

        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = new ArrayList(4);
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.GetExamineMessages.Examine", 
                                                GetName(0, true, player.Client.Account.Language, this), GetPronoun(0, true, player.Client.Account.Language),
                                                GetAggroLevelString(player, false)));
            return list;
        }

        public override void StartAttack(GameObject attackTarget)
        {
            base.StartAttack(attackTarget);

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
            {
                if (player != null)
                    switch (Realm)
                    {
                        case eRealm.Albion:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Albion.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                        case eRealm.Midgard:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Midgard.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                        case eRealm.Hibernia:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Hibernia.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow); break;
                    }
            }
        }
    }
}