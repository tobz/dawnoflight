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
using System.Linq;
using System.Reflection;
using DOL.Database;
using DOL.Language;
using log4net;

namespace DOL.GS.Housing
{
	public class House : Point3D, IGameLocation
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly DBHouse _databaseItem;
		private readonly Dictionary<int, DBHouseCharsXPerms> _housePermissions;
		private readonly Dictionary<uint, DBHousepointItem> _housepointItems;
		private readonly Dictionary<int, IndoorItem> _indoorItems;
		private readonly Dictionary<int, OutdoorItem> _outdoorItems;
		private readonly Dictionary<int, DBHousePermissions> _permissionLevels;
		private GameConsignmentMerchant _consignment;

		#region Properties

		public int HouseNumber
		{
			get { return _databaseItem.HouseNumber; }
			set { _databaseItem.HouseNumber = value; }
		}

		public string OwnerID
		{
			get { return _databaseItem.OwnerID; }
			set { _databaseItem.OwnerID = value; }
		}

		public int Model
		{
			get { return _databaseItem.Model; }
			set { _databaseItem.Model = value; }
		}

		public int Emblem
		{
			get { return _databaseItem.Emblem; }
			set { _databaseItem.Emblem = value; }
		}

		public int PorchRoofColor
		{
			get { return _databaseItem.PorchRoofColor; }
			set { _databaseItem.PorchRoofColor = value; }
		}

		public int PorchMaterial
		{
			get { return _databaseItem.PorchMaterial; }
			set { _databaseItem.PorchMaterial = value; }
		}

		public bool Porch
		{
			get { return _databaseItem.Porch; }
			set { _databaseItem.Porch = value; }
		}

		public bool IndoorGuildBanner
		{
			get { return _databaseItem.IndoorGuildBanner; }
			set { _databaseItem.IndoorGuildBanner = value; }
		}

		public bool IndoorGuildShield
		{
			get { return _databaseItem.IndoorGuildShield; }
			set { _databaseItem.IndoorGuildShield = value; }
		}

		public bool OutdoorGuildBanner
		{
			get { return _databaseItem.OutdoorGuildBanner; }
			set { _databaseItem.OutdoorGuildBanner = value; }
		}

		public bool OutdoorGuildShield
		{
			get { return _databaseItem.OutdoorGuildShield; }
			set { _databaseItem.OutdoorGuildShield = value; }
		}

		public int RoofMaterial
		{
			get { return _databaseItem.RoofMaterial; }
			set { _databaseItem.RoofMaterial = value; }
		}

		public int DoorMaterial
		{
			get { return _databaseItem.DoorMaterial; }
			set { _databaseItem.DoorMaterial = value; }
		}

		public int WallMaterial
		{
			get { return _databaseItem.WallMaterial; }
			set { _databaseItem.WallMaterial = value; }
		}

		public int TrussMaterial
		{
			get { return _databaseItem.TrussMaterial; }
			set { _databaseItem.TrussMaterial = value; }
		}

		public int WindowMaterial
		{
			get { return _databaseItem.WindowMaterial; }
			set { _databaseItem.WindowMaterial = value; }
		}

		public int Rug1Color
		{
			get { return _databaseItem.Rug1Color; }
			set { _databaseItem.Rug1Color = value; }
		}

		public int Rug2Color
		{
			get { return _databaseItem.Rug2Color; }
			set { _databaseItem.Rug2Color = value; }
		}

		public int Rug3Color
		{
			get { return _databaseItem.Rug3Color; }
			set { _databaseItem.Rug3Color = value; }
		}

		public int Rug4Color
		{
			get { return _databaseItem.Rug4Color; }
			set { _databaseItem.Rug4Color = value; }
		}

		public DateTime LastPaid
		{
			get { return _databaseItem.LastPaid; }
			set { _databaseItem.LastPaid = value; }
		}

		public long KeptMoney
		{
			get { return _databaseItem.KeptMoney; }
			set { _databaseItem.KeptMoney = value; }
		}

		public bool NoPurge
		{
			get { return _databaseItem.NoPurge; }
			set { _databaseItem.NoPurge = value; }
		}

		public int UniqueID { get; set; }

		public IDictionary<int, IndoorItem> IndoorItems
		{
			get { return _indoorItems; }
		}

		public IDictionary<int, OutdoorItem> OutdoorItems
		{
			get { return _outdoorItems; }
		}

		public IDictionary<uint, DBHousepointItem> HousepointItems
		{
			get { return _housepointItems; }
		}

		public DBHouse DatabaseItem
		{
			get { return _databaseItem; }
		}

		public IEnumerable<KeyValuePair<int, DBHouseCharsXPerms>> HousePermissions
		{
			get { return _housePermissions.OrderBy(entry => entry.Value.CreationTime); }
		}

		public IDictionary<int, DBHousePermissions> PermissionLevels
		{
			get { return _permissionLevels; }
		}

		public GameConsignmentMerchant ConsignmentMerchant
		{
			get { return _consignment; }
			set { _consignment = value; }
		}

		public bool IsOccupied
		{
			get
			{
				foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(RegionID, X, Y, 25000, WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player.CurrentHouse == this && player.InHouse)
					{
						return true;
					}
				}

				return false;
			}
		}

		public override int X
		{
			get { return _databaseItem.X; }
			set { _databaseItem.X = value; }
		}

		public override int Y
		{
			get { return _databaseItem.Y; }
			set { _databaseItem.Y = value; }
		}

		public override int Z
		{
			get { return _databaseItem.Z; }
			set { _databaseItem.Z = value; }
		}

		public ushort RegionID
		{
			get { return _databaseItem.RegionID; }
			set { _databaseItem.RegionID = value; }
		}

		public ushort Heading
		{
			get { return (ushort) _databaseItem.Heading; }
			set { _databaseItem.Heading = value; }
		}

		public string Name
		{
			get { return _databaseItem.Name; }
			set { _databaseItem.Name = value; }
		}

		#endregion

		public House(DBHouse house)
		{
			_databaseItem = house;
			_permissionLevels = new Dictionary<int, DBHousePermissions>();
			_indoorItems = new Dictionary<int, IndoorItem>();
			_outdoorItems = new Dictionary<int, OutdoorItem>();
			_housepointItems = new Dictionary<uint, DBHousepointItem>();
			_housePermissions = new Dictionary<int, DBHouseCharsXPerms>();
		}

		/// <summary>
		/// The spot you are teleported to when you exit this house.
		/// </summary>
		public GameLocation OutdoorJumpPoint
		{
			get
			{
				double angle = Heading*((Math.PI*2)/360); // angle*2pi/360;
				var x = (int) (X + (0*Math.Cos(angle) + 500*Math.Sin(angle)));
				var y = (int) (Y - (500*Math.Cos(angle) - 0*Math.Sin(angle)));
				var heading = (ushort) ((Heading < 180 ? Heading + 180 : Heading - 180)/0.08789);

				return new GameLocation("Housing", RegionID, x, y, Z, heading);
			}
		}

		/// <summary>
		/// Sends a update of the house and the garden to all players in range
		/// </summary>
		public void SendUpdate()
		{
			foreach (
				GamePlayer player in WorldMgr.GetPlayersCloseToSpot(RegionID, X, Y, Z, HousingConstants.HouseViewingDistance))
			{
				player.Out.SendHouse(this);
				player.Out.SendGarden(this);
			}
		}

		/// <summary>
		/// Used to get into a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		public void Enter(GamePlayer player)
		{
			IList<GamePlayer> list = GetAllPlayersInHouse();
			if (list.Count == 0)
			{
				foreach (GamePlayer pl in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					pl.Out.SendHouseOccupied(this, true);
				}
			}

			ChatUtil.SendSystemMessage(player, "House.Enter.EnteringHouse", HouseNumber);
			player.Out.SendEnterHouse(this);
			player.Out.SendFurniture(this);

			player.InHouse = true;
			player.CurrentHouse = this;

			switch (Model)
			{
					//thx to sp4m
				default:
					player.MoveTo(RegionID, X, Y, 25022, player.Heading);
					break;

				case 1:
					player.MoveTo(RegionID, X + 80, Y + 100, ((25025)), player.Heading);
					break;

				case 2:
					player.MoveTo(RegionID, X - 260, Y + 100, ((24910)), player.Heading);
					break;

				case 3:
					player.MoveTo(RegionID, X - 200, Y + 100, ((24800)), player.Heading);
					break;

				case 4:
					player.MoveTo(RegionID, X - 350, Y - 30, ((24660)), player.Heading);
					break;

				case 5:
					player.MoveTo(RegionID, X + 230, Y - 480, ((25100)), player.Heading);
					break;

				case 6:
					player.MoveTo(RegionID, X - 80, Y - 660, ((24700)), player.Heading);
					break;

				case 7:
					player.MoveTo(RegionID, X - 80, Y - 660, ((24700)), player.Heading);
					break;

				case 8:
					player.MoveTo(RegionID, X - 90, Y - 625, ((24670)), player.Heading);
					break;

				case 9:
					player.MoveTo(RegionID, X + 400, Y - 160, ((25150)), player.Heading);
					break;

				case 10:
					player.MoveTo(RegionID, X + 400, Y - 80, ((25060)), player.Heading);
					break;

				case 11:
					player.MoveTo(RegionID, X + 400, Y - 60, ((24900)), player.Heading);
					break;

				case 12:
					player.MoveTo(RegionID, X, Y - 620, ((24595)), player.Heading);
					break;
			}

			ChatUtil.SendSystemMessage(player, "House.Enter.EnteredHouse", HouseNumber);
		}

		/// <summary>
		/// Used to leave a house
		/// </summary>
		/// <param name="player">the player who wants to get in</param>
		/// <param name="silent">text or not</param>
		public void Exit(GamePlayer player, bool silent)
		{
			player.MoveTo(OutdoorJumpPoint);

			if (!silent)
			{
				ChatUtil.SendSystemMessage(player, "House.Exit.LeftHouse", HouseNumber);
			}

			player.Out.SendExitHouse(this);

			IList<GamePlayer> list = GetAllPlayersInHouse();
			if (list.Count == 0)
			{
				foreach (GamePlayer pl in player.GetPlayersInRadius(HousingConstants.HouseViewingDistance))
				{
					pl.Out.SendHouseOccupied(this, false);
				}
			}
		}

		/// <summary>
		/// Sends the house info window to a player
		/// </summary>
		/// <param name="player">the player</param>
		public void SendHouseInfo(GamePlayer player)
		{
			int level = Model - ((Model - 1)/4)*4;
			TimeSpan due = (LastPaid.AddDays(7).AddHours(1) - DateTime.Now);
			var text = new List<string>();

			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Owner", Name));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Lotnum", HouseNumber));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Level", level));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Porch"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.PorchEnabled", (Porch ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.PorchRoofColor", PorchRoofColor));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.ExteriorMaterials"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.RoofMaterial", RoofMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.WallMaterial", WallMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.DoorMaterial", DoorMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.TrussMaterial", TrussMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.PorchMaterial", PorchMaterial));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.WindowMaterial", WindowMaterial));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.ExteriorUpgrades"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.OutdoorGuildBanner",
			                                    ((OutdoorGuildBanner) ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.OutdoorGuildShield",
			                                    ((OutdoorGuildShield) ? "Y" : "N")));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.InteriorUpgrades"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.IndoorGuildBanner",
			                                    ((IndoorGuildBanner) ? "Y" : "N")));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.IndoorGuildShield",
			                                    ((IndoorGuildShield) ? "Y" : "N")));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.InteriorCarpets"));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Rug1Color", Rug1Color));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Rug2Color", Rug2Color));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Rug3Color", Rug3Color));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Rug4Color", Rug4Color));
			text.Add(" ");
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.Lockbox", Money.GetString(KeptMoney)));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.RentalPrice",
			                                    Money.GetString(HouseMgr.GetRentByModel(Model))));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.MaxLockbox",
			                                    Money.GetString(HouseMgr.GetRentByModel(Model)*4)));
			text.Add(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.RentDueIn", due.Days, due.Hours));

			player.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(player.Client, "House.SendHouseInfo.HouseOwner", Name),
			                                text);
		}

		public int GetPorchAndGuildEmblemFlags()
		{
			int flag = 0;

			if (Porch)
				flag |= 1;

			if (OutdoorGuildBanner)
				flag |= 2;

			if (OutdoorGuildShield)
				flag |= 4;

			return flag;
		}

		public int GetGuildEmblemFlags()
		{
			int flag = 0;

			if (IndoorGuildBanner)
				flag |= 1;

			if (IndoorGuildShield)
				flag |= 2;

			return flag;
		}

		/// <summary>
		/// Returns a ArrayList with all players in the house
		/// </summary>
		public IList<GamePlayer> GetAllPlayersInHouse()
		{
			var ret = new List<GamePlayer>();
			foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(RegionID, X, Y, 25000, WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player.CurrentHouse == this && player.InHouse)
				{
					ret.Add(player);
				}
			}

			return ret;
		}

		/// <summary>
		/// Find a number that can be used for this vault.
		/// </summary>
		/// <returns></returns>
		public int GetFreeVaultNumber()
		{
			var usedVaults = new[] {false, false, false, false};
			string sqlWhere = string.Format("HouseID = '{0}' AND ItemTemplateID LIKE '%_vault'", HouseNumber);

			foreach (DBHousepointItem housePointItem in GameServer.Database.SelectObjects<DBHousepointItem>(sqlWhere))
			{
				if (housePointItem.Index >= 0 && housePointItem.Index <= 3)
				{
					usedVaults[housePointItem.Index] = true;
				}
			}

			for (int freeVault = 0; freeVault <= 3; ++freeVault)
			{
				if (!usedVaults[freeVault])
				{
					return freeVault;
				}
			}

			return -1;
		}

		#region Hookpoints

		public static bool AddNewOffset(HouseHookpointOffset o)
		{
			if (o.Hookpoint <= HousingConstants.MaxHookpointLocations)
			{
				HousingConstants.RelativeHookpointsCoords[o.Model][o.Hookpoint] = new[] {o.OffX, o.OffY, o.OffZ, o.OffH};
				return true;
			}

			Log.Error("[Housing]: HouseHookPointOffset exceeds array size.  Model " + o.Model + ", hookpoint " + o.Hookpoint);

			return false;
		}

		public static void LoadHookpointOffsets()
		{
			//initialise array
			for (int i = 12; i > 0; i--)
			{
				for (int j = 1; j < HousingConstants.RelativeHookpointsCoords[i].Length; j++)
				{
					HousingConstants.RelativeHookpointsCoords[i][j] = null;
				}
			}

			IList<HouseHookpointOffset> objs = GameServer.Database.SelectAllObjects<HouseHookpointOffset>();
			foreach (HouseHookpointOffset o in objs)
			{
				AddNewOffset(o);
			}
		}

		public Point3D GetHookpointLocation(uint n)
		{
			if (n > HousingConstants.MaxHookpointLocations)
				return null;

			int[] hookpointsCoords = HousingConstants.RelativeHookpointsCoords[Model][n];

			if (hookpointsCoords == null)
				return null;

			return new Point3D(X + hookpointsCoords[0], Y + hookpointsCoords[1], 25000 + hookpointsCoords[2]);
		}

		private int GetHookpointPosition(int objX, int objY, int objZ)
		{
			int position = 16; //start with a position it can never be that can be checked for

			for (int i = 0; i < 15; i++)
			{
				if (HousingConstants.RelativeHookpointsCoords[Model][i] != null)
				{
					if (HousingConstants.RelativeHookpointsCoords[Model][i][0] + X == objX &&
					    HousingConstants.RelativeHookpointsCoords[Model][i][1] + Y == objY)
					{
						position = i;
					}
				}
			}

			return position;
		}

		private ushort GetHookpointHeading(uint n)
		{
			if (n > HousingConstants.MaxHookpointLocations)
				return 0;

			int[] hookpointsCoords = HousingConstants.RelativeHookpointsCoords[Model][n];

			if (hookpointsCoords == null)
				return 0;

			return (ushort) (Heading + hookpointsCoords[3]);
		}

		/// <summary>
		/// Fill a hookpoint with an object, create it in the database.
		/// </summary>
		/// <param name="item">The itemtemplate of the item used to fill the hookpoint (can be null if templateid is filled)</param>
		/// <param name="position">The position of the hookpoint</param>
		/// <param name="templateID">The template id of the item (can be blank if item is filled)</param>
		public GameObject FillHookpoint(ItemTemplate item, uint position, string templateID)
		{
			if (item == null)
			{
				item = GameServer.Database.SelectObject<ItemTemplate>("Id_nb = '" + GameServer.Database.Escape(templateID) + "'");

				if (item == null)
					return null;
			}


			//get location from slot
			IPoint3D location = GetHookpointLocation(position);
			if (location == null)
				return null;

			int x = location.X;
			int y = location.Y;
			int z = location.Z;
			ushort heading = GetHookpointHeading(position);
			GameStaticItem sItem = null;

			switch ((eObjectType) item.Object_Type)
			{
				case eObjectType.HouseNPC:
					{
						NpcTemplate npt = NpcTemplateMgr.GetTemplate(item.Bonus);
						if (npt == null)
							return null;

						try
						{
							string defaultClassType = npt.ClassType;
							if (string.IsNullOrEmpty(npt.ClassType))
							{
								defaultClassType="DOL.GS.GameNPC";
								Log.Warn ("[housing] null classtype in hookpoint attachment, using GameNPC instead");
							}

							var hNPC = (GameNPC)Assembly.GetAssembly(typeof(GameServer)).CreateInstance(defaultClassType, false);
							if (hNPC == null)
							{
								foreach (Assembly asm in ScriptMgr.Scripts)
								{
									hNPC = (GameNPC)asm.CreateInstance(npt.ClassType, false);
									if (hNPC != null) break;
								}
							}

							if (hNPC == null)
							{
								HouseMgr.Log.Error("[Housing] Can't create instance of type: " + npt.ClassType);
								return null;
							}

							hNPC.LoadTemplate(npt);

							hNPC.Name = item.Name;
							hNPC.CurrentHouse = this;
							hNPC.InHouse = true;
							hNPC.X = x;
							hNPC.Y = y;
							hNPC.Z = z;
							hNPC.Heading = heading;
							hNPC.CurrentRegionID = RegionID;
							hNPC.Realm = (eRealm)item.Realm;
							hNPC.Flags ^= GameNPC.eFlags.PEACE;
							hNPC.AddToWorld();
						}
						catch (Exception ex)
						{
							Log.Error("Error filling housing hookpoint using npc template " + npt.Name, ex);
						}

						break;
					}
				case eObjectType.HouseBindstone:
					{
						sItem = new GameStaticItem();
						sItem.CurrentHouse = this;
						sItem.InHouse = true;
						sItem.X = x;
						sItem.Y = y;
						sItem.Z = z;
						sItem.Heading = heading;
						sItem.CurrentRegionID = RegionID;
						sItem.Name = item.Name;
						sItem.Model = (ushort) item.Model;
						sItem.AddToWorld();
						//0:07:45.984 S=>C 0xD9 item/door create v171 (oid:0x0DDB emblem:0x0000 heading:0x0DE5 x:596203 y:530174 z:24723 model:0x05D2 health:  0% flags:0x04(realm:0) extraBytes:0 unk1_171:0x0096220C name:"Hibernia bindstone")
						//add bind point
						break;
					}
				case eObjectType.HouseInteriorObject:
					{
						sItem = new GameStaticItem();
						sItem.CurrentHouse = this;
						sItem.InHouse = true;
						sItem.X = x;
						sItem.Y = y;
						sItem.Z = z;
						sItem.Heading = heading;
						sItem.CurrentRegionID = RegionID;
						sItem.Name = item.Name;
						sItem.Model = (ushort) item.Model;
						sItem.AddToWorld();
						break;
					}
			}
			return sItem;
		}

		public void EmptyHookpoint(GamePlayer player, GameObject obj)
		{
			if (!CanEmptyHookpoint(player))
			{
				ChatUtil.SendSystemMessage(player, "Only the Owner of a House can remove or place Items on Hookpoints!");
				return;
			}

			int posi = GetHookpointPosition(obj.X, obj.Y, obj.Z);
			if (posi == 16) //the standard return value if none is found
				return;

			//get the housepoint item
			string sqlWhere = string.Format("Position = '{0}' AND HouseID = '{1}'", posi, obj.CurrentHouse.HouseNumber);

			var item = GameServer.Database.SelectObject<DBHousepointItem>(sqlWhere);
			if (item == null)
				return;

			obj.Delete();
			GameServer.Database.DeleteObject(item);

			// Need to clear the current house points so we can replace items
			player.CurrentHouse.HousepointItems.Clear();
			/* what is this used for? it causes errors and there is no reason for it
			foreach (DBHousepointItem hpitem in GameServer.Database.SelectObjects(typeof(DBHousepointItem), "HouseID = '" + player.CurrentHouse.HouseNumber + "'"))
			{
				FillHookpoint(null, hpitem.Position, hpitem.ItemTemplateID);
				this.HousepointItems[hpitem.Position] = hpitem;
			} */

			player.CurrentHouse.SendUpdate();

			var template = GameServer.Database.SelectObject<ItemTemplate>("Name = '" + GameServer.Database.Escape(obj.Name) + "'");
			if (template != null)
			{
				player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, GameInventoryItem.Create<ItemTemplate>(template));
			}
		}

		#endregion

		#region Editing

		public bool AddPorch()
		{
			if (Porch) // we can't add an already added porch
				return false;

			// set porch to true, we now have one!
			Porch = true;

			// broadcast updates
			SendUpdate();
			SaveIntoDatabase();

			return true;
		}

		public bool RemovePorch()
		{
			if (!Porch) // we can't remove an already removed porch
				return false;

			// remove the consignment merchant
			RemoveConsignment();

			// set porch to false, no mo porch!
			Porch = false;

			// broadcast updates
			SendUpdate();
			SaveIntoDatabase();

			return true;
		}

		public bool AddConsignment(int startValue)
		{
			// check to make sure a consignment merchant doesn't already exist for this house.
			var obj = GameServer.Database.SelectObject<Mob>("HouseNumber = '" + HouseNumber + "'");
			if (obj != null)
				return false;

			var merchant = GameServer.Database.SelectObject<DBHouseMerchant>("HouseNumber = '" + HouseNumber + "'");
			if (merchant != null)
				return false;

			// create a new consignment merchant entry, and add it to the DB
			var newM = new DBHouseMerchant {HouseNumber = HouseNumber, Quantity = startValue};
			GameServer.Database.AddObject(newM);

			float[] consignmentCoords = HousingConstants.ConsignmentPositioning[Model];
			double multi = consignmentCoords[0];
			var range = (int) consignmentCoords[1];
			var zaddition = (int) consignmentCoords[2];
			var realm = (int) consignmentCoords[3];

			double angle = Heading*((Math.PI*2)/360); // angle*2pi/360;
			var heading = (ushort) ((Heading < 180 ? Heading + 180 : Heading - 180)/0.08789);
			var tX = (int) ((X + (500*Math.Sin(angle))) - Math.Sin(angle - multi)*range);
			var tY = (int) ((Y - (500*Math.Cos(angle))) + Math.Cos(angle - multi)*range);

			var con = new GameConsignmentMerchant
			{
				CurrentRegionID = RegionID,
				X = tX,
				Y = tY,
				Z = Z + zaddition,
				Level = 50,
				Realm = (eRealm) realm,
				HouseNumber = (ushort)HouseNumber,
				Name = "GameConsignmentMerchant Merchant",
				Heading = heading,
				Model = 144
			};

			con.Flags |= GameNPC.eFlags.PEACE;
			con.LoadedFromScript = false;
			con.RoamingRange = 0;

			if (DatabaseItem.GuildHouse)
				con.GuildName = DatabaseItem.GuildName;

			con.AddToWorld();
			con.SaveIntoDatabase();

			DatabaseItem.HasConsignment = true;
			SaveIntoDatabase();

			return true;
		}

		public void RemoveConsignment()
		{
			var npcmob = GameServer.Database.SelectObject<Mob>("HouseNumber = '" + HouseNumber + "'");
			if (npcmob != null)
			{
				GameNPC[] npc = WorldMgr.GetNPCsByNameFromRegion(npcmob.Name, npcmob.Region, (eRealm) npcmob.Realm);

				foreach (GameNPC hnpc in npc)
				{
					if (hnpc.HouseNumber == HouseNumber)
					{
						var itemcon = GameServer.Database.SelectObjects<InventoryItem>("OwnerID = '" + this.OwnerID + "' AND SlotPosition >= 1500 AND SlotPosition <= 1599");
						if (itemcon.Count > 0)
						{
							for (int i = 0; i < itemcon.Count; i++)
							{
								itemcon[i].OwnerLot = 0;
								GameServer.Database.SaveObject(itemcon[i]);
							}
						}
						hnpc.DeleteFromDatabase();
						hnpc.Delete();
					}
				}
			}

			var merchant = GameServer.Database.SelectObject<DBHouseMerchant>("HouseNumber = '" + HouseNumber + "'");
			if (merchant != null)
			{
				GameServer.Database.DeleteObject(merchant);
			}

			_consignment = null;
			DatabaseItem.HasConsignment = false;

			SaveIntoDatabase();
		}

		public void Edit(GamePlayer player, List<int> changes)
		{
			MerchantTradeItems items = player.InHouse ? HouseTemplateMgr.IndoorMenuItems : HouseTemplateMgr.OutdoorMenuItems;
			if (items == null)
				return;

			if (!player.InHouse)
			{
				if (!CanChangeExternalAppearance(player))
					return;
			}
			else
			{
				if (!CanChangeInterior(player, DecorationPermissions.Add))
					return;
			}

			long price = 0;

			// tally up the price for all the requested changes
			foreach (int slot in changes)
			{
				int page = slot/30;
				int pslot = slot%30;

				ItemTemplate item = items.GetItem(page, (eMerchantWindowSlot) pslot);
				if (item != null)
				{
					price += item.Price;
				}
			}

			// make sure player has enough money to cover the changes
			if (!player.RemoveMoney(price))
			{
				ChatUtil.SendMerchantMessage(player, "House.Edit.NotEnoughMoney", null);
				return;
			}

			ChatUtil.SendSystemMessage(player, "House.Edit.PayForChanges", Money.GetString(price));

			// make all the changes
			foreach (int slot in changes)
			{
				int page = slot/30;
				int pslot = slot%30;

				ItemTemplate item = items.GetItem(page, (eMerchantWindowSlot) pslot);

				if (item != null)
				{
					switch ((eObjectType) item.Object_Type)
					{
						case eObjectType.HouseInteriorBanner:
							IndoorGuildBanner = (item.DPS_AF == 1 ? true : false);
							break;
						case eObjectType.HouseInteriorShield:
							IndoorGuildShield = (item.DPS_AF == 1 ? true : false);
							break;
						case eObjectType.HouseCarpetFirst:
							Rug1Color = item.DPS_AF;
							break;
						case eObjectType.HouseCarpetSecond:
							Rug2Color = item.DPS_AF;
							break;
						case eObjectType.HouseCarpetThird:
							Rug3Color = item.DPS_AF;
							break;
						case eObjectType.HouseCarpetFourth:
							Rug4Color = item.DPS_AF;
							break;
						case eObjectType.HouseTentColor:
							PorchRoofColor = item.DPS_AF;
							break;
						case eObjectType.HouseExteriorBanner:
							OutdoorGuildBanner = (item.DPS_AF == 1 ? true : false);
							break;
						case eObjectType.HouseExteriorShield:
							OutdoorGuildShield = (item.DPS_AF == 1 ? true : false);
							break;
						case eObjectType.HouseRoofMaterial:
							RoofMaterial = item.DPS_AF;
							break;
						case eObjectType.HouseWallMaterial:
							WallMaterial = item.DPS_AF;
							break;
						case eObjectType.HouseDoorMaterial:
							DoorMaterial = item.DPS_AF;
							break;
						case eObjectType.HousePorchMaterial:
							PorchMaterial = item.DPS_AF;
							break;
						case eObjectType.HouseWoodMaterial:
							TrussMaterial = item.DPS_AF;
							break;
						case eObjectType.HouseShutterMaterial:
							WindowMaterial = item.DPS_AF;
							break;
						default:
							break; //dirty work a round - dont know how mythic did it, hardcoded? but it works
					}
				}
			}

			// save the house
			GameServer.Database.SaveObject(_databaseItem);

			if (player.InHouse)
			{
				// update all players in the house
				foreach (GamePlayer p in GetAllPlayersInHouse())
				{
					p.Out.SendEnterHouse(this); //update rugs.
				}
			}
			else
			{
				// update all players nearby
				foreach (GamePlayer p in WorldMgr.GetPlayersCloseToSpot(this, HousingConstants.HouseViewingDistance))
				{
					p.Out.SendHouse(this); //update wall look
					p.Out.SendGarden(this);
				}
			}
		}

		#endregion

		#region Permissions

		#region Add/Remove/Edit

		public bool AddPermission(GamePlayer player, PermissionType permType, int permLevel)
		{
			// make sure player is not null
			if (player == null)
				return false;

			// get the proper target name (acct name or player name)
			string targetName = permType == PermissionType.Account ? player.Client.Account.Name : player.Name;

			//  check to make sure an existing mapping doesn't exist.
			foreach (DBHouseCharsXPerms perm in _housePermissions.Values)
			{
				// fast expression to evaluate to match appropriate permissions
				if (perm.PermissionType == (int) permType)
				{
					// make sure it's not identical, which would mean we couldn't add!
					if (perm.TargetName == targetName)
						return false;
				}
			}

			// no matching permissions, create a new one and add it.
			var housePermission = new DBHouseCharsXPerms(HouseNumber, targetName, player.Name, permLevel, (int) permType);
			GameServer.Database.AddObject(housePermission);

			// add it to our list
			_housePermissions.Add(GetOpenPermissionSlot(), housePermission);

			return true;
		}

		public bool AddPermission(string targetName, PermissionType permType, int permLevel)
		{
			//  check to make sure an existing mapping doesn't exist.
			foreach (DBHouseCharsXPerms perm in _housePermissions.Values)
			{
				// fast expression to evaluate to match appropriate permissions
				if (perm.PermissionType == (int) permType)
				{
					// make sure it's not identical, which would mean we couldn't add!
					if (perm.TargetName == targetName)
						return false;
				}
			}

			// no matching permissions, create a new one and add it.
			var housePermission = new DBHouseCharsXPerms(HouseNumber, targetName, targetName, permLevel, (int) permType);
			GameServer.Database.AddObject(housePermission);

			// add it to our list
			_housePermissions.Add(GetOpenPermissionSlot(), housePermission);

			return true;
		}

		/// <summary>
		/// Grabs the first available house permission slot.
		/// </summary>
		/// <returns>returns an open house permission slot as an int</returns>
		private int GetOpenPermissionSlot()
		{
			int startingSlot = 0;

			while(_housePermissions.ContainsKey(startingSlot))
			{
				startingSlot++;
			}

			return startingSlot;
		}

		public void RemovePermission(int slot)
		{
			// make sure the permission exists
			if (!_housePermissions.ContainsKey(slot))
				return;

			var matchedPerm = _housePermissions[slot];

			// remove the permission and delete it from the database
			_housePermissions.Remove(slot);
			GameServer.Database.DeleteObject(matchedPerm);
		}

		public void AdjustPermissionSlot(int slot, int newPermLevel)
		{
			// make sure the permission exists
			if (!_housePermissions.ContainsKey(slot))
				return;

			var permission = _housePermissions[slot];

			// check for proper permission level range
			if (newPermLevel < HousingConstants.MinPermissionLevel || newPermLevel > HousingConstants.MaxPermissionLevel)
				return;

			// update the permission level
			permission.PermissionLevel = newPermLevel;

			// save the permission
			GameServer.Database.SaveObject(permission);
		}

		#endregion

		#region Get Permissions

		private DBHouseCharsXPerms GetPlayerPermissions(GamePlayer player)
		{
			// make sure player isn't null
			if (player == null)
				return null;

			// try character permissions first
			IEnumerable<DBHouseCharsXPerms> charPermissions = from cp in _housePermissions.Values
				where
				cp.TargetName == player.Name &&
				cp.PermissionType == (int) PermissionType.Player
				select cp;

			if (charPermissions.Count() > 0)
				return charPermissions.First();

			// try account permissions next
			IEnumerable<DBHouseCharsXPerms> acctPermissions = from cp in _housePermissions.Values
				where
				cp.TargetName == player.Client.Account.Name &&
				cp.PermissionType == (int) PermissionType.Account
				select cp;

			if (acctPermissions.Count() > 0)
				return acctPermissions.First();

			if (player.Guild != null)
			{
				// try guild permissions next
				IEnumerable<DBHouseCharsXPerms> guildPermissions = from cp in _housePermissions.Values
					where
					player.Guild.Name == cp.TargetName &&
					cp.PermissionType == (int) PermissionType.Guild
					select cp;

				if (guildPermissions.Count() > 0)
					return guildPermissions.First();
			}

			// look for the catch-all permissions last
			IEnumerable<DBHouseCharsXPerms> allPermissions = from cp in _housePermissions.Values
				where cp.TargetName == "All"
				select cp;

			if (allPermissions.Count() > 0)
				return allPermissions.First();

			// nothing found, return null
			return null;
		}

		private bool HasAccess(GamePlayer player, Func<DBHousePermissions, bool> accessExpression)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and GMs+ can do everything
			if (HasOwnerPermissions(player) || PrivilegeMgr.IsGameMaster(player))
				return true;

			// get house permissions for the given player
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			// get result of the permission check expression
			return accessExpression(housePermissions);
		}

		private DBHousePermissions GetPermissionLevel(GamePlayer player)
		{
			// get player permissions mapping
			DBHouseCharsXPerms permissions = GetPlayerPermissions(player);

			if (permissions == null)
				return null;

			// get house permissions for the given mapping
			DBHousePermissions housePermissions = GetPermissionLevel(permissions);

			return housePermissions;
		}

		private DBHousePermissions GetPermissionLevel(DBHouseCharsXPerms charPerms)
		{
			// make sure permissions aren't null
			if (charPerms == null)
				return null;

			return GetPermissionLevel(charPerms.PermissionLevel);
		}

		private DBHousePermissions GetPermissionLevel(int permissionLevel)
		{
			DBHousePermissions permissions;
			_permissionLevels.TryGetValue(permissionLevel, out permissions);

			return permissions;
		}

		public bool HasOwnerPermissions(GamePlayer player)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// check by character name/account if not guild house
			if (!_databaseItem.GuildHouse)
			{
				// check if character is explicit owner
				if (_databaseItem.OwnerID == player.DBCharacter.ObjectId)
					return true;

				// check account-wide if not a guild house
				IEnumerable<DOLCharacters> charsOnAccount = from chr in player.Client.Account.Characters
					where chr.ObjectId == _databaseItem.OwnerID
					select chr;

				if (charsOnAccount.Count() > 0)
					return true;
			}

			// check based on guild
			if (player.Guild != null)
			{
				return OwnerID == player.Guild.GuildID && player.Guild.GotAccess(player, Guild.eRank.Leader);
			}

			// no character/account/guild match, not an owner
			return false;
		}

		#endregion

		#region Check Permissions

		public bool CanEmptyHookpoint(GamePlayer player)
		{
			return HasOwnerPermissions(player);
		}

		public bool CanEnterHome(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanEnterHouse);
		}

		public bool CanUseVault(GamePlayer player, GameHouseVault vault, VaultPermissions vaultPerms)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and GMs+ can do everything
			if (HasOwnerPermissions(player) || PrivilegeMgr.IsGameMaster(player))
				return true;

			// get player house permissions
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			// get the vault permissions for the given vault
			VaultPermissions activeVaultPermissions = VaultPermissions.None;

			switch (vault.Index)
			{
				case 0:
					activeVaultPermissions = (VaultPermissions) housePermissions.Vault1;
					break;
				case 1:
					activeVaultPermissions = (VaultPermissions) housePermissions.Vault2;
					break;
				case 2:
					activeVaultPermissions = (VaultPermissions) housePermissions.Vault3;
					break;
				case 3:
					activeVaultPermissions = (VaultPermissions) housePermissions.Vault4;
					break;
			}

			return (activeVaultPermissions & vaultPerms) > 0;
		}

		public bool CanUseConsignmentMerchant(GamePlayer player, ConsignmentPermissions consignPerms)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and GMs+ can do everything
			if (HasOwnerPermissions(player) || PrivilegeMgr.IsGameMaster(player))
				return true;

			// get player house permissions
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			return ((ConsignmentPermissions) housePermissions.ConsignmentMerchant & consignPerms) > 0;
		}

		public bool CanChangeInterior(GamePlayer player, DecorationPermissions interiorPerms)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and GMs+ can do everything
			if (HasOwnerPermissions(player) || PrivilegeMgr.IsGameMaster(player))
				return true;

			// get player house permissions
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			return ((DecorationPermissions) housePermissions.ChangeInterior & interiorPerms) > 0;
		}

		public bool CanChangeGarden(GamePlayer player, DecorationPermissions gardenPerms)
		{
			// make sure player isn't null
			if (player == null)
				return false;

			// owner and GMs+ can do everything
			if (HasOwnerPermissions(player) || PrivilegeMgr.IsGameMaster(player))
				return true;

			// get player house permissions
			DBHousePermissions housePermissions = GetPermissionLevel(player);

			if (housePermissions == null)
				return false;

			return ((DecorationPermissions) housePermissions.ChangeGarden & gardenPerms) > 0;
		}

		public bool CanChangeExternalAppearance(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanChangeExternalAppearance);
		}

		public bool CanBanish(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanBanish);
		}

		public bool CanBindInHouse(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanBindInHouse);
		}

		public bool CanPayRent(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanPayRent);
		}

		public bool CanUseMerchants(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanUseMerchants);
		}

		public bool CanUseTools(GamePlayer player)
		{
			// check if player has access
			return HasAccess(player, cp => cp.CanUseTools);
		}

		#endregion

		#endregion

		#region Database

		/// <summary>
		/// Saves this house into the database
		/// </summary>
		public void SaveIntoDatabase()
		{
			GameServer.Database.SaveObject(_databaseItem);
		}

		/// <summary>
		/// Load a house from the database
		/// </summary>
		public void LoadFromDatabase()
		{
			int i = 0;
			foreach (
				DBHouseIndoorItem dbiitem in
				GameServer.Database.SelectObjects<DBHouseIndoorItem>("HouseNumber = '" + HouseNumber + "'"))
			{
				_indoorItems.Add(i++, new IndoorItem(dbiitem));
			}

			i = 0;
			foreach (
				DBHouseOutdoorItem dboitem in
				GameServer.Database.SelectObjects<DBHouseOutdoorItem>("HouseNumber = '" + HouseNumber + "'"))
			{
				_outdoorItems.Add(i++, new OutdoorItem(dboitem));
			}

			foreach (
				DBHouseCharsXPerms d in GameServer.Database.SelectObjects<DBHouseCharsXPerms>("HouseNumber = '" + HouseNumber + "'")
			)
			{
				_housePermissions.Add(GetOpenPermissionSlot(), d);
			}

			foreach (
				DBHousePermissions dbperm in
				GameServer.Database.SelectObjects<DBHousePermissions>("HouseNumber = '" + HouseNumber + "'"))
			{
				_permissionLevels.Add(dbperm.PermissionLevel, dbperm);
			}

			foreach (
				DBHousepointItem item in GameServer.Database.SelectObjects<DBHousepointItem>("HouseID = '" + HouseNumber + "'"))
			{
				if (item.ItemTemplateID.EndsWith("_vault"))
				{
					var template =
						GameServer.Database.SelectObject<ItemTemplate>("Id_nb = '" + GameServer.Database.Escape(item.ItemTemplateID) + "'");
					if (template == null)
						continue;

					var houseVault = new GameHouseVault(template, item.Index);
					houseVault.Attach(this, item);
				}
				else
				{
					FillHookpoint(null, item.Position, item.ItemTemplateID);
				}

				HousepointItems[item.Position] = item;
			}

			// sort character permissions by creation time, oldest to newest
			//HousePermissions.Sort((a, b) => a.CreationTime.CompareTo(b.CreationTime));
		}

		#endregion
	}
}