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
using DOL.Database;
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handle housing pickup item requests from the client.
	/// </summary>
	[PacketHandler(PacketHandlerType.TCP, 0x0D, "Handles things like pickup indoor/outdoor items")]
	public class HousingPickupItemHandler : IPacketHandler
	{
		/// <summary>
		/// Handle the packet
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unknown = packet.ReadByte();
			int position = packet.ReadByte();
			int housenumber = packet.ReadShort();
			int method = packet.ReadByte();

			//HouseMgr.Logger.Debug("HousingPickupItemHandler unknown" + unknown + " position " + position + " method " + method);

			House house = (House) HouseMgr.GetHouse(client.Player.CurrentRegionID, housenumber);

			if (house == null) return 1;
			if (client.Player == null) return 1;

			switch (method)
			{
				case 1: //garden item
					if (!house.CanChangeGarden(client.Player, DecorationPermissions.Remove))
                        return 1;

					foreach(var entry in house.OutdoorItems)
					{
						OutdoorItem oitem = entry.Value;
						if (oitem.Position != position)
							continue;

						int i = entry.Key;
						GameServer.Database.DeleteObject(oitem.DatabaseItem); //delete the database instance

						InventoryItem invitem = new InventoryItem(((OutdoorItem) house.OutdoorItems[i]).BaseItem);
						client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
						house.OutdoorItems.Remove(i);

						client.Out.SendGarden(house);
						client.Out.SendMessage("Garden object removed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage(string.Format("You get {0} and put it in your backpack.", invitem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
					//no object @ position
					client.Out.SendMessage("There is no Garden Tile at slot " + position + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;

				case 2:
				case 3: //wall/floor mode
					if (!house.CanChangeInterior(client.Player, DecorationPermissions.Remove))
                        return 1;

					IndoorItem iitem = house.IndoorItems[position];
					if (iitem == null)
					{
						client.Player.Out.SendMessage("error: id was null", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					} //should this ever happen?

					if (iitem.BaseItem != null)
					{
						InventoryItem item = new InventoryItem(((IndoorItem) house.IndoorItems[(position)]).BaseItem);
						if (GetItemBack(item) == true)
						{
							if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
							{
								if (method == 2)
									client.Player.Out.SendMessage("The " + item.Name + " is cleared from the wall surface.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								else client.Player.Out.SendMessage("The " + item.Name + " is cleared from the floor.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								client.Player.Out.SendMessage("You need place in your inventory !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 1;
							}
						}
						else
							client.Player.Out.SendMessage("The " + item.Name + " is cleared from the wall surface.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else if (iitem.DatabaseItem.BaseItemID.Contains("GuildBanner"))
					{
						ItemTemplate it = new ItemTemplate();
						it.Id_nb = iitem.DatabaseItem.BaseItemID;
						it.CanDropAsLoot = false;
						it.IsDropable = true;
						it.IsPickable = true;
						it.IsTradable = true;
						it.Item_Type = 41;
						it.Level = 1;
						it.MaxCharges = 1;
						it.MaxCount = 1;
						it.Model = iitem.DatabaseItem.Model;
						it.Emblem = iitem.DatabaseItem.Emblem;
						string[] idnb = iitem.DatabaseItem.BaseItemID.Split('_');
						it.Name = idnb[1] + "'s Banner";
						it.Object_Type = (int)eObjectType.HouseWallObject;
						it.Realm = 0;
						it.Quality = 100;
						InventoryItem inv = new InventoryItem(it);
						if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, inv))
						{
							if (method == 2)
								client.Player.Out.SendMessage("The " + inv.Name + " is cleared from the wall surface.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							else client.Player.Out.SendMessage("The " + inv.Name + " is cleared from the floor.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							client.Player.Out.SendMessage("You need place in your inventory !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					else
						if (method == 2)
							client.Player.Out.SendMessage("The decoration item is cleared from the wall surface.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					else client.Player.Out.SendMessage("The decoration item is cleared from the floor.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

					GameServer.Database.DeleteObject(((IndoorItem) house.IndoorItems[(position)]).DatabaseItem);
					house.IndoorItems.Remove(position);

					GSTCPPacketOut pak = new GSTCPPacketOut(client.Out.GetPacketCode(ePackets.HousingItem));
					pak.WriteShort((ushort) housenumber);
					pak.WriteByte(0x01);
					pak.WriteByte(0x00);
					pak.WriteByte((byte)position);
					pak.WriteByte(0x00);
					foreach (GamePlayer plr in house.GetAllPlayersInHouse())
						plr.Out.SendTCP(pak);

					break;
			}
			return 1;
		}
		
		private bool GetItemBack(InventoryItem item)
		{
			#region item types
			switch (item.Object_Type)
			{
				case (int)eObjectType.Axe:
				case (int)eObjectType.Blades:
				case (int)eObjectType.Blunt:
				case (int)eObjectType.CelticSpear:
				case (int)eObjectType.CompositeBow:
				case (int)eObjectType.Crossbow:
				case (int)eObjectType.Flexible:
				case (int)eObjectType.Hammer:
				case (int)eObjectType.HandToHand:
				case (int)eObjectType.LargeWeapons:
				case (int)eObjectType.LeftAxe:
				case (int)eObjectType.Longbow:
				case (int)eObjectType.MaulerStaff:
				case (int)eObjectType.Piercing:
				case (int)eObjectType.PolearmWeapon:
				case (int)eObjectType.RecurvedBow:
				case (int)eObjectType.Scythe:
				case (int)eObjectType.Shield:
				case (int)eObjectType.SlashingWeapon:
				case (int)eObjectType.Spear:
				case (int)eObjectType.Staff:
				case (int)eObjectType.Sword:
				case (int)eObjectType.Thrown:
				case (int)eObjectType.ThrustWeapon:
				case (int)eObjectType.TwoHandedWeapon:
					return false;
					default: return true;
			}
			#endregion
		}
	}
}