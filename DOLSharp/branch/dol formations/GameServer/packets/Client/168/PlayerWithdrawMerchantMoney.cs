﻿/*
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
using System.Reflection;
using DOL.Database;
using DOL.GS.Housing;
using log4net;
using DOL.Language;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, 0x1C, "Withdraw Consignment Merchant Money")]
    public class PlayerWithdrawMerchantMoney : IPacketHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
            if (client.Player == null)
                return 0;
            Consignment con = client.Player.ActiveConMerchant;

            if (con == null)
                return 0;
            House house = HouseMgr.GetHouse(con.HouseNumber);

            if (house == null)
                return 0;

            if (!house.HasOwnerPermissions(client.Player))
            {
                client.Player.Out.SendMessage("You don't have permission to withdraw money from this merchant!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                return 0;
            }

            DBHouseMerchant merchant = GameServer.Database.SelectObject<DBHouseMerchant>("HouseNumber = '" + con.HouseNumber + "'");

            if (merchant.Quantity > 0)
            {
                if (ConsignmentMoney.UseBP)
                {
                    client.Player.Out.SendMessage("You withdraw " + merchant.Quantity.ToString() + " BountyPoints from your Merchant.", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                    client.Player.BountyPoints += merchant.Quantity;
                    client.Player.Out.SendUpdatePoints();
                }
                else
                {
                    string message = LanguageMgr.GetTranslation(client, "GameMerchant.OnPlayerWithdraw", Money.GetString((long)merchant.Quantity));
                    client.Player.AddMoney((long)merchant.Quantity, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
                }
                merchant.Quantity = 0;
                GameServer.Database.SaveObject(merchant);
            }

            return 1;
        }
    }
}