using System;
using System.Collections.Generic;
using DawnOfLight.Database;
using DawnOfLight.GameServer.Housing;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.GameObjects.CustomNPC
{
    public class MarketExplorer : GameNPC, IGameInventoryObject
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string EXPLORER_ITEM_LIST = "MarketExplorerItems";

		public object LockObject()
		{
			return new object(); // not applicable for a Market Explorer
		}

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

			if (player.ActiveInventoryObject != null)
			{
				player.ActiveInventoryObject.RemoveObserver(player);
				player.ActiveInventoryObject = null;
			}

			if (ServerProperties.Properties.MARKET_ENABLE)
			{
				player.ActiveInventoryObject = this;
				player.Out.SendMarketExplorerWindow();
			}
			else
			{
				player.Out.SendMessage("Sorry, the market is not available at this time.", ChatType.CT_Staff, ChatLocation.CL_SystemWindow);
			}
            return true;
        }

		public virtual string GetOwner(GamePlayer player)
		{
			return player.InternalID;
		}

		public virtual Dictionary<int, InventoryItem> GetClientInventory(GamePlayer player)
		{
			return null; // we don't have any inventory
		}

		/// <summary>
		/// List of items in this objects inventory
		/// </summary>
		public virtual IList<InventoryItem> DBItems(GamePlayer player = null)
		{
			return MarketCache.Items;
		}

		/// <summary>
		/// First slot of the client window that shows this inventory
		/// </summary>
		public virtual int FirstClientSlot
		{
			get { return (int)InventorySlot.MarketExplorerFirst; }
		}

		/// <summary>
		/// Last slot of the client window that shows this inventory
		/// </summary>
		public virtual int LastClientSlot
		{
			get { return (int)InventorySlot.MarketExplorerFirst + 39; } // not really sure
		}

		/// <summary>
		/// First slot in the DB.
		/// </summary>
		public virtual int FirstDBSlot
		{
			get { return (int)InventorySlot.Consignment_First; } // not used
		}

		/// <summary>
		/// Last slot in the DB.
		/// </summary>
		public virtual int LastDBSlot
		{
			get { return (int)InventorySlot.Consignment_Last; } // not used
		}


		/// <summary>
		/// Search the MarketCache
		/// </summary>
		public virtual bool SearchInventory(GamePlayer player, MarketSearch.SearchData marketSearchQuery)
		{
			var marketSearch = new MarketSearch(player);
			List<InventoryItem> items = marketSearch.FindItemsInList(DBItems(), marketSearchQuery);

			if (items != null)
			{
				int maxPerPage = 20;
				byte maxPages = (byte)(Math.Ceiling(items.Count / (double)maxPerPage) - 1);
				int first = (marketSearchQuery.page) * maxPerPage;
				int last = first + maxPerPage;
				List<InventoryItem> list = new List<InventoryItem>();
				int index = 0;
				foreach (InventoryItem item in items)
				{
					if (index >= first && index <= last)
						list.Add(item);
					index++;
				}

				if ((int)marketSearchQuery.page == 0)
				{
					player.Out.SendMessage("Items returned: " + items.Count + ".", ChatType.CT_Important, ChatLocation.CL_SystemWindow);
				}

				if (items.Count == 0)	// No items returned, let the client know
				{
					player.Out.SendMarketExplorerWindow(list, 0, 0);
				}
				else if ((int)marketSearchQuery.page <= (int)maxPages)	//Don't let us tell the client about any more than the max pages
				{
					player.Out.SendMessage("Moving to page " + ((int)(marketSearchQuery.page + 1)) + ".", ChatType.CT_Important, ChatLocation.CL_SystemWindow);
					player.Out.SendMarketExplorerWindow(list, marketSearchQuery.page, maxPages);
				}

				// Save the last search list in case we buy an item from it
				player.TempProperties.setProperty(EXPLORER_ITEM_LIST, list);
			}


			return true;
		}

		/// <summary>
		/// Is this a move request for a market explorer
		/// </summary>
		/// <param name="player"></param>
		/// <param name="fromClientSlot"></param>
		/// <param name="toClientSlot"></param>
		/// <returns></returns>
		public virtual bool CanHandleMove(GamePlayer player, ushort fromClientSlot, ushort toClientSlot)
		{
			if (player == null || player.ActiveInventoryObject != this)
				return false;

			bool canHandle = false;

			if (fromClientSlot >= FirstClientSlot && toClientSlot >= (int)InventorySlot.FirstBackpack && toClientSlot <= (ushort)InventorySlot.LastBackpack)
			{
				// buy request
				canHandle = true;
			}

			return canHandle;
		}

		/// <summary>
		/// Move Item from MarketExplorer
		/// </summary>
		/// <param name="player"></param>
		/// <param name="fromClientSlot"></param>
		/// <param name="toClientSlot"></param>
		/// <returns></returns>
		public virtual bool MoveItem(GamePlayer player, ushort fromClientSlot, ushort toClientSlot)
		{
			// this move represents a buy item request
			if (fromClientSlot >= (ushort)InventorySlot.MarketExplorerFirst && 
				toClientSlot >= (ushort)InventorySlot.FirstBackpack && 
				toClientSlot <= (ushort)InventorySlot.LastBackpack &&
				player.ActiveInventoryObject == this)
			{
				var list = player.TempProperties.getProperty<List<InventoryItem>>(EXPLORER_ITEM_LIST, null);
				if (list == null)
				{
					return false;
				}

				int itemSlot = fromClientSlot - (int)InventorySlot.MarketExplorerFirst;

				InventoryItem item = list[itemSlot];

				BuyItem(item, player);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Add an item to this object
		/// </summary>
		public virtual bool OnAddItem(GamePlayer player, InventoryItem item)
		{
			return false;
		}

		/// <summary>
		/// Not applicable
		/// </summary>
		public virtual bool SetSellPrice(GamePlayer player, ushort clientSlot, uint price)
		{
			return false;
		}

		/// <summary>
		/// Remove an item from this object
		/// </summary>
		public virtual bool OnRemoveItem(GamePlayer player, InventoryItem item)
		{
			return false;
		}

		public virtual void BuyItem(InventoryItem item, GamePlayer player)
        {
			GameConsignmentMerchant cm = HouseMgr.GetConsignmentByHouseNumber((int)item.OwnerLot);

			if (cm == null)
			{
				player.Out.SendMessage("I can't find the consigmnent merchant for this item!", ChatType.CT_Merchant, ChatLocation.CL_ChatWindow);
				log.ErrorFormat("ME: Error finding consignment merchant for lot {0}; {1}:{2} trying to buy {3}", item.OwnerLot, player.Name, player.Client.Account.Name, item.Name);
				return;
			}

			if (player.ActiveInventoryObject != null)
			{
				player.ActiveInventoryObject.RemoveObserver(player);
			}

			player.ActiveInventoryObject = cm; // activate the target con merchant
			player.Out.SendInventoryItemsUpdate(cm.GetClientInventory(player), eInventoryWindowType.ConsignmentViewer);
			cm.AddObserver(player);
		}

		public virtual void AddObserver(GamePlayer player)
		{
			// not applicable
		}

		public virtual void RemoveObserver(GamePlayer player)
		{
			// not applicable
		}
    }
}
