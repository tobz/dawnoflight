/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using System;
using DawnOfLight.Database;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.RealmAbilities.effects;
using DawnOfLight.GameServer.RealmAbilities.effects.rr5;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.RealmAbilities.handlers
{
	public class MasteryofConcentrationAbility : TimedRealmAbility
	{
        public MasteryofConcentrationAbility(DBAbility dba, int level) : base(dba, level) { }
		public const Int32 Duration = 30 * 1000;

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
			GamePlayer caster = living as GamePlayer;

			if (caster == null)
				return;

			MasteryofConcentrationEffect MoCEffect = caster.EffectList.GetOfType<MasteryofConcentrationEffect>();
			if (MoCEffect != null)
			{
				MoCEffect.Cancel(false);
				return;
			}
			
			// Check for the RA5L on the Sorceror: he cannot cast MoC when the other is up
			ShieldOfImmunityEffect ra5l = caster.EffectList.GetOfType<ShieldOfImmunityEffect>();
			if (ra5l != null)
			{
				caster.Out.SendMessage("You cannot currently use this ability", ChatType.CT_SpellResisted, ChatLocation.CL_SystemWindow);
				return;
			}
			
			SendCasterSpellEffectAndCastMessage(living, 7007, true);
			foreach (GamePlayer player in caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{

                if ( caster.IsWithinRadius( player, WorldMgr.INFO_DISTANCE ) )
				{
					if (player == caster)
					{
						player.MessageToSelf("You cast " + this.Name + "!", ChatType.CT_Spell);
						player.MessageToSelf("You become steadier in your casting abilities!", ChatType.CT_Spell);
					}
					else
					{
						player.MessageFromArea(caster, caster.Name + " casts a spell!", ChatType.CT_Spell, ChatLocation.CL_SystemWindow);
						player.Out.SendMessage(caster.Name + "'s castings have perfect poise!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
					}
				}
			}

			DisableSkill(living);

			new MasteryofConcentrationEffect().Start(caster);
		}
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
        
        public virtual int GetAmountForLevel(int level)
		{
        	if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
        	{
        		switch(level)
        		{
        			case 1: return 25;
        			case 2: return 35;
        			case 3: return 50;
        			case 4: return 60;
        			case 5: return 75;
        		}
        	}
        	else
        	{
         		switch(level)
        		{
        			case 1: return 25;
        			case 2: return 50;
        			case 3: return 75;
        		}       		
        	}
        	return 25;
		}
	}
}
