using DawnOfLight.GameServer.GameObjects;

namespace DawnOfLight.GameServer.AI.Brain.Npcs
{
    public class MeleeBehaviour : IAttackBehaviour
    {
        public MeleeBehaviour(GameLiving body)
        {
            Body = body;
        }

        private GameLiving Body { get; set; }

        #region IAttackBehaviour Members

        public void Attack(GameObject target)
        {
            Body.StartAttack(target);
        }

        public void Retreat()
        {
            Body.StopAttack();
        }

        #endregion
    }
}
