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

using System.Reflection;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.GameObjects.CustomNPC;
using DawnOfLight.GameServer.Housing;
using DawnOfLight.GameServer.Utilities;
using log4net;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.PlayerWithdrawMerchantMoney, ClientStatus.PlayerInGame)]
    public class PlayerWithdrawMerchantMoneyHandler : IPacketHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void HandlePacket(GameClient client, GamePacketIn packet)
        {
			var conMerchant = client.Player.ActiveInventoryObject as GameConsignmentMerchant;
            if (conMerchant == null)
                return;

			// current house is null, return
            var house = HouseMgr.GetHouse(conMerchant.HouseNumber);
            if (house == null)
                return;

			// make sure player has permissions to withdraw from the consignment merchant
            if (!house.CanUseConsignmentMerchant(client.Player, ConsignmentPermissions.Withdraw))
            {
                client.Player.Out.SendMessage("You don't have permission to withdraw money from this merchant!", ChatType.CT_Important, ChatLocation.CL_ChatWindow);
                return;
            }

			lock (conMerchant.LockObject())
			{
				long totalConMoney = conMerchant.TotalMoney;

				if (totalConMoney > 0)
				{
					if (ServerProperties.Properties.CONSIGNMENT_USE_BP)
					{
						client.Player.Out.SendMessage("You withdraw " + totalConMoney.ToString() + " BountyPoints from your Merchant.", ChatType.CT_Important, ChatLocation.CL_ChatWindow);
						client.Player.BountyPoints += totalConMoney;
						client.Player.Out.SendUpdatePoints();
					}
					else
					{
						ChatUtil.SendMerchantMessage(client, "GameMerchant.OnPlayerWithdraw", Money.GetString(totalConMoney));
						client.Player.AddMoney(totalConMoney);
						InventoryLogging.LogInventoryAction(conMerchant, client.Player, eInventoryActionType.Merchant, totalConMoney);
					}

					conMerchant.TotalMoney -= totalConMoney;

					if (ServerProperties.Properties.MARKET_ENABLE_LOG)
					{
						log.DebugFormat("CM: [{0}:{1}] withdraws {2} from CM on lot {3}.", client.Player.Name, client.Account.Name, totalConMoney, conMerchant.HouseNumber);
					}

					client.Out.SendConsignmentMerchantMoney(conMerchant.TotalMoney);
				}
			}
        }
    }
}