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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&merchant", //command to handle
		 (uint)ePrivLevel.GM, //minimum privelege level
		 "Various merchant creation commands!", //command description
		 //Usage
		 "'/merchant create' to create an empty merchant",
		 "'/merchant save' to save this merchant as new object in the DB",
		 "'/merchant remove' to remove this merchant from the DB",
		 "'/merchant sell <itemsListID>' to assign this merchant with an articles list template",
		 "'/merchant sellremove' to remove the articles list template from merchant",
		 "'/merchant articles add <itemTemplateID> <pageNumber> [slot]' to add an item to the merchant articles list template",
		 "'/merchant articles remove <slot>' to remove item from the specified slot in this merchant inventory articles list template",
		 "'/merchant articles delete' to delete the inventory articles list template of the merchant")]	


	public class MerchantCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if(args.Length==1)
			{
				client.Out.SendMessage("Usage:",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/merchant create' to create an new merchant",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/merchant save' to save this merchant as new object in the DB",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/merchant remove' to remove this merchant from the DB",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/merchant sell <itemsListID>' to assign this merchant with an articles list template",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/merchant sellremove' to remove the articles list template from merchant",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/merchant articles add <itemTemplateID> <pageNumber> [slot]' to add an item to the merchant articles list template",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/merchant articles remove <pageNumber> <slot>' to remove item from the specified slot in this merchant inventory articles list template",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/merchant articles delete' to delete the inventory articles list template of the merchant",eChatType.CT_System,eChatLoc.CL_SystemWindow);	
				return 1;
			}
			string param="";
			if(args.Length>2)
				param=String.Join(" ",args,2,args.Length-2);
    
			GameMerchant targetMerchant=null;
			if(client.Player.TargetObject!=null && client.Player.TargetObject is GameMerchant)
				targetMerchant = (GameMerchant) client.Player.TargetObject;		
    			      			  
			if(args[1]!="create" && targetMerchant==null)
			{
				client.Out.SendMessage("Type /merchant for command overview",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 1;
			}
			
			switch(args[1].ToLower())
			{
				case "create":
				{
					//Create a new merchant
					GameMerchant merchant = new GameMerchant();
					//Fill the object variables
					merchant.Position=client.Player.Position;
					merchant.Region=client.Player.Region;
					merchant.Heading=client.Player.Heading;
					merchant.Level=1;
					merchant.Realm=client.Player.Realm;
					merchant.Name="New merchant";
					merchant.Model=9;
					//Fill the living variables
					merchant.CurrentSpeed=0;
					merchant.MaxSpeedBase=200;
					merchant.GuildName="";
					merchant.Size=50;
					merchant.AddToWorld();
					GameServer.Database.AddNewObject(merchant);
					client.Out.SendMessage("Merchant created: OID="+merchant.ObjectID,eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}
					break;

				case "save":
				{
					GameServer.Database.SaveObject(targetMerchant);
					client.Out.SendMessage("Target Merchant saved in DB!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}
					break;
			
				case "remove":
				{
					targetMerchant.RemoveFromWorld();
					if(targetMerchant.PersistantGameObjectID != 0) GameServer.Database.DeleteObject(targetMerchant);
					client.Out.SendMessage("Target Merchant removed from DB!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}
					break;

				case "sell":
				{
					if(args.Length==3)
					{
						try
						{
							targetMerchant.MerchantWindowID = int.Parse(args[2]);
							GameServer.Database.SaveObject(targetMerchant);
							client.Out.SendMessage("Merchant articles "+ int.Parse(args[2]) +" list loaded!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						} 
						catch(Exception)
						{
							client.Out.SendMessage("Type /merchant for command overview",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return 1;
						}
					} 
				}
					break;

				case "sellremove":
				{
					if(args.Length==2)
					{
						targetMerchant.MerchantWindowID = 0;
						GameServer.Database.SaveObject(targetMerchant);
						client.Out.SendMessage("Merchant articles list removed!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					}
				}
					break;

				/*case "articles":
				{
					switch(args[2])
					{
						case "add" : if(args.Length<=6)
									 {
										 try
										 {
											 string templateID = args[3];
											 int page = Convert.ToInt32(args[4]);
											 eMerchantWindowSlot slot = eMerchantWindowSlot.FirstEmptyInPage;

											 if(targetMerchant.TradeItems == null)
											 {
												 client.Out.SendMessage("Merchant articles list no found!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
												 return 1;
											 }
											
											 GenericItemTemplate template = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), templateID);
											 if(template == null)
											 {
												 client.Out.SendMessage("ItemTemplate with id "+templateID+" could not be found!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
												 return 1;
											 }

											 if(args.Length == 6)
											 {
												 slot = (eMerchantWindowSlot)Convert.ToInt32(args[5]);
											 }

											 slot = targetMerchant.TradeItems.GetValidSlot(page ,slot);
											 if(slot == eMerchantWindowSlot.Invalid)
											 {
												 client.Out.SendMessage("Page number ("+page+") must be from 0 to "+(MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS-1)+" and slot ("+slot+") must be from 0 to "+(MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS-1)+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
												 return 1;
											 }

											 MerchantItem item = (MerchantItem) GameServer.Database.SelectObject(typeof(MerchantItem),Expression.And(Expression.And(Expression.Eq("ItemListID",targetMerchant.TradeItems.ItemsListID), Expression.Eq("PageNumber", page)) ,Expression.Eq("SlotPosition", slot)));
											 if(item == null)
											 {
												 item = new MerchantItem();
												 item.ItemListID = targetMerchant.TradeItems.ItemsListID;
												 item.ItemTemplateID = templateID;
												 item.SlotPosition = (int)slot;
												 item.PageNumber = page;

												 GameServer.Database.AddNewObject(item);
											 }
											 else
											 {
												 item.ItemTemplateID = templateID;
												 GameServer.Database.SaveObject(item);
											 }
											 client.Out.SendMessage("Item added to the merchant articles list!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
										 }
										 catch(Exception)
										 {
											 client.Out.SendMessage("Type /merchant for command overview",eChatType.CT_System,eChatLoc.CL_SystemWindow);
											 return 1;
										 }
									 }
									 else
									 {
										 client.Out.SendMessage("Usage: /merchant articles add <ItemTemplate> <page> [slot]",eChatType.CT_System,eChatLoc.CL_SystemWindow);
									 }
							break;

						case "remove" : if(args.Length==5)
										{
											try
											{
												int page = Convert.ToInt32(args[3]);
												int slot = Convert.ToInt32(args[4]);

												if(page < 0 || page >= MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS)
												{
													client.Out.SendMessage("Page number ("+page+") must be between [0;"+(MerchantTradeItems.MAX_PAGES_IN_TRADEWINDOWS-1)+"]!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
													return 1;
												}

												if(slot < 0 || slot >= MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS)
												{
													client.Out.SendMessage("Slot ("+slot+") must be between [0;"+(MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS-1)+"]!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
													return 1;
												}
												
												if(targetMerchant.TradeItems == null)
												{
													client.Out.SendMessage("Merchant articles list no found!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
													return 1;
												}

												MerchantItem item = (MerchantItem) GameServer.Database.SelectObject(typeof(MerchantItem),Expression.And(Expression.And(Expression.Eq("ItemListID",targetMerchant.TradeItems.ItemsListID), Expression.Eq("PageNumber", page)) ,Expression.Eq("SlotPosition", slot)));
												if(item == null)
												{
													client.Out.SendMessage("Slot "+slot+" in page "+page+" is already empty.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
													return 1;
												}
												GameServer.Database.DeleteObject(item);
												client.Out.SendMessage("Merchant articles list slot "+slot+" in page "+page+" cleaned!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
											
											} 
											catch(Exception)
											{
												client.Out.SendMessage("Type /merchant for command overview",eChatType.CT_System,eChatLoc.CL_SystemWindow);
												return 1;
											}
										}
										else
										{
											client.Out.SendMessage("Usage: /merchant articles remove <page> <slot>",eChatType.CT_System,eChatLoc.CL_SystemWindow);
										}
							break;

						case "delete" : if(args.Length==3)
										{
											try
											{
												if(targetMerchant.TradeItems == null)
												{
													client.Out.SendMessage("Merchant articles list no found!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
													return 1;
												}
												client.Out.SendMessage("Deleting articles list template ...",eChatType.CT_System,eChatLoc.CL_SystemWindow);

												IList merchantitems = GameServer.Database.SelectObjects(typeof(MerchantItem),Expression.Eq("ItemsListID",targetMerchant.TradeItems.ItemsListID));
												foreach(MerchantItem item in merchantitems)
												{
													GameServer.Database.DeleteObject(item);
												}
												
												client.Out.SendMessage("Merchant articles list deleted.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
											} 
											catch(Exception)
											{
												client.Out.SendMessage("Type /merchant for command overview",eChatType.CT_System,eChatLoc.CL_SystemWindow);
												return 1;
											}

										}
							break;
						default : client.Out.SendMessage("Type /merchant for command overview",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							break;
					}
					break;
				}*/
			}
			return 1;
		}
	}
}