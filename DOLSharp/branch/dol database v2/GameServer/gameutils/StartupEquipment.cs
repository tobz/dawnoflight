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
using System.Linq;
using System.Collections;
using DOL.Database2;

namespace DOL.GS
{
	/// <summary>
	/// Equips new created Characters with standard equipment
	/// </summary>
	public class StartupEquipment
	{
		/// <summary>
		/// Add starter equipment to the character
		/// </summary>
		/// <param name="c">The character</param>
		public static void AddEquipment(Character c)
		{
			Hashtable usedSlots = new Hashtable();


			foreach (StarterEquipment item in (from s in GameServer.Database.OfType<StarterEquipment>()
                                        where s.Class == 0 || s.Class == c.Class
                                        select s))
			{
				if (item.Template == null)
				{
					GameServer.Instance.Logger.Error("StartupEquipment.cs error adding starter equipment for class " + c.Class + " cannot find itemtemplate for " + item.TemplateID);
					continue;
				}

				InventoryItem inventoryItem = new InventoryItem(item.Template);
				inventoryItem.OwnerID = c.ObjectId;
				inventoryItem.Realm = c.Realm;

				//if equipable item, equip
				foreach (eInventorySlot slot in GameLivingInventory.EQUIP_SLOTS)
				{
					if (slot == (eInventorySlot)inventoryItem.Item_Type)
					{
						if (usedSlots.ContainsKey(slot))
						{
							GameServer.Instance.Logger.Error("Cannot add item " + item.TemplateID + " to class " + item.Class + " already an item for that slot assigned!");
							continue;
						}
						else
						{
							eInventorySlot chosenSlot = eInventorySlot.FirstEmptyBackpack;
							//left hand weapons we put in right hands
							if (slot == eInventorySlot.LeftHandWeapon && (eObjectType)inventoryItem.Object_Type != eObjectType.Shield)
								chosenSlot = eInventorySlot.RightHandWeapon;
							else chosenSlot = slot;
							inventoryItem.SlotPosition = (int)chosenSlot;
							usedSlots[chosenSlot] = true;
							if (c.ActiveWeaponSlot == 0)
							{
								switch (inventoryItem.SlotPosition)
								{
									case Slot.RIGHTHAND:
										c.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.Standard;
										break;
									case Slot.TWOHAND:
										c.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.TwoHanded;
										break;
									case Slot.RANGED:
										c.ActiveWeaponSlot = (byte)GamePlayer.eActiveWeaponSlot.Distance;
										break;
								}
							}
						}
					}
				}
				if (inventoryItem.SlotPosition == 0)
				{
					//otherwise stick the item in the backpack
					for (int i = (int)eInventorySlot.FirstBackpack; i < (int)eInventorySlot.LastBackpack; i++)
					{
						if (usedSlots[i] == null)
						{
							inventoryItem.SlotPosition = i;
							usedSlots[i] = true;
							break;
						}
					}
				}
				GameServer.Database.AddNewObject(inventoryItem);
			}
		}
	}
}