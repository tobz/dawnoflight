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
 * 1) Travel to loc=22723,48005 Camelot Hills (Prydwen Keep) to speak with Hugh Gallen
 * 2) Go to loc=17500,45153 Camelot Hills and kill some punny skeleton until Mulgrut pop
 * 2) Kill Mulgrut to have your reward
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
    public class ClericMulgrutDescriptor : AbstractQuestDescriptor
    {
        /* This is the type of the quest class linked with 
         * this requirement class, you must override the 
         * base methid like that
         */
        public override Type LinkedQuestType
        {
            get { return typeof(ClericMulgrut); }
        }

        /* This value is used to retrieves the minimum level needed
         *  to be able to make this quest. Override it only if you need, 
         * the default value is 1
         */
        public override int MinLevel
        {
            get { return 5; }
        }

        /* This value is used to retrieves how maximum level needed
         * to be able to make this quest. Override it only if you need, 
         * the default value is 50
         */
        public override int MaxLevel
        {
            get { return 10; }
        }
    }

    /* The second thing we do, is to declare the class we create
     * as Quest. We must make it persistant using attributes, to
     * do this, we derive from the abstract class AbstractQuest
     */
    [Subclass(NameType = typeof(ClericMulgrut), ExtendsType = typeof(AbstractQuest))]
	public class ClericMulgrut : BaseQuest
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
		protected const string questTitle = "Cleric Mulgrut";

		private static GameNPC hughGallen = null;
		private static GameNPC mulgrutMaggot = null;
		
		private static WaistTemplate beltOfAnimation = null;
		
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

			hughGallen = ResearchQuestObject(typeof(GameMob), WorldMgr.GetRegion(1), eRealm.Albion, "Hugh Gallen") as GameMob;
			if (hughGallen == null)
			{
				hughGallen = new GameMob();
				hughGallen.Model = 40;
				hughGallen.Name = "Hugh Gallen";
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + hughGallen.Name + ", creating him ...");
				hughGallen.GuildName = "Part of " + questTitle + " Quest";
				hughGallen.Realm = (byte) eRealm.Albion;
				hughGallen.Region = WorldMgr.GetRegion(1);

				GameNpcInventory template = new GameNpcInventory();
				template.AddItem(eInventorySlot.HandsArmor, new NPCArmor(39));
				template.AddItem(eInventorySlot.FeetArmor, new NPCArmor(40));
				template.AddItem(eInventorySlot.TorsoArmor, new NPCArmor(36));
				template.AddItem(eInventorySlot.LegsArmor, new NPCArmor(37));
				template.AddItem(eInventorySlot.ArmsArmor, new NPCArmor(38));
				hughGallen.Inventory = template;

				hughGallen.Size = 49;
				hughGallen.Level = 38;
				hughGallen.Position = new Point(574640, 531109, 2896);
				hughGallen.Heading = 2275;

				StandardMobBrain newBrain = new StandardMobBrain();
				newBrain.Body = hughGallen;
				newBrain.AggroLevel = 100;
				newBrain.AggroRange = 0;
				hughGallen.OwnBrain = newBrain;

				if(!hughGallen.AddToWorld())
				{
					if (log.IsWarnEnabled)
						log.Warn("Quest "+questTitle+" abort because a needed region is not in use in this server!");
					return;
				}

				//You don't have to store the created mob in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(hughGallen);
			}

			#endregion

			#region defineItems

			// item db check
			beltOfAnimation = GameServer.Database.SelectObject(typeof (WaistTemplate), Expression.Eq("Name", "Belt of Animation")) as WaistTemplate;
			if (beltOfAnimation == null)
			{
				beltOfAnimation = new WaistTemplate();
				beltOfAnimation.Name = "Belt of Animation";
				if (log.IsWarnEnabled)
					log.Warn("Could not find "+beltOfAnimation.Name+", creating it ...");
				
				beltOfAnimation.Level = 5;
				beltOfAnimation.Weight = 3;
				beltOfAnimation.Model = 597;
				
                beltOfAnimation.IsDropable = false;
                beltOfAnimation.IsSaleable = false;
                beltOfAnimation.IsTradable = false;

                beltOfAnimation.MagicalBonus.Add(new ItemMagicalBonus(eProperty.MaxHealth, 6));
				
				beltOfAnimation.Quality = 100;

				//You don't have to store the created item in the db if you don't want,
				//it will be recreated each time it is not found, just comment the following
				//line if you rather not modify your database
				if (SAVE_INTO_DATABASE)
					GameServer.Database.AddNewObject(beltOfAnimation);
			}

			#endregion


			/* Now we add some hooks to the npc we found.
			* Actually, we want to know when a player interacts with him.
			* So, we hook the right-click (interact) and the whisper method
			* of npc and set the callback method to the "TalkToXXX"
			* method. This means, the "TalkToXXX" method is called whenever
			* a player right clicks on him or when he whispers to him.
			*/
			
			GameEventMgr.AddHandler(hughGallen, GameObjectEvent.Interact, new DOLEventHandler(TalkToHughGallen));
			GameEventMgr.AddHandler(hughGallen, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHughGallen));

			/* Now we add some hooks to trigger the quest dialog reponse. */
			GameEventMgr.AddHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.AddHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we bring to Yetta Fletcher the possibility to give this quest to players */
            QuestMgr.AddQuestDescriptor(hughGallen, typeof(ClericMulgrutDescriptor));

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
			/* If Yetta Fletcher has not been initialized, then we don't have to remove any
			 * hooks from him ;-)
			 */
			if (hughGallen == null)
				return;

			/* Removing hooks works just as adding them but instead of 
			 * AddHandler, we call RemoveHandler, the parameters stay the same
			 */
			GameEventMgr.RemoveHandler(hughGallen, GameObjectEvent.Interact, new DOLEventHandler(TalkToHughGallen));
			GameEventMgr.RemoveHandler(hughGallen, GameLivingEvent.WhisperReceive, new DOLEventHandler(TalkToHughGallen));

			GameEventMgr.RemoveHandler(GamePlayerEvent.AcceptQuest, new DOLEventHandler(QuestDialogResponse));
			GameEventMgr.RemoveHandler(GamePlayerEvent.DeclineQuest, new DOLEventHandler(QuestDialogResponse));

			/* Now we remove to Yetta Fletcher the possibility to give this quest to players */
            QuestMgr.RemoveQuestDescriptor(hughGallen, typeof(ClericMulgrutDescriptor));
		}

		/* This is the method we declared as callback for the hooks we set to
		 * NPC. It will be called whenever a player right clicks on NPC
		 * or when he whispers something to him.
		 */

		protected static void TalkToHughGallen(DOLEvent e, object sender, EventArgs args)
		{
			//We get the player from the event arguments and check if he qualifies		
			GamePlayer player = ((SourceEventArgs) args).Source as GamePlayer;
			if (player == null)
				return;

            if (QuestMgr.CanGiveQuest(typeof(ClericMulgrut), player, hughGallen) <= 0)
				return;

			//We also check if the player is already doing the quest
			ClericMulgrut quest = player.IsDoingQuest(typeof (ClericMulgrut)) as ClericMulgrut;

			hughGallen.TurnTo(player);
			//Did the player rightclick on NPC?
			if (e == GameObjectEvent.Interact)
			{
				if (quest == null)
				{
					//Player is not doing the quest...
					hughGallen.SayTo(player, "I have a [bit of information] you might be interested in should you wish to hear it.");
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
						case "bit of information":
							hughGallen.SayTo(player, "Listen close! Lord Prydwen once had a faithful cleric known as Mulgrut. His service to our realm was unparalleled, yet, during Arthur's siege on Lancelot, Mulgrut's son Durren [was slain].");
							break;

						case "was slain":
							hughGallen.SayTo(player, "Yes! Mulgrut [never recovered] from that. He turned his eyes from God and never looked back.");
							break;

						case "never recovered":
							hughGallen.SayTo(player, "Aye! Even in death his soul never rests. If you are interested, I can tell you how to [make a profit] from this!");
							break;
						
							//If the player offered his help, we send the quest dialog now!
						case "make a profit":
							QuestMgr.ProposeQuestToPlayer(typeof(ClericMulgrut), "Do you accept the Cleric Mulgrut quest? \n[Levels 5-10]", player, hughGallen);
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

			if (gArgs != null && gArgs.QuestType.Equals(typeof(ClericMulgrut)))
			{
				GamePlayer player = gArgs.Player;
				if (player == null) return;

				if (e == GamePlayerEvent.AcceptQuest)
				{
					if (QuestMgr.GiveQuestToPlayer(typeof(ClericMulgrut), player, gArgs.Source as GameNPC))
					{
						player.Out.SendMessage("It is said that upon his death, he carried into the after life an item of great worth. Some say he still walks the cemetery not far from here!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
						return "[Step #1] Locate and slay Mulgrut for his magical item.  He can usually be found either at the graveyard near Prydwen Keep or wandering about Camelot Hills.";
                    default:
                        return "[Step #" + Step + "] No Description entered for this step!";
				}
			}
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || player.IsDoingQuest(typeof (ClericMulgrut)) == null)
				return;

			if (e == GameLivingEvent.EnemyKilled)
			{
				EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs) args;
				if(Step == 1)
				{
					if (gArgs.Target.Name == "puny skeleton")
					{
						if (Util.Chance(25))
						{
							if(mulgrutMaggot == null)
							{
								mulgrutMaggot = new GameMob();
								mulgrutMaggot.Model = 467;
								mulgrutMaggot.Name = "Mulgrut Maggot";
								mulgrutMaggot.Realm = (byte) eRealm.None;
								mulgrutMaggot.Region = WorldMgr.GetRegion(1);

								mulgrutMaggot.Size = 60;
								mulgrutMaggot.Level = 5;
								mulgrutMaggot.Position = new Point(565941, 528121, 2152);
								mulgrutMaggot.Heading = 2278;

								StandardMobBrain brain = new StandardMobBrain();  // set a brain witch find a lot mob friend to attack the player
								brain.Body = mulgrutMaggot;
								brain.AggroLevel = 100;
								brain.AggroRange = 2000;
								mulgrutMaggot.OwnBrain = brain;

								mulgrutMaggot.AddToWorld();
							}
						}
					}
					else if (gArgs.Target.Name == mulgrutMaggot.Name)
					{
						if(mulgrutMaggot != null) { mulgrutMaggot = null; }
						FinishQuest();
					}
				}
			}
		}

		public override void FinishQuest()
		{
			GiveItemToPlayer(beltOfAnimation.CreateInstance());

			base.FinishQuest(); //Defined in Quest, changes the state, stores in DB etc ...
		}
	}
}