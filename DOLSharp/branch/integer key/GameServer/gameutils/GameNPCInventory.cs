using System;
using System.Reflection;
using DOL.Database;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.Events;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// A class for individual NPC inventories
	/// this bypasses shared inventory templates which we sometimes need
	/// </summary>
	public class GameNPCInventory : GameLivingInventory
	{
		/// <summary>
		/// Creates a Guard Inventory from an Inventory Template
		/// </summary>
		/// <param name="template"></param>
		public GameNPCInventory(GameNpcInventoryTemplate template)
		{
			foreach (InventoryItem item in template.AllItems)
			{
				InventoryItem newItem = new InventoryItem();
				newItem.Model = item.Model;
				newItem.Object_Type = item.Object_Type;
				newItem.Hand = item.Hand;
				AddItem((eInventorySlot)item.SlotPosition, newItem);
			}
		}
	}
}
