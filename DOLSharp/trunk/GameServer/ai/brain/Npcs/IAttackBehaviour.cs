using System;
using System.Collections.Generic;
using System.Text;
using DawnOfLight.GameServer;

namespace DawnOfLight.AI.Brain
{
    public interface IAttackBehaviour
    {
        void Attack(GameObject target);
        void Retreat();
    }
}
