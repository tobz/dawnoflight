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

using System.Collections.Generic;
using System.Reflection;
using DawnOfLight.Database;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.Crafting;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Utilities;
using DawnOfLight.GameServer.World;
using log4net;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.WorldInitRequest, ClientStatus.PlayerInGame)]
	public class WorldInitRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
            client.UdpConfirm = false;

			new WorldInitAction(client.Player).Start(1);
		}

		/// <summary>
		/// Handles player world init requests
		/// </summary>
		protected class WorldInitAction : RegionAction
		{
			/// <summary>
			/// Constructs a new WorldInitAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public WorldInitAction(GamePlayer actionSource)
				: base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				if (player == null) return;
				//check emblems at world load before any updates
				if (player.Inventory != null) 
				{
					lock (player.Inventory)
					{
						Guild playerGuild = player.Guild;
						foreach (InventoryItem myitem in player.Inventory.AllItems)
						{
							if (myitem != null && myitem.Emblem != 0)
							{
								if (playerGuild == null || myitem.Emblem != playerGuild.Emblem)
								{
									myitem.Emblem = 0;
								}
								if (player.Level < 20)
								{
									if (player.CraftingPrimarySkill == eCraftingSkill.NoCrafting)
									{
										myitem.Emblem = 0;
									}
									else
									{
										if (player.GetCraftingSkillValue(player.CraftingPrimarySkill) < 400)
										{
											myitem.Emblem = 0;
										}
									}
								}
							}
						}
					}
				}

				player.Client.ClientState = GameClient.eClientState.WorldEnter;
				if (!player.AddToWorld())
				{
					log.ErrorFormat("Failed to add player to the region! {0}", player.ToString());
					player.Client.Out.SendPlayerQuit(true);
					player.Client.Player.SaveIntoDatabase();
					player.Client.Player.Quit(true);
					player.Client.Disconnect();

					return;
				}

				// this is bind stuff
				// make sure that players doesnt start dead when coming in
				// thats important since if client moves the player it requests player creation
				if (player.Health <= 0)
				{
					player.Health = player.MaxHealth;
				}

				player.Out.SendPlayerPositionAndObjectID();
				player.Out.SendEncumberance(); // Send only max encumberance without used
				player.Out.SendUpdateMaxSpeed();
				player.Out.SendUpdateMaxSpeed(); // Speed after conc buffs
				player.Out.SendStatusUpdate();
				player.Out.SendInventoryItemsUpdate(eInventoryWindowType.Equipment, player.Inventory.EquippedItems);
                player.Out.SendInventoryItemsUpdate(eInventoryWindowType.Inventory, player.Inventory.GetItemRange(InventorySlot.FirstBackpack, InventorySlot.LastBagHorse));
				player.Out.SendUpdatePlayerSkills();   //TODO Insert 0xBE - 08 Various in SendUpdatePlayerSkills() before send spells
				player.Out.SendUpdateCraftingSkills(); // ^
				player.Out.SendUpdatePlayer();
				player.Out.SendUpdateMoney();
				player.Out.SendCharStatsUpdate();

				var friends = player.Friends;
				var onlineFriends = new List<string>();
				foreach (string friendName in friends)
				{
					GameClient friendClient = WorldMgr.GetClientByPlayerName(friendName, true, false);
					if (friendClient == null) 
						continue;

					if (friendClient.Player != null && friendClient.Player.IsAnonymous) 
						continue;

					onlineFriends.Add(friendName);
				}

				player.Out.SendAddFriends(onlineFriends.ToArray());
				player.Out.SendCharResistsUpdate();
				int effectsCount = 0;
				player.Out.SendUpdateIcons(null, ref effectsCount);
				player.Out.SendUpdateWeaponAndArmorStats();
				player.Out.SendQuestListUpdate();
				player.Out.SendStatusUpdate();
				player.Out.SendUpdatePoints();
				player.Out.SendEncumberance();
                player.Out.SendConcentrationList();
				player.Out.SendObjectGuildID(player, player.Guild);
				player.Out.SendDebugMode(player.TempProperties.getProperty<object>(GamePlayer.DEBUG_MODE_PROPERTY, null) != null);
				player.Out.SendUpdateMaxSpeed(); // Speed in debug mode ?

				player.Stealth(false);
				player.Out.SendSetControlledHorse(player);
				//check item at world load
				if (log.IsDebugEnabled)
					log.DebugFormat("Client {0}({1} PID:{2} OID:{3}) entering Region {4}(ID:{5})", player.Client.Account.Name, player.Name, player.Client.SessionID, player.ObjectID, player.CurrentRegion.Description, player.CurrentRegionID);
			}
		}
	}
}
