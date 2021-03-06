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
/*
 * Author:		Doulbousiouf
 * Date:			
 * Directory: /scripts/quests/albion/
 *
 * Description:
 *  Brief Walkthrough: 
 * 1) Travel to loc=15075,25906 Camelot Hills to speak with Farmer Asma and take is plan
 * 2) Go to the first vacant field at loc=19590,20668 Camelot Hills (you can /use the plan for more infos).
 * 3) Go to the second vacant field at loc=25093,25428 Camelot Hills (you can /use the plan for more infos).
 * 4) Go to the third vacant field at loc=28083,29814 Camelot Hills (you can /use the plan for more infos).
 * 3) Come back to Farmer Asma to have have your reward. 
 */

using System;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;
using NHibernate.Expression;
using NHibernate.Mapping.Attributes;
/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests.Albion
{
	/* The first thing we do, is to declare the quest requirement
	* class linked with the new Quest. To do this, we derive 
	* from the abstract class AbstractQuestDescriptor
	*/
	public class GreenerPasturesDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(GreenerPastures); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 2; }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 5; }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[Subclass(NameType = typeof(GreenerPastures), ExtendsType = typeof(AbstractQuest))]
	public class GreenerPastures : BaseQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/* Declare the variables we need inside our quest.
		 * You can declare static variables here, which will be available in 
		 * ALL instance of your quest and should be initialized ONLY ONCE inside
		 * the OnScriptLoaded method.
		 * 
		 * Or declare nonstatic variables here which can be unique for each Player
		 * and change through the quest journey...
		 * 
		 */
		protected const string questTitle = "Greener Pastures";

		private static GameMob farmerAsma = null;

		private static GenericItemTemplate farmerAsmasMap = null;

		private static GameLocation firstField = new GameLocation("First Field", 1, 568278, 504052, 2168);
		private static GameLocation secondField = new GameLocation("Second Field", 1, 573718, 509044, 2192);
		private static GameLocation thirdField = new GameLocation("Third Field", 1, 577336, 513324, 2169);

		private static Circle firstFieldArea = null;
		private static Circle secondFieldArea = null;
		private static Circle thirdFieldArea = null;

		/* The following method is called automatically when this quest class
		 * is loaded. You might notice that this method is the same as in standard
		 * game events. And yes, quests basically are game events for single players
		 * 
		 * To make this method automatically load, we have to declare it static
		 * and give it the [ScriptLoadedEvent] attribute. 
		 *
		 * Inside this method we initialize the quest. This is neccessary if we 
		 * want to set the quest hooks to the NPCs.
		 * 
		 * If you want, you can however add a quest to the player from ANY place
		 * inside your code, from events, from custom items, from anywhere you
		 * want. 
		 */

		[ScriptLoadedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initializing ...");
			/* First thing we do in here is to search for the NPCs inside
			* the world who comes from the certain Realm. If we find a the players,
			* this means we don't have to create a new one.
			* 
			* NOTE: You can do anything you want in this method, you don't have
			* to search for NPC's ... you could create a custom item, place it
			* on the ground and if a player picks it up, he will get the quest!
			* Just examples, do anything you like and feel comfortable with :)
			*/

			#region defineNPCS

			farmerAsma = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Farmer Asma") as GameMob;
			if (farmerAsma == null)
			{
				farmerAsma = new GameMob();
				farmerAsma.Model = 82;
				farmerAsma.Name = "Farmer Asma";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + farmerAsma.Name + ", creating him ...");
				farmerAsma.GuildName = "Part of " + questTitle + " Quest";
				farmerAsma.Realm = (byte)eRealm.Albion;
				farmerAsma.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(31));
				template.AddItem(eInventorySlot.Cloak, new NPCEquipment(57));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(32));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(33));
				farmerAsma.Inventory = template;
				
				farmerAsma.Size = 50;
				farmerAsma.Level = 35;
				farmerAsma.Position = new Point(563939, 509234, 2744);
				farmerAsma.Heading = 21;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = farmerAsma;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				farmerAsma.OwnBrain = newBrain;

				if(!farmerAsma.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(farmerAsma);
			}

			#endregion

			#region defineItems

			// item db check
			farmerAsmasMap = GameServer.Database.SelectObject(typeof(GenericItemTemplate), Expression.Eq("Name", "Farmer Asma's Map")) as GenericItemTemplate;
			if (farmerAsmasMap == null)
			{
				farmerAsmasMap = new GenericItemTemplate();
				farmerAsmasMap.Name = "Farmer Asma's Map";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + farmerAsmasMap.Name + ", creating it ...");
				farmerAsmasMap.Level = 0;
				farmerAsmasMap.Weight = 1;
				farmerAsmasMap.Model = 499;

				farmerAsmasMap.IsDropable = false;
				farmerAsmasMap.IsSaleable = false;
				farmerAsmasMap.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(farmerAsmasMap);
			}

			#endregion

			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			firstFieldArea = new Circle();
			firstFieldArea.Description = "First Vacant Field";
			firstFieldArea.IsBroadcastEnabled = false;
			firstFieldArea.Radius = 1450;
			firstFieldArea.RegionID = firstField.Region.RegionID;
			firstFieldArea.X = firstField.Position.X;
			firstFieldArea.Y = firstField.Position.Y;

			secondFieldArea = new Circle();
			secondFieldArea.Description = "Second Vacant Field";
			secondFieldArea.IsBroadcastEnabled = false;
			secondFieldArea.Radius = 1450;
			secondFieldArea.RegionID = secondField.Region.RegionID;
			secondFieldArea.X = secondField.Position.X;
			secondFieldArea.Y = secondField.Position.Y;

			thirdFieldArea = new Circle();
			thirdFieldArea.Description = "Third Vacant Field";
			thirdFieldArea.IsBroadcastEnabled = false;
			thirdFieldArea.Radius = 1450;
			thirdFieldArea.RegionID = thirdField.Region.RegionID;
			thirdFieldArea.X = thirdField.Position.X;
			thirdFieldArea.Y = thirdField.Position.Y;

			AreaMgr.RegisterArea(firstFieldArea);
			AreaMgr.RegisterArea(secondFieldArea);
			AreaMgr.RegisterArea(thirdFieldArea);

			GameEventMgr.AddHandler(AreaEvent.PlayerEnter, new DOLEventHandler(PlayerEnterFieldArea));

			GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.AddHandler(farmerAsma, GameObjectEvent.Interact, new DOLEventHandler(TalkToFarmerAsma));
			GameEventMgr.AddHandler(farmerAsma, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFarmerAsma));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.AbortQuest, new DOLEventHandler(QuestCancelDialogResponse));
			
			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(farmerAsma, typeof(GreenerPasturesDescriptor));

			if (log.IsInfoEnabled)
				log.Info("Quest \"" + questTitle + "\" initialized");
		}

		/* The following method is called automatically when this quest class
		 * is unloaded. 
		 * 
		 * Since we set hooks in the load method, it is good practice to remove
		 * those hooks again!
		 */

		[ScriptUnloadedEvent]
		public static void ScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			/* If Farmer Asma has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (farmerAsma == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			AreaMgr.UnregisterArea(firstFieldArea);
			AreaMgr.UnregisterArea(secondFieldArea);
			AreaMgr.UnregisterArea(thirdFieldArea);

			GameEventMgr.RemoveHandler(AreaEvent.PlayerEnter, new DOLEventHandler(PlayerEnterFieldArea));

			GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(PlayerEnterWorld));

			GameEventMgr.RemoveHandler(farmerAsma, GameObjectEvent.Interact, new DOLEventHandler(TalkToFarmerAsma));
			GameEventMgr.RemoveHandler(farmerAsma, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToFarmerAsma));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.AbortQuest, new DOLEventHandler(QuestCancelDialogResponse));
			
			/* Now we remove to Ydenia the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(farmerAsma, typeof(GreenerPasturesDescriptor));
		}

		protected static void PlayerEnterWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			GreenerPastures quest = player.IsDoingQuest(typeof(GreenerPastures)) as GreenerPastures;
			if (quest != null)
			{
				GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			}
		}

		protected static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;
			if (player == null)
				return;

			GreenerPastures quest = player.IsDoingQuest(typeof(GreenerPastures)) as GreenerPastures;
			if (quest != null)
			{
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
				GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToFarmerAsma(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs)args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(GreenerPastures), player, farmerAsma) <= 0)
				return;

			//We also check if the player is already doing the quest
			GreenerPastures quest = player.IsDoingQuest(typeof(GreenerPastures)) as GreenerPastures;

			farmerAsma.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					farmerAsma.SayTo(player, "Good night. I wish I had time to talk, " + player.CharacterClass.Name + ", but I'm in the process of trying to find a new field to lease. I'd like to return to my life as a farmer. It's not that Cotswold isn't a nice village, but I feel more at home in the [field].");
					return;
				}
				else
				{
					if (quest.Step == 4)
					{
						farmerAsma.SayTo(player, "Ah, you've returned. I hope you were able to find the fields without too much difficulty. I'm still learning my way around the area.  Which field would you recommend renting?");
						SendMessage(player, "[I'd recommend the first field.]", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						SendMessage(player, "[The second field is best.]", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						SendMessage(player, "[You should rent the third one.]", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
					return;
				}
			}
			// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs)args;
				if (quest == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "field":
							farmerAsma.SayTo(player, "Ah, yes, Camelot Hills, where the wind comes sweepin' down the plain, and the wavin' barley can sure smell sweet when the wind comes right behind the rain. I have a lead on some fields that are up for lease, but I don't have time to [check them out].");
							break;
						case "check them out":
							farmerAsma.SayTo(player, "Would you be willing to take a look at these fields for me and let me know if you think they are worth leasing?");
							QuestMgr.ProposeQuestToPlayer(typeof(GreenerPastures), "Will you help Farmer Asma \nsearch for new farmland?\n[Level 2-5]", player, farmerAsma);
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "them":
							if (quest.Step < 4)
							{
								farmerAsma.SayTo(player, "When you're done taking a look at the fields, please return to me and let me know what you saw.");
							}
							break;

						case "I'd recommend the first field.":
						case "The second field is best.":
						case "You should rent the third one.":
							if (quest.Step == 4)
							{
								farmerAsma.SayTo(player, "Excellent. I'll speak to the owner tomorrow. May I have the map back?");
							}
							break;
					}
				}
			}
		}

		protected static void PlayerUseSlot(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;

			GreenerPastures quest = (GreenerPastures)player.IsDoingQuest(typeof(GreenerPastures));
			if (quest == null)
				return;

			UseSlotEventArgs uArgs = (UseSlotEventArgs)args;

			GenericItem item = player.Inventory.GetItem((eInventorySlot)uArgs.Slot) as GenericItem;
			if (item != null && item.QuestName == quest.Name && item.Name == farmerAsmasMap.Name)
			{
				switch (quest.Step)
				{
					case 1:
						SendMessage(player, "To find the first vacant field, travel a short distance north from the Shrouded Isles portal.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						break;

					case 2:
						SendMessage(player, "Farmer Asma's map shows that the second field located across the road to the east southeast of the first vacant field.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						break;

					case 3:
						SendMessage(player, "You open Farmer Asma's Map and discover that the last field is a short trip to the southeast from the second field.", 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
						break;
				}
			}
		}

		protected static void PlayerEnterFieldArea(DOLEvent e, object sender, EventArgs args)
		{
			AreaEventArgs aargs = args as AreaEventArgs;
			GamePlayer player = aargs.GameObject as GamePlayer;

			GreenerPastures quest = player.IsDoingQuest(typeof(GreenerPastures)) as GreenerPastures;

			if (quest == null) return;
			
			switch(quest.Step)
			{
				case 1 :
					if(aargs.Area == firstFieldArea) // first area
					{
						player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've located the first field on Asma's \nMap. Turning over the map, you jot down \na few notes about your impressions. \nYour quest journal has been updated.");
						quest.ChangeQuestStep(2);
					}
					break;

				case 2 :
					if(aargs.Area == secondFieldArea) // secnd area
					{
						player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've located the next field on Asma's \nMap. Turning over the map, you jot down \na few notes about your impressions. \nYour quest journal has been updated.");
						quest.ChangeQuestStep(3);
					}
					break;

				case 3 :
					if(aargs.Area == thirdFieldArea) // third area
					{
						player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You've located the last field on Asma's \nMap. Turning over the map, you jot down \na few notes about your impressions. \nYour quest journal has been updated.");
						quest.ChangeQuestStep(4);
					}
					break;
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if (gArgs != null && gArgs.QuestType.Equals(typeof(GreenerPastures)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(GreenerPastures), player, gArgs.Source as GameNPC))
					{
						// give map
						GiveItemToPlayer(CreateQuestItem(farmerAsmasMap, player.IsDoingQuest(typeof(GreenerPastures))), player);
				
						SendReply(player, "Thank you for agreeing to help.  A man in Cotswold was kind enough to draw a map of some vacant fields in the area. I'll give you the map so that you can travel to the fields and take a look at [them].");

						GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
						GameEventMgr.AddHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
					}
				}
				else if (e == GamePlayerEvent.DeclineQuest)
				{
					player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest cancel dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestCancelDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestCancelEventArgs gArgs = args as QuestCancelEventArgs;

			if (gArgs != null && gArgs.Quest.GetType().Equals(typeof(GreenerPastures)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AbortQuest)
				{
					GameEventMgr.RemoveHandler(player, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
					GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		
					RemoveItemFromPlayer(player.Inventory.GetFirstItemByName(farmerAsmasMap.Name, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv), player);

					gArgs.Quest.AbortQuest();
				}		
			}
		}

		/* Now we set the quest name.
		 * If we don't override the base method, then the quest
		 * will have the name "UNDEFINED QUEST NAME" and we don't
		 * want that, do we? ;-)
		 */

		public override string Name
		{
			get { return questTitle; }
		}

		/* Now we set the quest step descriptions.
		 * If we don't override the base method, then the quest
		 * description for ALL steps will be "UNDEFINDED QUEST DESCRIPTION"
		 * and this isn't something nice either ;-)
		 */

		public override string Description
		{
			get
			{
				switch (Step)
				{
					case 1:
						return "[Step #1] Find the first field listed on Asma's Map. To read the map, right click on it in your inventory and /use it.";
					case 2:
						return "[Step #2] Now that you've inspected the first field, find the second field listed on Asma's Map. To read the map, right click on it in your inventory and /use it.";
					case 3:
						return "[Step #3] You've completed your inspections of the first two fields. Now, find the third field listed on Asma's Map. To read the map, right click on it in your inventory and /use it.";
					case 4:
						return "[Step #4] You've taken a look at the three fields Farmer Asma asked you to inspect. Return to her in the camp near the Shrouded Isles portal, and tell her your findings.";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof(GreenerPastures)) == null)
				return;


			if (e == GamePlayerEvent.GiveItem)
			{
				if (Step == 4)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs)args;
					if (gArgs.Target == farmerAsma && gArgs.Item.QuestName == Name && gArgs.Item.Name == farmerAsmasMap.Name)
					{
						RemoveItemFromPlayer(farmerAsma, gArgs.Item);

						farmerAsma.TurnTo(m_questPlayer);
						farmerAsma.SayTo(m_questPlayer, "Thank you for you help. I can only offer you a small bit of coin as a reward for your assistance, but I am grateful for your advice.");

						FinishQuest();
						return;
					}
				}
			}
		}

		public override void FinishQuest()
		{
			//Give reward to player here ...
			m_questPlayer.GainExperience(m_questPlayer.Level * 10, 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 35 + m_questPlayer.Level), "You are awarded " + (35 + m_questPlayer.Level) + " copper!");

			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.UseSlot, new DOLEventHandler(PlayerUseSlot));
			GameEventMgr.RemoveHandler(m_questPlayer, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
