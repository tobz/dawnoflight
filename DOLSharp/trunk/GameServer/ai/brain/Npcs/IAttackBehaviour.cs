using DawnOfLight.GameServer.GameObjects;

namespace DawnOfLight.GameServer.AI.Brain.Npcs
{
    public interface IAttackBehaviour
    {
        void Attack(GameObject target);
        void Retreat();
    }
}
