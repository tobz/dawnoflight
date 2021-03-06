﻿/*
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
using DawnOfLight.GameServer.AI.Brain;
using DawnOfLight.GameServer.GameObjects;

namespace DawnOfLight.GameServer.Utilities
{

	/// <summary>
	/// LootGeneratorDragonscales
	/// At the moment this generator only adds dragonscales to the loot
	/// </summary>
	public class LootGeneratorDragonscales : LootGeneratorBase
	{
		
		private static ItemTemplate m_dragonscales = GameServer.Database.FindObjectByKey<ItemTemplate>("dragonscales");
		/// <summary>
        /// Generate loot for given mob
		/// </summary>
		/// <param name="mob"></param>
		/// <param name="killer"></param>
		/// <returns></returns>
		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);
			
			try
			{
				GamePlayer player = killer as GamePlayer;
				if (killer is GameNPC && ((GameNPC)killer).Brain is IControlledBrain)
					player = ((ControlledNpcBrain)((GameNPC)killer).Brain).GetPlayerOwner();
				if (player == null)
					return loot;			
			
				
				ItemTemplate dragonscales = new ItemTemplate(m_dragonscales);

				int killedcon = (int)player.GetConLevel(mob)+3;
				
				if(killedcon <= 0)
					return loot;
				
				int lvl = mob.Level + 1;
				if (lvl < 1) lvl = 1;
				int maxcount = 1;
				
				//Switch pack size
				if (lvl > 0 && lvl < 10) 
				{
					//Single Dragonscales
					maxcount = (int)Math.Floor((double)(lvl/2))+1;
				}
				else if (lvl >= 10 && lvl < 20)
				{
					//Double
					dragonscales.PackSize = 2;
					maxcount = (int)Math.Floor((double)((lvl-10)/2))+1;
				}
				else if (lvl >= 20 && lvl < 30)
				{
					//Triple
					dragonscales.PackSize = 3;
					maxcount = (int)Math.Floor((double)((lvl-20)/2))+1;
					
				}
				else if (lvl >=30 && lvl < 40) 
				{
					//Quad
					dragonscales.PackSize = 4;
					maxcount = (int)Math.Floor((double)((lvl-30)/2))+1;
				}
				else if (lvl >= 40 && lvl < 50)
				{
					//Quint
					dragonscales.PackSize = 5;
					maxcount = (int)Math.Floor((double)((lvl-40)/2))+1;
				}
				else 
				{
					//Cache (x10)
					dragonscales.PackSize = 10;
					maxcount = (int)Math.Round((double)(lvl/10));
				}
				
				if (!mob.Name.ToLower().Equals(mob.Name))
				{
					//Named mob, more cash !
					maxcount = (int)Math.Round(maxcount*ServerProperties.Properties.LOOTGENERATOR_DRAGONSCALES_NAMED_COUNT);
				}
				
				if(maxcount > 0 && Util.Chance(ServerProperties.Properties.LOOTGENERATOR_DRAGONSCALES_BASE_CHANCE+Math.Max(10, killedcon)))
				{
					loot.AddFixed(dragonscales, maxcount);
				}
				
			}
			catch
			{
				return loot;
			}
			
			return loot;
		}
	}
}
