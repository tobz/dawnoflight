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
using System.Collections;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Database;
using log4net;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Albion Rogue Trainer
	/// </summary>	
	public class AlbionRogueTrainer : GameStandardTrainer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This function is called at the server startup
		/// </summary>	
		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			#region Practice dirk

			ThrustWeaponTemplate practice_dirk_template = new ThrustWeaponTemplate();
			practice_dirk_template.Name = "practice dirk";
			practice_dirk_template.Level = 0;
			practice_dirk_template.Durability = 100;
			practice_dirk_template.Condition = 100;
			practice_dirk_template.Quality = 90;
			practice_dirk_template.Bonus = 0;
			practice_dirk_template.DamagePerSecond = 12;
			practice_dirk_template.Speed = 2200;
			practice_dirk_template.Weight = 8;
			practice_dirk_template.Model = 21;
			practice_dirk_template.Realm = eRealm.Albion;
			practice_dirk_template.IsDropable = true; 
			practice_dirk_template.IsTradable = false; 
			practice_dirk_template.IsSaleable = false;
			practice_dirk_template.MaterialLevel = eMaterialLevel.Bronze;
	
			if(!allStartupItems.Contains(practice_dirk_template))
			{
				allStartupItems.Add(practice_dirk_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + practice_dirk_template.Name + " to AlbionRogueTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// This hash constrain all item template the trainer can give
		/// </summary>	
		protected static IList allStartupItems = new ArrayList();

		/// <summary>
		/// Gets all trainer gifts
		/// </summary>
		public override IList TrainerGifts
		{
			get { return allStartupItems; }
		}
	
		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Rogue"; }
		}

		/// <summary>
		/// Gets trained class
		/// </summary>
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.AlbionRogue; }
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;
								
			player.Out.SendMessage(this.Name + " says, \"[Infiltrator], [Minstrel], or [Scout]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												
					
			return true;
		}

		/// <summary>
		/// Talk to trainer
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{				
			if (!base.WhisperReceive(source, text)) return false;	
			GamePlayer player = source as GamePlayer;

			switch (text) {
			case "Infiltrator":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Inconnu){
					player.Out.SendMessage(this.Name + " says, \"You seek a tough life if you go that path. The life of an Infiltrator involves daily use of his special skills. The Guild of Shadows has made its fortune by using them to sneak, hide, disguise, backstab, and spy on the enemy. Without question they are an invaluable asset to Albion.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of an Infiltrator is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Minstrel":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Highlander){
					player.Out.SendMessage(this.Name + " says, \"Ah! To sing the victories of Albion! To give praise to those who fight to keep the light of Camelot eternal. Many have studied their skill within the walls of The Academy and come forth to defend this realm. Without their magical songs, many would not be here today.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Minstrel is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Scout":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Inconnu){
					player.Out.SendMessage(this.Name + " says, \"Ah! You wish to join the Defenders of Albion eh? That is quite a good choice for a Rogue. A Scouts accuracy with a bow is feared by all our enemies and has won Albion quite a few battles.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Scout is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			}
			return true;			
		}
	}
}
