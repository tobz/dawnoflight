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
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.ServerProperties;

namespace DOL.GS.Housing
{
	public sealed class HouseTemplateMgr
	{
		public static MerchantTradeItems AlbionLotMarkerItems;
		public static MerchantTradeItems HiberniaLotMarkerItems;
		public static MerchantTradeItems IndoorBindstoneMenuItems;
		public static MerchantTradeItems IndoorCraftMenuItems;
		public static MerchantTradeItems IndoorMenuItems;
		public static MerchantTradeItems IndoorNPCMenuItems;
		public static MerchantTradeItems IndoorShopItems;
		public static MerchantTradeItems IndoorVaultMenuItems;
		public static MerchantTradeItems MidgardLotMarkerItems;
		public static MerchantTradeItems OutdoorMenuItems;
		public static MerchantTradeItems OutdoorShopItems;

		public static void Initialize()
		{
			CheckItemTemplates();
			CheckMerchantItemTemplates();
			LoadItemLists();
			CheckNPCTemplates();
		}

		public static long GetLotPrice(DBHouse house)
		{
			TimeSpan diff = (DateTime.Now - house.CreationTime);

			long price = Properties.HOUSING_LOT_PRICE_START - (long) (diff.TotalHours*Properties.HOUSING_LOT_PRICE_PER_HOUR);
			if (price < Properties.HOUSING_LOT_PRICE_MINIMUM)
			{
				return Properties.HOUSING_LOT_PRICE_MINIMUM;
			}

			return price;
		}

		public static MerchantTradeItems GetLotMarkerItems(GameLotMarker marker)
		{
			switch (marker.CurrentRegionID)
			{
				case 2:
					return AlbionLotMarkerItems;
				case 102:
					return MidgardLotMarkerItems;
				default:
					return HiberniaLotMarkerItems;
			}
		}

		private static void LoadItemLists()
		{
			AlbionLotMarkerItems = new MerchantTradeItems("alb_lotmarker");
			MidgardLotMarkerItems = new MerchantTradeItems("mid_lotmarker");
			HiberniaLotMarkerItems = new MerchantTradeItems("hib_lotmarker");

			IndoorMenuItems = new MerchantTradeItems("housing_indoor_menu");
			IndoorShopItems = new MerchantTradeItems("housing_indoor_shop");
			OutdoorMenuItems = new MerchantTradeItems("housing_outdoor_menu");
			OutdoorShopItems = new MerchantTradeItems("housing_outdoor_shop");

			IndoorNPCMenuItems = new MerchantTradeItems("housing_indoor_npc");
			IndoorVaultMenuItems = new MerchantTradeItems("housing_indoor_vault");
			IndoorCraftMenuItems = new MerchantTradeItems("housing_indoor_craft");
			IndoorBindstoneMenuItems = new MerchantTradeItems("housing_indoor_bindstone");
		}

		private static void CheckItemTemplates()
		{
			//lot marker
			CheckItemTemplate("Albion cottage deed", "alb_cottage_deed", 498, 0, 10000000, 0, 0, 0, 0, 1);
			CheckItemTemplate("Albion house deed", "alb_house_deed", 498, 0, 50000000, 0, 0, 0, 0, 1);
			CheckItemTemplate("Albion villa deed", "alb_villa_deed", 498, 0, 100000000, 0, 0, 0, 0, 1);
			CheckItemTemplate("Albion mansion deed", "alb_mansion_deed", 498, 0, 250000000, 0, 0, 0, 0, 1);
			CheckItemTemplate("Midgard cottage deed", "mid_cottage_deed", 498, 0, 10000000, 0, 0, 0, 0, 2);
			CheckItemTemplate("Midgard house deed", "mid_house_deed", 498, 0, 50000000, 0, 0, 0, 0, 2);
			CheckItemTemplate("Midgard villa deed", "mid_villa_deed", 498, 0, 100000000, 0, 0, 0, 0, 2);
			CheckItemTemplate("Midgard mansion deed", "mid_mansion_deed", 498, 0, 250000000, 0, 0, 0, 0, 2);
			CheckItemTemplate("Hibernia cottage deed", "hib_cottage_deed", 498, 0, 10000000, 0, 0, 0, 0, 3);
			CheckItemTemplate("Hibernia house deed", "hib_house_deed", 498, 0, 50000000, 0, 0, 0, 0, 3);
			CheckItemTemplate("Hibernia villa deed", "hib_villa_deed", 498, 0, 100000000, 0, 0, 0, 0, 3);
			CheckItemTemplate("Hibernia mansion deed", "hib_mansion_deed", 498, 0, 250000000, 0, 0, 0, 0, 3);
			CheckItemTemplate("Porch deed", "porch_deed", 498, 0, 5000000, 0, 0, 0, 0, 0);
			CheckItemTemplate("Porch remove deed", "porch_remove_deed", 498, 0, 500000, 0, 0, 0, 0, 0);
			CheckItemTemplate("deed of guild transfer", "deed_of_guild_transfer", 498, 0, 500000, 0, 0, 0, 0, 0);
			CheckItemTemplate("House removal deed", "house_removal_deed", 498, 0, 50000000, 0, 0, 0, 0, 0);

			//indoor npc
			CheckItemTemplate("Hastener", "hastener", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 500, 0, 0);
			CheckItemTemplate("Smith", "smith", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 501, 0, 0);
			CheckItemTemplate("Enchanter", "enchanter", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 502, 0, 0);
			CheckItemTemplate("Emblemer", "emblemer", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 503, 0, 0);
			CheckItemTemplate("Healer", "healer", 593, (int) eObjectType.HouseNPC, 30000000, 0, 0, 504, 0, 0);
			CheckItemTemplate("Recharger", "recharger", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 505, 0, 0);
			CheckItemTemplate("Hibernia Teleporter", "hib_teleporter", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 506, 0, 0);
			CheckItemTemplate("Albion Teleporter", "alb_teleporter", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 506, 0, 0);
			CheckItemTemplate("Midgard Teleporter", "mid_teleporter", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 506, 0, 0);
			CheckItemTemplate("Apprentice Merchant", "apprentice_merchant", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 507,
			                  0, 0);
			CheckItemTemplate("Grandmaster Merchant", "grandmaster_merchant", 593, (int) eObjectType.HouseNPC, 5000000, 0, 0, 508,
			                  0, 0);
			CheckItemTemplate("Incantation Merchant", "incantation_merchant", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 509,
			                  0, 0);
			CheckItemTemplate("Poison and Dye Supplies", "poison_dye_supplies", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0,
			                  510, 0, 0);
			CheckItemTemplate("Potion, Tincture, and Enchantment Supplies", "potion_tincture_enchantment_supplies", 593,
			                  (int) eObjectType.HouseNPC, 1000000, 0, 0, 511, 0, 0);
			CheckItemTemplate("Poison and Potion Supplies", "poison_potion_supplies", 593, (int) eObjectType.HouseNPC, 1000000, 0,
			                  0, 512, 0, 0);
			CheckItemTemplate("Dye, Tincture, and Enchantment Supplies", "dye_tincture_enchantment_supplies", 593,
			                  (int) eObjectType.HouseNPC, 1000000, 0, 0, 513, 0, 0);
			CheckItemTemplate("Taxidermy Supplies", "taxidermy_supplies", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 514, 0,
			                  0);
			CheckItemTemplate("Siegecraft Supplies", "siegecraft_supplies", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 515,
			                  0, 0);
			CheckItemTemplate("Hibernia Vault Keeper", "hib_vault_keeper", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 516, 0,
			                  0);
			CheckItemTemplate("Dye Supply Master", "dye_supply_master", 593, (int) eObjectType.HouseNPC, 1000000, 0, 0, 517, 0, 0);

			//indoor craft
			CheckItemTemplate("alchemy table", "alchemy_table", 1494, (int) eObjectType.HouseInteriorObject, 10000000, 0, 0, 0, 0,
			                  0);
			CheckItemTemplate("forge", "forge", 1495, (int) eObjectType.HouseInteriorObject, 10000000, 0, 0, 0, 0, 0);
			CheckItemTemplate("lathe", "lathe", 1496, (int) eObjectType.HouseInteriorObject, 10000000, 0, 0, 0, 0, 0);

			//indoor bindstone
			CheckItemTemplate("Albion bindstone", "alb_bindstone", 1488, (int) eObjectType.HouseBindstone, 10000000, 0, 0, 0, 0,
			                  1);
			CheckItemTemplate("Midgard bindstone", "mid_bindstone", 1492, (int) eObjectType.HouseBindstone, 10000000, 0, 0, 0, 0,
			                  2);
			CheckItemTemplate("Hibernia bindstone", "hib_bindstone", 1490, (int) eObjectType.HouseBindstone, 10000000, 0, 0, 0, 0,
			                  3);

			//indoor vault
			CheckItemTemplate("Albion vault", "alb_vault", 1489, (int) eObjectType.HouseVault, 10000000, 0, 0, 0, 0, 1);
			CheckItemTemplate("Midgard vault", "mid_vault", 1493, (int) eObjectType.HouseVault, 10000000, 0, 0, 0, 0, 2);
			CheckItemTemplate("Hibernia vault", "hib_vault", 1491, (int) eObjectType.HouseVault, 10000000, 0, 0, 0, 0, 3);
		}

		private static void CheckMerchantItemTemplates()
		{
			//lot markers
			string[] alblotmarkeritems = {
			                             	"alb_cottage_deed", "alb_house_deed", "alb_villa_deed", "alb_mansion_deed",
			                             	"porch_deed", "porch_remove_deed", "deed_of_guild_transfer"
			                             };
			CheckMerchantItems("alb_lotmarker", alblotmarkeritems);
			string[] midlotmarkeritems = {
			                             	"mid_cottage_deed", "mid_house_deed", "mid_villa_deed", "mid_mansion_deed",
			                             	"porch_deed", "porch_remove_deed", "deed_of_guild_transfer"
			                             };
			CheckMerchantItems("mid_lotmarker", midlotmarkeritems);
			string[] hiblotmarkeritems = {
			                             	"hib_cottage_deed", "hib_house_deed", "hib_villa_deed", "hib_mansion_deed",
			                             	"porch_deed", "porch_remove_deed", "deed_of_guild_transfer"
			                             };
			CheckMerchantItems("hib_lotmarker", hiblotmarkeritems);

			//hookpoints
			string[] indoornpc = {
			                     	"hastener", "smith", "enchanter", "emblemeer", "healer", "recharger", "hib_teleporter",
			                     	"alb_teleporter", "mid_teleporter", "apprentice_merchant", "grandmaster_merchant",
			                     	"incantation_merchant", "poison_dye_supplies", "potion_tincture_enchantment_supplies",
			                     	"poison_potion_supplies", "taxidermy_supplies", "siegecraft_supplies", "hib_vault_keeper",
			                     	"mid_vault_keeper", "alb_vault_keeper", "dye_supply_master"
			                     };
			CheckMerchantItems("housing_indoor_npc", indoornpc);
			string[] indoorbindstone = {"hib_bindstone", "mid_bindstone", "alb_bindstone"};
			CheckMerchantItems("housing_indoor_bindstone", indoorbindstone);
			string[] indoorcraft = {"alchemy_table", "forge", "lathe"};
			CheckMerchantItems("housing_indoor_craft", indoorcraft);
			string[] indoorvault = {"hib_vault", "mid_vault", "alb_vault"};
			CheckMerchantItems("housing_indoor_vault", indoorvault);
		}

		private static void CheckMerchantItems(string merchantid, string[] itemids)
		{
			IList<MerchantItem> merchantitems =
				GameServer.Database.SelectObjects<MerchantItem>("ItemListID=\'" + GameServer.Database.Escape(merchantid) + "\'");

			int slot = 0;
			foreach (string itemid in itemids)
			{
				bool found = false;
				foreach (MerchantItem dbitem in merchantitems)
				{
					if (dbitem.ItemTemplateID == itemid)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					var newitem = new MerchantItem
					              	{
					              		ItemListID = merchantid,
					              		ItemTemplateID = itemid,
					              		SlotPosition = (slot%30),
					              		PageNumber = (slot/30)
					              	};

					GameServer.Database.AddObject(newitem);
				}
				slot += 1;
			}
		}

		private static void CheckItemTemplate(string name, string id, int model, int objtype, int copper, int dps, int spd,
		                                      int bonus, int weight, int realm)
		{
			var templateitem = GameServer.Database.FindObjectByKey<ItemTemplate>(GameServer.Database.Escape(id));
			if (templateitem == null)
			{
				templateitem = new ItemTemplate
				               	{
				               		Name = name,
				               		Model = model,
				               		Level = 0,
				               		Object_Type = objtype,
				               		Id_nb = id,
				               		IsPickable = true,
				               		IsDropable = true,
				               		DPS_AF = dps,
				               		SPD_ABS = spd,
				               		Hand = 0x0E,
				               		Weight = weight,
				               		Price = copper,
				               		Bonus = bonus,
				               		Realm = (byte) realm,
				               	};

				GameServer.Database.AddObject(templateitem);
			}
		}

		private static void CheckNPCTemplates()
		{
			#region Hibernia

			//Hastener
			CheckNPCTemplate(500, "DOL.GS.GameHastener", "Hastener", "50", "");
			//Smith
			CheckNPCTemplate(501, "DOL.GS.Blacksmith", "Smith", "50", "");
			//Enchanter
			CheckNPCTemplate(502, "DOL.GS.Enchanter", "Enchanter", "50", "");
			//Emblemeer
			CheckNPCTemplate(503, "DOL.GS.EmblemNPC", "Emblemer", "50", "");
			//Healer
			CheckNPCTemplate(504, "DOL.GS.GameHealer", "Healer", "50", "");
			//Recharger
			CheckNPCTemplate(505, "DOL.GS.Recharger", "Recharger", "50", "");
			//Teleporter
			//TODO: [WARN] CheckNPCTemplate( ... Teleporter ... )
			CheckNPCTemplate(506, "DOL.GS.GameNPC", "Teleporter", "50", "");
			//Apprentice Merchant
			//TODO: [WARN] merchant list
			CheckNPCTemplate(507, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Grandmaster Merchant
			//TODO: [WARN] merchant list
			CheckNPCTemplate(508, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Incantation Merchant
			//TODO: [WARN] merchant list
			CheckNPCTemplate(509, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Poison and Dye Supplies
			//TODO: [WARN] merchant list
			CheckNPCTemplate(510, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Potion, Tincture, and Enchantment Supplies
			//TODO: [WARN] merchant list
			CheckNPCTemplate(511, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Poison and Potion Supplies
			//TODO: [WARN] merchant list
			CheckNPCTemplate(512, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Dye, Tincture, and Enchantment Supplies
			//TODO: [WARN] merchant list
			CheckNPCTemplate(513, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Taxidermy Supplies
			//TODO: [WARN] merchant list
			CheckNPCTemplate(514, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Siegecraft Supplies
			//TODO: [WARN] merchant list
			CheckNPCTemplate(515, "DOL.GS.GameMerchant", "Merchant", "50", "");
			//Hibernia Vault Keeper
			CheckNPCTemplate(516, "DOL.GS.GameVaultKeeper", "Vault Keeper", "50", "");
			//Dye Supply Master
			//TODO: [WARN] merchant list
			CheckNPCTemplate(517, "DOL.GS.GameMerchant", "Merchant", "50", "");

			#endregion
		}

		private static void CheckNPCTemplate(int templateID, string classType, string guild, string model, string inventory)
		{
			NpcTemplate template = NpcTemplateMgr.GetTemplate(templateID);
			if (template == null)
			{
				template = new NpcTemplate
				           	{
				           		Name = "",
				           		TemplateId = templateID,
				           		ClassType = classType,
				           		GuildName = guild,
				           		Model = model,
				           		Size = "50",
				           		Level = "50",
				           		Inventory = inventory
				           	};

				NpcTemplateMgr.AddTemplate(template);
				//template.SaveIntoDatabase(); lets not save yet
			}
		}
	}
}