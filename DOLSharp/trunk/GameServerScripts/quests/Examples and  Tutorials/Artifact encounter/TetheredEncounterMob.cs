﻿using DawnOfLight.AI.Brain;
using DawnOfLight.GameServer.AI.Brain;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Network;

namespace DawnOfLight.GameServer.Atlantis
{
    /// <summary>
    /// The base class that most or all ArtifactEncounter mob's should inherit from.
    /// </summary>
    public class TetheredEncounterMob : BasicEncounterMob
    {
        public override void SaveIntoDatabase()
        {
        }

        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            //Check if this encounter mob is tethered and if so, ignore any damage done both outside of or too far from it's tether range.
            if (this.TetherRange > 100)
            {
                // if controlled NPC - do checks for owner instead
                if (source is GameNPC)
                {
                    IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
                    if (controlled != null)
                    {
                        source = controlled.GetPlayerOwner();
                    }
                }

                if (IsOutOfTetherRange)
                {
                    if (source is GamePlayer)
                    {
                        GamePlayer player = source as GamePlayer;
                        player.Out.SendMessage("The " + this.Name + " is too far from its encounter area, your damage fails to have an effect on it!", ChatType.CT_Important, ChatLocation.CL_ChatWindow);
                        return;
                    }
                    return;
                }
                else
                {
                    if (IsWithinRadius(source, this.TetherRange))
                    {
                        base.TakeDamage(source, damageType, damageAmount, criticalAmount);
                        return;
                    }
                    if (source is GamePlayer)
                    {
                        GamePlayer player = source as GamePlayer;
                        player.Out.SendMessage("You are too far from the " + this.Name + ", your damage fails to effect it!", ChatType.CT_Important, ChatLocation.CL_ChatWindow);
                    }
                    return;
                }
            }
            else
                base.TakeDamage(source, damageType, damageAmount, criticalAmount);
        }

    }
}
