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
 * Author:		Gandulf Kohlweiss
 * Date:
 * Directory: /scripts/quests/
 *
 * Description:
 *  Brief Walkthrough:
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using DOL.Database;
using DOL.Events;
using DOL.GS.Behaviour;
using DOL.GS.PacketHandler;
using log4net;

/* I suggest you declare yourself some namespaces for your quests
 * Like: DOL.GS.Quests.Albion
 *       DOL.GS.Quests.Midgard
 *       DOL.GS.Quests.Hibernia
 * Also this is the name that will show up in the database as QuestName
 * so setting good values here will result in easier to read and cleaner
 * Database Code
 */

namespace DOL.GS.Quests
{

	/// <summary>
	/// BaseQuest provides some helper classes for writing quests and
	/// integrates a new QuestPart Based QuestSystem.
	/// </summary>
	public abstract class BaseQuest : AbstractQuest
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


		/// <summary>
		/// Global Constant for all quests to define wether npcs and items should be saved in db or not.
		/// </summary>
		public static bool SAVE_INTO_DATABASE = ServerProperties.Properties.SAVE_QUEST_MOBS_INTO_DATABASE;

		public static Queue m_sayTimerQueue = new Queue();
		public static Queue m_sayObjectQueue = new Queue();
		public static Queue m_sayMessageQueue = new Queue();
		public static Queue m_sayChatTypeQueue = new Queue();
		public static Queue m_sayChatLocQueue = new Queue();

		public Queue m_animEmoteTeleportTimerQueue = new Queue();
		public Queue m_animEmoteObjectQueue = new Queue();

		public Queue m_animSpellTeleportTimerQueue = new Queue();
		public Queue m_animSpellObjectQueue = new Queue();

		public Queue m_portTeleportTimerQueue = new Queue();
		public Queue m_portObjectQueue = new Queue();
		public Queue m_portDestinationQueue = new Queue();

		// /// <summary>
		// /// List of all QuestParts that can be fired on interact Events.
		// /// </summary>
		//private static IDictionary interactQuestParts = new HybridDictionary();

		/// <summary>
		/// Create an empty Quest
		/// </summary>
		public BaseQuest()
			: base()
		{
		}

		/// <summary>
		/// Constructs a new empty Quest
		/// </summary>
		public BaseQuest(GamePlayer questingPlayer)
			: base(questingPlayer)
		{
		}

		/// <summary>
		/// Constructs a new Quest
		/// </summary>
		/// <param name="questingPlayer">The player doing this quest</param>
		/// <param name="step">The current step the player is on</param>
		public BaseQuest(GamePlayer questingPlayer, int step)
			: base(questingPlayer, step)
		{
		}

		/// <summary>
		/// Constructs a new Quest from a database Object
		/// </summary>
		/// <param name="questingPlayer">The player doing the quest</param>
		/// <param name="dbQuest">The database object</param>
		public BaseQuest(GamePlayer questingPlayer, DBQuest dbQuest)
			: base(questingPlayer, dbQuest)
		{
		}


		[ScriptUnloadedEvent]
		public static void ScriptUnloadedBase(DOLEvent e, object sender, EventArgs args)
		{
			if (questParts != null)
			{
				for (int i = questParts.Count - 1; i >= 0; i--)
				{
					RemoveBehaviour((QuestBehaviour)questParts[i]);
				}
			}
			questParts = null;
		}

		// Base QuestPart methods

		/// <summary>
		/// Remove all registered handlers for this quest,
		/// this will not remove the questPart from the quest.
		/// </summary>
		/// <param name="questPart">QuestPart to remove handlers from</param>
		protected static void UnRegisterBehaviour(QuestBehaviour questPart)
		{
			if (questPart.Triggers == null)
				return;

			foreach (IBehaviourTrigger trigger in questPart.Triggers)
			{
				trigger.Unregister();
			}
		}
		/// <summary>
		/// Adds the given questpart to the quest depending on the added triggers it will either
		/// be added as InteractQuestPart as NotifyQuestPart or both and also register the needed event handler.
		/// </summary>
		/// <param name="questPart">QuestPart to be added</param>
		public static void AddBehaviour(QuestBehaviour questPart)
		{
			if (questParts == null)
				questParts = new ArrayList();

			if (!questParts.Contains(questPart))
				questParts.Add(questPart);
			
			questPart.ID = questParts.Count; // fake id but ids only have to be unique quest wide its enough to use the number in the list as id.
		}

		/// <summary>
		/// Remove the given questpart from the quest and also unregister the handlers
		/// </summary>
		/// <param name="questPart">QuestPart to be removed</param>
		public static void RemoveBehaviour(QuestBehaviour questPart)
		{
			if (questParts == null)
				return;

			UnRegisterBehaviour(questPart);
			questParts.Remove(questPart);
		}

		/// <summary>
		/// Quest internal Notify method only fires if player already has the quest assigned
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (sender is GamePlayer && e == GameObjectEvent.InteractWith)
			{
				InteractWithEventArgs iArgs = args as InteractWithEventArgs;
				if (iArgs.Target is GameStaticItem)
				{
					InteractWithObject(sender as GamePlayer, iArgs.Target as GameStaticItem);
					return;
				}
			}

			if (questParts == null)
				return;

			foreach (QuestBehaviour questPart in questParts)
			{
				questPart.Notify(e, sender, args);
			}
		}

		#region Items

		protected static void RemoveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			RemoveItem(null, player, itemTemplate, true);
		}

		protected static void RemoveItem(GamePlayer player, ItemTemplate itemTemplate, bool notify)
		{
			RemoveItem(null, player, itemTemplate, notify);
		}

		protected static void RemoveItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate)
		{
			RemoveItem(target, player, itemTemplate, true);
		}

		protected static void ReplaceItem(GamePlayer target, ItemTemplate itemTemplateOut, ItemTemplate itemTemplateIn)
		{
			target.Inventory.BeginChanges();
			RemoveItem(target, itemTemplateOut, false);
			GiveItem(target, itemTemplateIn);
			target.Inventory.CommitChanges();
		}

		protected static void RemoveItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate, bool notify)
		{
			if (itemTemplate == null)
			{
				log.Error("itemtemplate is null in RemoveItem:" + Environment.StackTrace);
				return;
			}
			lock (player.Inventory)
			{
				InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				if (item != null)
				{
					player.Inventory.RemoveItem(item);
					if (target != null)
					{
						player.Out.SendMessage("You give the " + itemTemplate.Name + " to " + target.GetName(0, false), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				else if (notify)
				{
					player.Out.SendMessage("You cannot remove the \"" + itemTemplate.Name + "\" because you don't have it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		protected static int RemoveAllItem(GameLiving target, GamePlayer player, ItemTemplate itemTemplate, bool notify)
		{
			int itemsRemoved = 0;

			if (itemTemplate == null)
			{
				log.Error("itemtemplate is null in RemoveItem:" + Environment.StackTrace);
				return 0;
			}
			lock (player.Inventory)
			{
				InventoryItem item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

				while (item != null)
				{
					player.Inventory.RemoveItem(item);
					itemsRemoved++;
					item = player.Inventory.GetFirstItemByID(itemTemplate.Id_nb, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				}

				if (notify)
				{
					if (itemsRemoved == 0)
					{
						player.Out.SendMessage("You cannot remove the \"" + itemTemplate.Name + "\" because you don't have it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else if (target != null)
					{
						if (itemTemplate.Name.EndsWith("s"))
						{
							player.Out.SendMessage("You give the " + itemTemplate.Name + " to " + target.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							player.Out.SendMessage("You give the " + itemTemplate.Name + "'s to " + target.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
				}
			}

			return itemsRemoved;
		}
		#endregion

		protected static int MakeSaySequence(RegionTimer callingTimer)
		{
			m_sayTimerQueue.Dequeue();
			GamePlayer player = (GamePlayer)m_sayObjectQueue.Dequeue();
			String message = (String)m_sayMessageQueue.Dequeue();
			eChatType chatType = (eChatType)m_sayChatTypeQueue.Dequeue();
			eChatLoc chatLoc = (eChatLoc)m_sayChatLocQueue.Dequeue();

			player.Out.SendMessage(message, chatType, chatLoc);

			return 0;
		}


		protected void SendSystemMessage(String msg)
		{
			SendSystemMessage(m_questPlayer, msg);
		}

		protected void SendEmoteMessage(String msg)
		{
			SendEmoteMessage(m_questPlayer, msg, 0);
		}

		protected static void SendSystemMessage(GamePlayer player, String msg)
		{
			SendEmoteMessage(player, msg, 0);
		}

		protected static void SendSystemMessage(GamePlayer player, String msg, uint delay)
		{
			SendMessage(player, msg, delay, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		protected static void SendEmoteMessage(GamePlayer player, String msg)
		{
			SendEmoteMessage(player, msg, 0);
		}

		protected static void SendEmoteMessage(GamePlayer player, String msg, uint delay)
		{
			SendMessage(player, msg, delay, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
		}

		protected static void SendReply(GamePlayer player, String msg)
		{
			SendMessage(player, msg, 0, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
		}

		protected static void SendMessage(GamePlayer player, String msg, uint delay, eChatType chatType, eChatLoc chatLoc)
		{
			if (delay == 0)
				player.Out.SendMessage(msg, chatType, chatLoc);
			else
			{
				m_sayMessageQueue.Enqueue(msg);
				m_sayObjectQueue.Enqueue(player);
				m_sayChatLocQueue.Enqueue(chatLoc);
				m_sayChatTypeQueue.Enqueue(chatType);
				m_sayTimerQueue.Enqueue(new RegionTimer(player, new RegionTimerCallback(MakeSaySequence), (int)delay * 100));
			}
		}

		protected static bool TryGiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			return GiveItem(null, player, itemTemplate, false);
		}

		protected static bool TryGiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
		{
			return GiveItem(source, player, itemTemplate, false);
		}

		protected static bool GiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			return GiveItem(null, player, itemTemplate, true);
		}

		protected static bool GiveItem(GamePlayer player, ItemTemplate itemTemplate, bool canDrop)
		{
			return GiveItem(null, player, itemTemplate, canDrop);
		}

		protected static bool GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate)
		{
			return GiveItem(source, player, itemTemplate, true);
		}

		protected static bool GiveItem(GameLiving source, GamePlayer player, ItemTemplate itemTemplate, bool canDrop)
		{
			InventoryItem item = null;

			if (itemTemplate is ItemUnique)
			{
				GameServer.Database.AddObject(itemTemplate as ItemUnique);
				item = new InventoryItem(itemTemplate as ItemUnique);
			}
			else
			{
				item = new InventoryItem(itemTemplate);
			}

			if (!player.ReceiveItem(source, item))
			{
				if (canDrop)
				{
					player.CreateItemOnTheGround(item);
					player.Out.SendMessage(String.Format("Your backpack is full, {0} is dropped on the ground.", itemTemplate.Name), eChatType.CT_Important, eChatLoc.CL_PopupWindow);
				}
				else
				{
					player.Out.SendMessage("Your backpack is full!", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
					return false;
				}
			}

			return true;
		}

		protected static ItemTemplate CreateTicketTo(String destination, String ticket_Id)
		{
			ItemTemplate ticket = GameServer.Database.FindObjectByKey<ItemTemplate>(GameServer.Database.Escape(ticket_Id.ToLower()));
			if (ticket == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Could not find " + destination + ", creating it ...");

				ticket = new ItemTemplate();
				ticket.Name = "ticket to " + destination;

				ticket.Id_nb = ticket_Id.ToLower();

				ticket.Model = 499;

				ticket.Object_Type = (int)eObjectType.GenericItem;
				ticket.Item_Type = 40;

				ticket.IsPickable = true;
				ticket.IsDropable = true;

				ticket.Price = Money.GetMoney(0,0,0,5,3);

				ticket.PackSize = 1;
				ticket.Weight = 0;

				GameServer.Database.AddObject(ticket);
			}
			return ticket;
		}

		//timer callbacks
		protected virtual int MakeAnimSpellSequence(RegionTimer callingTimer)
		{
			if (m_animSpellTeleportTimerQueue.Count > 0)
			{
				m_animSpellTeleportTimerQueue.Dequeue();
				GameLiving animObject = (GameLiving)m_animSpellObjectQueue.Dequeue();
				foreach (GamePlayer player in animObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendSpellCastAnimation(animObject, 1, 20);
				}
			}
			return 0;
		}

		protected virtual int MakeAnimEmoteSequence(RegionTimer callingTimer)
		{
			if (m_animEmoteTeleportTimerQueue.Count > 0)
			{
				m_animEmoteTeleportTimerQueue.Dequeue();
				GameLiving animObject = (GameLiving)m_animEmoteObjectQueue.Dequeue();
				foreach (GamePlayer player in animObject.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendEmoteAnimation(animObject, eEmote.Bind);
				}
			}
			return 0;
		}

		protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location)
		{
			TeleportTo(target, caster, location, 0, 0);
		}

		protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location, uint delay)
		{
			TeleportTo(target, caster, location, delay, 0);
		}

		protected virtual void TeleportTo(GameObject target, GameObject caster, GameLocation location, uint delay, int fuzzyLocation)
		{
			delay *= 100; // 1/10sec to milliseconds
			if (delay <= 0)
				delay = 1;
			m_animSpellObjectQueue.Enqueue(caster);
			m_animSpellTeleportTimerQueue.Enqueue(new RegionTimer(caster, new RegionTimerCallback(MakeAnimSpellSequence), (int)delay));

			m_animEmoteObjectQueue.Enqueue(target);
			m_animEmoteTeleportTimerQueue.Enqueue(new RegionTimer(target, new RegionTimerCallback(MakeAnimEmoteSequence), (int)delay + 2000));

			m_portObjectQueue.Enqueue(target);

			location.X += Util.Random(0 - fuzzyLocation, fuzzyLocation);
			location.Y += Util.Random(0 - fuzzyLocation, fuzzyLocation);

			m_portDestinationQueue.Enqueue(location);
			m_portTeleportTimerQueue.Enqueue(new RegionTimer(target, new RegionTimerCallback(MakePortSequence), (int)delay + 3000));

			if (location.Name != null)
			{
				m_questPlayer.Out.SendMessage(target.Name + " is being teleported to " + location.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

		}

		protected virtual int MakePortSequence(RegionTimer callingTimer)
		{
			if (m_portTeleportTimerQueue.Count > 0)
			{
				m_portTeleportTimerQueue.Dequeue();
				GameObject gameObject = (GameObject)m_portObjectQueue.Dequeue();
				GameLocation location = (GameLocation)m_portDestinationQueue.Dequeue();
				gameObject.MoveTo(location.RegionID, location.X, location.Y, location.Z, location.Heading);
			}
			return 0;
		}

		#region World Item Interaction

		protected struct QuestStepInteraction
		{
			public string objectName;
			public int numRequired;
			public ItemTemplate itemResult;
			public string interactText;
		}

		Dictionary<int, QuestStepInteraction> m_interactions = new Dictionary<int, QuestStepInteraction>();
		const int INTERACT_ITEM_RESPAWN_SECONDS = 120;

		/// <summary>
		/// Add an interact item associated with a step for this quest
		/// </summary>
		/// <param name="step">What step is this item valid for</param>
		/// <param name="staticObjectName">the name of the static item to interact with</param>
		/// <param name="numRequired">How many times to interact before this step is complete</param>
		/// <param name="itemResult">What item is given to the player when interacting</param>
		/// <param name="interactText">Text presented to player when interacting with the object</param>
		protected void AddInteractStep(int step, string objectName, int numRequired, ItemTemplate itemResult, string interactText)
		{
			try
			{
				QuestStepInteraction info = new QuestStepInteraction();
				info.objectName = objectName;
				info.numRequired = numRequired;
				info.itemResult = itemResult;
				info.interactText = interactText;

				m_interactions.Add(step, info);
			}
			catch (Exception ex)
			{
				log.Error("Error adding Interact Step, possible duplicate?", ex);
			}
		}

		/// <summary>
		/// We are interacting with an object, check to see if this quest and step needs to respond
		/// </summary>
		/// <param name="player"></param>
		/// <param name="staticItem"></param>
		protected void InteractWithObject(GamePlayer player, GameStaticItem staticItem)
		{
			if (m_interactions.Count > 0)
			{
				if (m_interactions.ContainsKey(Step))
				{
					QuestStepInteraction info = m_interactions[Step];

					if (staticItem.Name == info.objectName)
					{
						if (GiveItem(player, info.itemResult, false))
						{
							player.Out.SendMessage(info.interactText, eChatType.CT_System, eChatLoc.CL_SystemWindow);
							staticItem.RemoveFromWorld(INTERACT_ITEM_RESPAWN_SECONDS);
							OnObjectInteract(info);
						}
					}
				}
			}
		}

		/// <summary>
		/// When an object is interacted with this message is sent after world item is removed and inventory item added
		/// </summary>
		/// <param name="info"></param>
		protected virtual void OnObjectInteract(QuestStepInteraction info)
		{
			// this is needed in order to support both Base and Reward quests
			log.Error("Override OnObjectInteract to advance goal progress");
		}

		#endregion World Item Interaction

	}
}
