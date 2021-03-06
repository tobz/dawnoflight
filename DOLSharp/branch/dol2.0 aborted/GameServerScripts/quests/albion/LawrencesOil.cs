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
 * 1) Travel to loc=10695,30110 Camelot Hills (Cotswold Village) to speak with Brother Lawrence
 * 2) Go to loc=12288,36480 Camelot Hills (near the housing zone) and kill river spriteling until your flask will be full
 * 2) Came back to Cotswold Village and give your filled flask to Brother Lawrence to have your reward
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
	public class LawrencesOilDescriptor : AbstractQuestDescriptor
	{
		/* This is the type of the quest class linked with 
		 * this requirement class, you must override the 
		 * base methid like that
		 */
		public override Type LinkedQuestType
		{
			get { return typeof(LawrencesOil); }
		}

		/* This value is used to retrieves the minimum level needed
		 *  to be able to make this quest. Override it only if you need, 
		 * the default value is 1
		 */
		public override int MinLevel
		{
			get { return 4; }
		}

		/* This value is used to retrieves how maximum level needed
		 * to be able to make this quest. Override it only if you need, 
		 * the default value is 50
		 */
		public override int MaxLevel
		{
			get { return 7; }
		}
	}

	/* The second thing we do, is to declare the class we create
	 * as Quest. We must make it persistant using attributes, to
	 * do this, we derive from the abstract class AbstractQuest
	 */
	[Subclass(NameType = typeof(LawrencesOil), ExtendsType = typeof(AbstractQuest))]
	public class LawrencesOil : BaseQuest
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
		protected const string questTitle = "Lawrence's Oil";

		private static GameMob brotherLawrence = null;

		private static GenericItemTemplate lawrencesEmptyFlask = null;
		private static GenericItemTemplate lawrencesFilledFlask= null;

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

			brotherLawrence = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Brother Lawrence") as GameMob;
			if (brotherLawrence == null)
			{
				brotherLawrence = new GameMob();
				brotherLawrence.Model = 32;
				brotherLawrence.Name = "Brother Lawrence";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + brotherLawrence.Name + ", creating him ...");
				brotherLawrence.GuildName = "Part of " + questTitle + " Quest";
				brotherLawrence.Realm = (byte) eRealm.Albion;
				brotherLawrence.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.RightHandWeapon, new NPCWeapon(14, 20, 0));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(98, 44, 0));
				brotherLawrence.Inventory = template;
				brotherLawrence.SwitchWeapon(GameLiving.eActiveWeaponSlot.Standard);

				brotherLawrence.Size = 54;
				brotherLawrence.Level = 29;
				brotherLawrence.Position = new Point(559556, 513431, 2568);
				brotherLawrence.Heading = 2082;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = brotherLawrence;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				brotherLawrence.OwnBrain = newBrain;

				if(!brotherLawrence.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(brotherLawrence);
			}

			#endregion

			#region defineItems

			// item db check
			lawrencesEmptyFlask = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Lawrence's Empty Flask")) as GenericItemTemplate;
			if (lawrencesEmptyFlask == null)
			{
				lawrencesEmptyFlask = new GenericItemTemplate();
				lawrencesEmptyFlask.Name = "Lawrence's Empty Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+lawrencesEmptyFlask.Name+", creating it ...");
				
				lawrencesEmptyFlask.Level = 0;
				lawrencesEmptyFlask.Weight = 1;
				lawrencesEmptyFlask.Model = 490;
				
				lawrencesEmptyFlask.IsDropable = false;
				lawrencesEmptyFlask.IsSaleable = false;
				lawrencesEmptyFlask.IsTradable = false;


				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(lawrencesEmptyFlask);
			}

			// item db check
			lawrencesFilledFlask = GameServer.Database.SelectObject(typeof (GenericItemTemplate), Expression.Eq("Name", "Lawrence's Filled Flask")) as GenericItemTemplate;
			if (lawrencesFilledFlask == null)
			{
				lawrencesFilledFlask = new GenericItemTemplate();
				lawrencesFilledFlask.Name = "Lawrence's Filled Flask";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+lawrencesFilledFlask.Name+", creating it ...");
				
				lawrencesFilledFlask.Level = 0;
				lawrencesFilledFlask.Weight = 1;
				lawrencesFilledFlask.Model = 490;
				
				lawrencesFilledFlask.IsDropable = false;
				lawrencesFilledFlask.IsSaleable = false;
				lawrencesFilledFlask.IsTradable = false;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(lawrencesFilledFlask);
			}

			#endregion


			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			
			GameEventMgr.AddHandler(brotherLawrence, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrotherLawrence));
			GameEventMgr.AddHandler(brotherLawrence, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrotherLawrence));
			
			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Ydenia the possibility to give this quest to players */
			QuestMgr.AddQuestDescriptor(brotherLawrence, typeof(LawrencesOilDescriptor));

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
			/* If Elvar Ironhand has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (brotherLawrence == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			
			GameEventMgr.RemoveHandler(brotherLawrence, GameObjectEvent.Interact, new DOLEventHandler(TalkToBrotherLawrence));
			GameEventMgr.RemoveHandler(brotherLawrence, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToBrotherLawrence));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to Ydenia the possibility to give this quest to players */
			QuestMgr.RemoveQuestDescriptor(brotherLawrence, typeof(LawrencesOilDescriptor));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToBrotherLawrence(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

			if (QuestMgr.CanGiveQuest(typeof(LawrencesOil), player, brotherLawrence) <= 0)
				return;

			//We also check if the player is already doing the quest
			LawrencesOil quest = player.IsDoingQuest(typeof (LawrencesOil)) as LawrencesOil;

			brotherLawrence.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					brotherLawrence.SayTo(player, "Greetings. My name is Brother Lawrence, and I'm the head healer here in Cotswold. It's my responsibility to keep the townspeople in good health and ward off the plague.  People come to me with everything from cuts and rashes to more [serious] ailments.");
					return;
				}
				else
				{
					if (quest.Step == 4)
					{
						brotherLawrence.SayTo(player, "Welcome back, "+player.CharacterClass.Name+". I've almost finished making my preparations for the demonstration. May I have the flask of oil?");
					}
					return;
				}
			}
				// The player whispered to NPC (clicked on the text inside the [])
			else if (e == GameLivingEvent.WhisperReceive)
			{
				WhisperReceiveEventArgs wArgs = (WhisperReceiveEventArgs) args;
				if (quest == null)
				{
					//Do some small talk :)
					switch (wArgs.Text)
					{
						case "serious":
							brotherLawrence.SayTo(player, "Even in a small community like Cotswold, I'm almost always busy.  The Church teaches us to use prayers and magic to speed the natural healing process, but that requires a lot of energy. It's not uncommon for healers to be worn out at the end of the [day].");
							break;

							//If the player offered his help, we send the quest dialog now!
						case "day":
							brotherLawrence.SayTo(player, "It's exhausting work, but I believe I've found my calling.  I think I may have discovered a way to help deal with minor injuries and preserve the use of magic for more serious cases. Are you willing to help me prepare for a demonstration of my methods?");
							QuestMgr.ProposeQuestToPlayer(typeof(LawrencesOil), "Will you help Brother Lawrence gather \nthe oil he needs for his demonstration? \n[Levels 4-7]", player, brotherLawrence);
							break;
					}
				}
				else
				{
					switch (wArgs.Text)
					{
						case "healing":
							if(quest.Step == 1)
							{
								brotherLawrence.SayTo(player, "So far, I've used it as the base for a variety of salves and other treatments. It's proven very valuable in healing minor injuries, rashes and infections.  If we work quickly, I can have all my supplies ready in time for the [demonstration].");
							}
							break;

						case "demonstration":
							if(quest.Step == 1)
							{
								brotherLawrence.SayTo(player, "Here's a flask to store the oil in. Killing two of the river spritelings should provide enough oil for the demonstration and the next week's use. To find the spritelings, cross the bridge toward Camelot, but turn south before you get to the gates. Continue following the west bank of the river to the south, and you should see the spritelings before you come to the entrance to the Housing areas.");
								GiveItemToPlayer(brotherLawrence, CreateQuestItem(lawrencesEmptyFlask, quest), player);
								quest.ChangeQuestStep(2);
							}
							break;	

						case "methods":
							if(quest.Step == 5)
							{
								brotherLawrence.SayTo(player, "Here's a bit of copper for your trouble. I wish I could offer more, but times are tough right now. If you are ever in need of healing, please don't hesitate to visit me.");
								quest.FinishQuest();
							}
							break;
					}
				}
			}
		}

		/* This is our callback hook that will be called when the player clicks
		 * on any button in the quest offer dialog. We check if he accepts or
		 * declines here...
		 */
		protected static void QuestDialogResponse(DOLEvent e, object sender, EventArgs args)
		{
			QuestEventArgs gArgs = args as QuestEventArgs;

			if (gArgs != null && gArgs.QuestType.Equals(typeof(LawrencesOil)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(LawrencesOil), player, gArgs.Source as GameNPC))
					{
						player.Out.SendMessage("Thank you for agreeing to help! This should make things go much more smoothly. I was concerned I might not have enough time to gather all the materials. I've found that the oil from river sprites and spritelings is a very versatile compound for [healing].", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					}
				}
				else if (e == GamePlayerEvent.DeclineQuest)
				{
					player.Out.SendMessage("Oh well, if you change your mind, please come back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
						return "[Step #1] Continue speaking with Brother Lawrence about his use of spriteling oil in his [healing] duties.";
					case 2:
						return "[Step #2] Search for river spritelings by crossing the bridge from Cotswold toward Camelot and heading south before you get to the city gates. Kill two of the spritelings and gather their oil in the flask provided by Brother Lawrence.";
					case 3:
						return "[Step #3] Search for river spritelings by crossing the bridge from Cotswold toward Camelot and heading south before you get to the city gates. Kill two of the spritelings and gather their oil in the flask provided by Brother Lawrence.";
					case 4:
						return "[Step #4] Return to Brother Lawrence with the filled flask and hand it to him when he asks."; 
					case 5:
						return "[Step #5] Continue speaking with Brother Lawrence about the demonstration of his healing [methods].";
					default:
						return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (LawrencesOil)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				if(Step == 2)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "river spriteling")
					{
						if (Util.Chance(50))
						{
							player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You gather oil from the spriteling in Brother \nLawrence's Flask. Your journal \nhas been updated.");
				
							GenericItem item = player.Inventory.GetFirstItemByName(lawrencesEmptyFlask.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
							if (item != null)
							{
								item.Name = "Lawrence's Flask (Half Full)";
								player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});
						
								ChangeQuestStep(3);
							}
						}
						return;
					}
				}
				else if(Step == 3)
				{
					EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
					if (gArgs.Target.Name == "river spriteling")
					{
						if (Util.Chance(50))
						{
							player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0x00, 0x00, 0x00, 0x00, eDialogType.Ok, true, "You gather oil from the spriteling in Brother \nLawrence's Flask. Your journal \nhas been updated.");
							RemoveItemFromPlayer(player.Inventory.GetFirstItemByName(lawrencesEmptyFlask.Name, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack));
							GiveItemToPlayer(CreateQuestItem(lawrencesFilledFlask));
							ChangeQuestStep(4);
						}
						return;
					}
				}
			}
			else if (e == GamePlayerEvent.GiveItem)
			{
				if(Step == 4)
				{
					GiveItemEventArgs gArgs = (GiveItemEventArgs) args;
					if (gArgs.Target == brotherLawrence && gArgs.Item.QuestName == Name && gArgs.Item.Name == lawrencesFilledFlask.Name)
					{
						RemoveItemFromPlayer(brotherLawrence, gArgs.Item);

						brotherLawrence.TurnTo(m_questPlayer);
						brotherLawrence.SayTo(m_questPlayer, "Thank you for retrieving this. You've been a tremendous help to me in preparing for this visit.  With a little luck, perhaps soon all the Church's healers will be using my [methods].");
						ChangeQuestStep(5);
					}
				}
			}
		}

		public override void FinishQuest()
		{
			//Give reward to player here ...
			m_questPlayer.GainExperience((long)(m_questPlayer.ExperienceForNextLevel/15.5), 0, 0, true);
			m_questPlayer.AddMoney(Money.GetMoney(0, 0, 0, 0, 67), "You are awarded 67 copper!");
		
			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}
