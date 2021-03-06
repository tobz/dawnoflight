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
using DOL.Database2;
using DOL.Language;
using DOL.GS.Housing;
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x0C, "Handles things like placing indoor/outdoor items")]
	public class HousingPlaceItemHandler : IPacketHandler
	{
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private const string DEED_WEAK = "deedItem";
		private int position;

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unknow1	= packet.ReadByte();		// 1=Money 0=Item (?)
			int slot		= packet.ReadByte();		// Item/money slot
			ushort housenumber = packet.ReadShort();	// N� of house
			int unknow2	= (byte)packet.ReadByte();	
			position	= (byte)packet.ReadByte();
			int method	= packet.ReadByte();		// 2=Wall 3=Floor
			int rotation	= packet.ReadByte();		// garden items only
			short xpos	= (short)packet.ReadShort();	// x for inside objs
			short ypos	= (short)packet.ReadShort();	// y for inside objs.
			//log.Info("U1: " + unknow1 + " - U2: " + unknow2);

			House house = (House) HouseMgr.GetHouse(client.Player.CurrentRegionID,housenumber);

			//log.Info("position: " + position + " - rotation: " + rotation);
			if (house == null) return 1;
			if (client.Player == null) return 1;
			
			if ((slot >= 244) && (slot <= 248)) // money
			{
				if (!house.CanPayRent(client.Player))
				{
					client.Out.SendInventorySlotsUpdate(new int[] { slot });
					return 1;
				}
				long MoneyToAdd = position;
				switch (slot)
				{
					case 248: MoneyToAdd *= 1; break;
					case 247: MoneyToAdd *= 100; break;
					case 246: MoneyToAdd *= 10000; break;
					case 245: MoneyToAdd *= 10000000; break;
					case 244: MoneyToAdd *= 10000000000; break;
				}
				client.Player.TempProperties.setProperty(House.MONEYFORHOUSERENT, MoneyToAdd);
				client.Player.TempProperties.setProperty(House.HOUSEFORHOUSERENT, house);
				client.Player.Out.SendInventorySlotsUpdate(null);
				client.Player.Out.SendHousePayRentDialog("Housing07");
				
				return 1;
			}

			InventoryItem orgitem = client.Player.Inventory.GetItem((eInventorySlot)slot);
			if (orgitem == null) return 1;

            if (orgitem.Id_nb == "house_removal_deed")
            {                
                client.Out.SendInventorySlotsUpdate(null);
                if (HouseMgr.GetRealHouseByPlayer(client.Player) != house)
                {
                    client.Player.Out.SendMessage("You may not remove Houses that you don't own", eChatType.CT_System, eChatLoc.CL_SystemWindow);                   
                    return 1;
                }
                client.Player.TempProperties.setProperty(DEED_WEAK, new WeakRef(orgitem));                
                client.Player.Out.SendCustomDialog(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HouseRemoveOffer"), new CustomDialogResponse(HouseRemovalDialogue));
                return 0;
            }
			if (orgitem.Id_nb.Contains("cottage_deed") || orgitem.Id_nb.Contains("house_deed") || orgitem.Id_nb.Contains("villa_deed") || orgitem.Id_nb.Contains("mansion_deed"))
            {
                client.Out.SendInventorySlotsUpdate(null);
                if (HouseMgr.GetRealHouseByPlayer(client.Player) != house)
                {
                    client.Player.Out.SendMessage("You may not change other peoples houses", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    
                    return 1;
                }
                client.Player.TempProperties.setProperty(DEED_WEAK, new WeakRef(orgitem)); 
                client.Player.Out.SendMessage("Warning:\n This will remove all items from your current house!", eChatType.CT_System, eChatLoc.CL_PopupWindow);                
                client.Player.Out.SendCustomDialog("Are you sure you want to upgrade your House?", new CustomDialogResponse(HouseUpgradeDialogue));
                return 0;
            }
            if (orgitem.Name == "deed of guild transfer" 
                && client.Player.Guild != null && !client.Player.Guild.GuildOwnsHouse() 
                && house.HasOwnerPermissions(client.Player))
            {
                HouseMgr.HouseTransferToGuild(client.Player);
                client.Player.Inventory.RemoveItem(orgitem);
                client.Player.Guild.UpdateGuildWindow();
                return 0;
            }
            if (orgitem.Name == "interior banner removal")
            {
                house.IndoorGuildBanner = false;
                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.InteriorBannersRemoved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0;
            }
            if (orgitem.Name == "interior shield removal")
            {
                house.IndoorGuildShield = false;
                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.InteriorShieldsRemoved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0;
            }
            if (orgitem.Name == "exterior banner removal")
            {
                house.OutdoorGuildBanner = false;
                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.OutdoorBannersRemoved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0;
            }
            if (orgitem.Name == "exterior shield removal")
            {
                house.OutdoorGuildShield = false;
                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.OutdoorShieldsRemoved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0;
            }
            if (orgitem.Name == "carpet removal")
            {
                house.Rug1Color = 0;
                house.Rug2Color = 0;
                house.Rug3Color = 0;
                house.Rug4Color = 0;
                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.CarpetsRemoved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return 0;
            }

			if (orgitem.Object_Type == 49) // Garden items 
				method = 1;
			else if (orgitem.Id_nb == "porch_deed" || orgitem.Id_nb == "porch_remove_deed")
				method = 4;
			else if (orgitem.Object_Type == 50) // Indoor wall items
				method = 2;
			else if (orgitem.Object_Type == 51) // Indoor floor items
				method = 3;
			else if (orgitem.Object_Type >= 59 && orgitem.Object_Type <= 64) // Outdoor Roof/Wall/Door/Porch/Wood/Shutter/awning Material item type
			{
				client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HouseUseMaterials"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			else if (orgitem.Object_Type == 56 || orgitem.Object_Type == 52 || (orgitem.Object_Type >= 69 && orgitem.Object_Type <= 71)) // Indoor carpets 1-4
			{
				client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HouseUseCarpets"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			else if (orgitem.Object_Type == 57 || orgitem.Object_Type == 58  // Exterior banner/shield
				|| orgitem.Object_Type == 66 || orgitem.Object_Type == 67) // Interior banner/shield
				method = 6;
			else if (orgitem.Object_Type == 53 || orgitem.Object_Type == 55 || orgitem.Object_Type == 68)
				method = 5;
			else if (orgitem.Object_Type == 54)
				method = 7;
            int pos;
			switch (method)
			{
				case 1:
                    if (!house.CanAddGarden(client.Player))
					{
						client.Out.SendInventorySlotsUpdate(new int[] { slot });
						return 1;
					}
					if (house.OutdoorItems.Count >= ServerProperties.Properties.MAX_OUTDOOR_HOUSE_ITEMS)
					{
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.GardenMaxObjects"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendInventorySlotsUpdate(new int[] { slot });
						return 1;
					}

                    OutdoorItem oitem = new OutdoorItem();
                    oitem.BaseItem = (ItemTemplate)GameServer.Database.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), orgitem.Id_nb);
                    oitem.Model = orgitem.Model;
                    oitem.Position = Convert.ToByte(position);
					oitem.Rotation = Convert.ToByte(rotation);

					//add item in db
                    pos = GetFirstFreeSlot(house.OutdoorItems);
                    DBHouseOutdoorItem odbitem = oitem.CreateDBOutdoorItem(housenumber);
					oitem.DatabaseItem = odbitem;
					GameServer.Database.AddNewObject(odbitem);

                    client.Player.Inventory.RemoveItem(orgitem);
                    //add item to outdooritems
					house.OutdoorItems.Add(pos, oitem);
                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.GardenItemPlaced", (ServerProperties.Properties.MAX_OUTDOOR_HOUSE_ITEMS - house.OutdoorItems.Count)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.GardenItemPlacedName", orgitem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

					foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort) house.RegionID, house.X, house.Y, house.Z, WorldMgr.OBJ_UPDATE_DISTANCE))
						player.Out.SendGarden(house);
                    house.SaveIntoDatabase();
					break;

				case 2:
				case 3:
                    if (!house.CanAddInterior(client.Player))
					{
						client.Out.SendInventorySlotsUpdate(new int[] { slot });
						return 1;
					}
					if (orgitem.Object_Type != 50 && method == 2)
					{
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.NotWallObject"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendInventorySlotsUpdate(new int[] { slot });
						return 1;
					}
					if (orgitem.Object_Type != 51 && method == 3)
					{
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.NotFloorObject"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendInventorySlotsUpdate(new int[] { slot });
						return 1;
					}

					if (house.IndoorItems.Count >= ServerProperties.Properties.MAX_INDOOR_HOUSE_ITEMS)
					{
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.IndoorMaxItems", ServerProperties.Properties.MAX_INDOOR_HOUSE_ITEMS), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Out.SendInventorySlotsUpdate(new int[] { slot });
						return 1;
					}
					IndoorItem iitem = new IndoorItem();
					iitem.Model = orgitem.Model;
					iitem.Color = orgitem.Color;
					iitem.X = xpos;
					iitem.Y = ypos;

                    int ProperRotation = client.Player.Heading / 10;
                    if (ProperRotation > 360)
                    {
                        ProperRotation = 360;
                    }
                    else if (ProperRotation < 0)
                    {
                        ProperRotation = 0;
                    }
					if (orgitem.Object_Type == 50)
                        ProperRotation = 360;
                    iitem.Rotation = ProperRotation;

					iitem.Size = 100; //? dont know how this is defined. maybe DPS_AF or something.
					iitem.Position = position;
					iitem.Placemode = method;
					iitem.BaseItem = null;
					pos = GetFirstFreeSlot(house.IndoorItems);
					if (orgitem.Object_Type == 50 || orgitem.Object_Type == 51)
					{
						//its a housing item, so lets take it!
						client.Player.Inventory.RemoveItem(orgitem);
						//set right base item, so we can recreate it on take.
						iitem.BaseItem = (ItemTemplate) GameServer.Database.GetDatabaseObjectFromIDnb(typeof (ItemTemplate), orgitem.Id_nb);
					}

					DBHouseIndoorItem idbitem = iitem.CreateDBIndoorItem(housenumber);
					iitem.DatabaseItem = idbitem;
					GameServer.Database.AddNewObject(idbitem);
					house.IndoorItems.Add(pos, iitem);
                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.IndoorItemPlaced", (ServerProperties.Properties.MAX_INDOOR_HOUSE_ITEMS - house.IndoorItems.Count)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

					switch (method)
					{
						case 2:
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.IndoorWallPlaced", orgitem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
						case 3:
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.IndoorFloorPlaced", orgitem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
					}
					foreach (GamePlayer plr in house.GetAllPlayersInHouse())
						plr.Out.SendFurniture(house, pos);

					break;

                case 4:
                    {
                        if (!house.HasOwnerPermissions(client.Player) && !house.CanAddGarden(client.Player))
                        {
                            client.Out.SendInventorySlotsUpdate(new int[] { slot });
                            return 1;
                        }
                        switch (orgitem.Id_nb)
                        {
                            case "porch_deed":
                                if (house.EditPorch(true))
                                    client.Player.Inventory.RemoveItem(orgitem);
                                else
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.PorchAlready"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    client.Out.SendInventorySlotsUpdate(new int[] { slot });
                                }
                                return 1;
                            case "porch_remove_deed":
                                if (house.EditPorch(false))
                                    client.Player.Inventory.RemoveItem(orgitem);
                                else
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.PorchNone"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    client.Out.SendInventorySlotsUpdate(new int[] { slot });
                                }
                                return 1;
                            default:
                                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.PorchNotItem"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                client.Out.SendInventorySlotsUpdate(new int[] { slot });
                                return 1;

                        }
                    }

				case 5:
					{
						if (!house.CanAddInterior(client.Player))
						{
							client.Out.SendInventorySlotsUpdate(new int[] { slot });
							return 1;
						}

						if (house.GetHookpointLocation((uint)position) == null)
						{
                            client.Player.Inventory.RemoveItem(orgitem);
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HookPointID", + position), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HookPointCloser"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            client.Player.Out.SendCustomDialog(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HookPointLogLoc"), new CustomDialogResponse(LogLocation));
                        }
                        else if (house.GetHookpointLocation((uint)position) != null)
                        {
                            DBHousepointItem point = new DBHousepointItem();
                            point.HouseID = house.HouseNumber;
                            point.ItemTemplateID = orgitem.Id_nb;
                            point.Position = (uint)position;

                            // If we already have soemthing here, do not place more
                            foreach (DBHousepointItem hpitem in GameServer.Database.SelectObjects<DBHousepointItem>("HouseID",house.HouseNumber))
                            {
                                if (hpitem.Position == point.Position)
                                {
                                    client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HookPointAlready"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    return 1;
                                }
                            }
                            GameServer.Database.AddNewObject(point);
                            GameObject obj = house.FillHookpoint(orgitem, (uint)position, orgitem.Id_nb);
                            house.HousepointItems[point.Position] = point;
                            client.Player.Inventory.RemoveItem(orgitem);
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HookPointAdded"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            house.SaveIntoDatabase();
                        }
                        else
                            client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.HookPointNot"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        house.SendUpdate();
						break;
					}
                case 6: 
                    if (!house.CanEditAppearance(client.Player))
                    {
                        client.Out.SendInventorySlotsUpdate(new int[] { slot });
                        return 1;
                    }
                    if (orgitem.Object_Type == 57) // We have outdoor banner
                    {
                        house.OutdoorGuildBanner = true;
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.OutdoorBannersAdded"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Player.Inventory.RemoveItem(orgitem);
                    }
                    else if (orgitem.Object_Type == 58) // We have outdoor shield
                    {
                        house.OutdoorGuildShield = true;
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.OutdoorShieldsAdded"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Player.Inventory.RemoveItem(orgitem);
                    }
                    else if (orgitem.Object_Type == 66) // We have indoor banner
                    {
                        house.IndoorGuildBanner = true;
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.InteriorBannersAdded"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Player.Inventory.RemoveItem(orgitem);
                    }
                    else if (orgitem.Object_Type == 67) // We have indoor shield
                    {
                        house.IndoorGuildShield = true;
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.InteriorShieldsAdded"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        client.Player.Inventory.RemoveItem(orgitem);
                    }
                    else
                        client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Player.Housing.BadShieldBanner"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                    house.SaveIntoDatabase();
                    house.SendUpdate();
                    break;
				case 7: // House vault.
					int vaultIndex = house.GetFreeVaultNumber();
					if (vaultIndex < 0)
					{
						client.Player.Out.SendMessage("You can't add any more vaults to this house!",
							eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendInventorySlotsUpdate(new int[] { slot });
						return 1;
					}
					GameHouseVault houseVault = new GameHouseVault(orgitem, vaultIndex);
					houseVault.Attach(house, (uint)position, (ushort)((client.Player.Heading + 2048) % 4096));
					client.Player.Inventory.RemoveItem(orgitem);
					house.SaveIntoDatabase();
					house.SendUpdate();
					return 0;
				default:
					break;
			}
			return 1;
		}

		protected int GetFirstFreeSlot(Hashtable tbl)
		{
			int i = 0;//tbl.Count;
			while(tbl.Contains(i))
				i++;
			return i;
		}

		/// <summary>
		/// Does the player want to log the offset location of the missing housepoint
		/// </summary>
		/// <param name="player">The player</param>
		/// <param name="response">1 = yes 0 = no</param>
		protected void LogLocation(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return;

			if (player.CurrentHouse == null)
				return;

			log.Error("Position: " + position + " Offset: " + (player.X - player.CurrentHouse.X) + ", " + (player.Y - player.CurrentHouse.Y) + ", " + (player.Z - 25000) + ", " + (player.Heading - player.CurrentHouse.Heading));

            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Housing.HookPointLogged", position), eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
        protected static void HouseRemovalDialogue(GamePlayer player, byte response)
        {
            if (response != 0x01)
                return;


            WeakReference itemWeak = (WeakReference)player.TempProperties.getObjectProperty(DEED_WEAK, new WeakRef(null));
            player.TempProperties.removeProperty(DEED_WEAK);
            InventoryItem item = (InventoryItem)itemWeak.Target;

            if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
                || item.OwnerID == null || item.OwnerID != player.InternalID)
            {
                player.Out.SendMessage("You need a House removal Deed for this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            player.Inventory.RemoveItem(item);
            House house = HouseMgr.GetHouse((HouseMgr.GetHouseNumberByPlayer(player)));            
            HouseMgr.RemoveHouse(house);
            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Housing.HouseRemoved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }
        protected static void HouseUpgradeDialogue(GamePlayer player, byte response)
        {
            if (response != 0x01)
                return;


            WeakReference itemWeak = (WeakReference)player.TempProperties.getObjectProperty(DEED_WEAK, new WeakRef(null));
            player.TempProperties.removeProperty(DEED_WEAK);
            InventoryItem item = (InventoryItem)itemWeak.Target;

            if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
                || item.OwnerID == null || item.OwnerID != player.InternalID)
            {
                player.Out.SendMessage("This does not work without a House Deed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            House house = HouseMgr.GetHouse((HouseMgr.GetHouseNumberByPlayer(player)));
            HouseMgr.UpgradeHouse(house, item);
            player.Inventory.RemoveItem(item);
        }
    }
}
