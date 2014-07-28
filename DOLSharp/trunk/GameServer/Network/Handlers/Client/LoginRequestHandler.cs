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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DawnOfLight.Base;
using DawnOfLight.Database;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.ServerProperties;
using DawnOfLight.GameServer.Utilities;
using DawnOfLight.GameServer.World;
using log4net;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
    [PacketHandler(PacketType.TCP, ClientPackets.LoginRequest)]
	public class LoginRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			string ipAddress = client.TcpEndpointAddress;

			packet.Skip(2); //Skip the client_type byte
			var major = (byte)packet.ReadByte();
			var minor = (byte)packet.ReadByte();
			var build = (byte)packet.ReadByte();
			string password = packet.ReadString(20);

			bool v174;
			//the logger detection we had is no longer working
			//bool loggerUsing = false;
			switch (client.Version)
			{
				case GameClient.eClientVersion.Version168:
				case GameClient.eClientVersion.Version169:
				case GameClient.eClientVersion.Version170:
				case GameClient.eClientVersion.Version171:
				case GameClient.eClientVersion.Version172:
				case GameClient.eClientVersion.Version173:
					v174 = false;
					break;
				default:
					v174 = true;
					break;
			}

			if (v174)
			{
				packet.Skip(11);
			}
			else
			{
				packet.Skip(7);
			}

			uint c2 = packet.ReadInt();
			uint c3 = packet.ReadInt();
			uint c4 = packet.ReadInt();

			if (v174)
			{
				packet.Skip(27);
			}
			else
			{
				packet.Skip(31);
			}

			string userName = packet.ReadString(20);
			/*
			if (c2 == 0 && c3 == 0x05000000 && c4 == 0xF4000000)
			{
				loggerUsing = true;
				Log.Warn("logger detected (" + username + ")");
			}*/

			// check server status
			if (GameServer.Instance.ServerStatus == eGameServerStatus.GSS_Closed)
            {
                client.IsConnected = false;
				client.Out.SendLoginDenied(eLoginError.GameCurrentlyClosed);
				GameServer.Instance.Disconnect(client);

				return;
			}

			if (!GameServer.ServerRules.IsAllowedToConnect(client, userName))
			{
				GameServer.Instance.Disconnect(client);
				return;
			}


			try
			{
				Account playerAccount;

				// Make sure that client won't quit
				lock (client)
				{
					GameClient.eClientState state = client.ClientState;
					if (state != GameClient.eClientState.NotConnected)
					{
						Log.DebugFormat("wrong client state on connect {0} {1}", userName, state);
						return;
					}

					// check client already connected
					GameClient findclient = WorldMgr.GetClientByAccountName(userName, true);
					if (findclient != null)
					{
						client.IsConnected = false;
						                            
						if (findclient.ClientState == GameClient.eClientState.Connecting)
						{
							client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIn);
							return;
						}

						if (findclient.ClientState == GameClient.eClientState.Linkdead)
						{
							client.Out.SendLoginDenied(eLoginError.AccountIsInLogoutProcedure);
						}
						else
						{
							client.Out.SendLoginDenied(eLoginError.AccountAlreadyLoggedIn);
						}

						GameServer.Instance.Disconnect(client);
						return;
					}
					
					Regex goodName = new Regex("^[a-zA-Z0-9]*$");
					if (!goodName.IsMatch(userName) || string.IsNullOrWhiteSpace(userName))
					{
						if (Log.IsInfoEnabled)
							Log.Info("Invalid symbols in account name \"" + userName + "\" found!");

						client.IsConnected = false;
						client.Out.SendLoginDenied(eLoginError.AccountInvalid);

						GameServer.Instance.Disconnect(client);
						return;
					}

				    playerAccount = GameServer.Database.FindObjectByKey<Account>(userName);

				    client.PingTime = DateTime.Now.Ticks;

				    if (playerAccount == null)
				    {
				        //check autocreate ...

				        if (GameServer.Instance.Configuration.AutoAccountCreation && Properties.ALLOW_AUTO_ACCOUNT_CREATION)
				        {
				            // autocreate account
				            if (string.IsNullOrEmpty(password))
				            {
				                client.IsConnected = false;
				                client.Out.SendLoginDenied(eLoginError.AccountInvalid);
				                GameServer.Instance.Disconnect(client);

				                if (Log.IsInfoEnabled)
				                    Log.Info("Account creation failed, no password set for Account: " + userName);

				                return;
				            }

				            // check for account bombing
				            TimeSpan ts;
				            var allAccByIp = GameServer.Database.SelectObjects<Account>("LastLoginIP = '" + ipAddress + "'");
				            int totalacc = 0;
				            foreach (Account ac in allAccByIp)
				            {
				                ts = DateTime.Now - ac.CreationDate;
				                if (ts.TotalMinutes < Properties.TIME_BETWEEN_ACCOUNT_CREATION_SAMEIP && totalacc > 1)
				                {
				                    Log.Warn("Account creation: too many from same IP within set minutes - " + userName + " : " + ipAddress);

				                    client.IsConnected = false;
				                    client.Out.SendLoginDenied(eLoginError.PersonalAccountIsOutOfTime);
				                    GameServer.Instance.Disconnect(client);

				                    return;
				                }

				                totalacc++;
				            }

				            if (totalacc >= Properties.TOTAL_ACCOUNTS_ALLOWED_SAMEIP)
				            {
				                Log.Warn("Account creation: too many accounts created from same ip - " + userName + " : " + ipAddress);

				                client.IsConnected = false;
				                client.Out.SendLoginDenied(eLoginError.AccountNoAccessThisGame);
				                GameServer.Instance.Disconnect(client);

				                return;
				            }

				            playerAccount = new Account();
				            playerAccount.Name = userName;
				            playerAccount.Password = Password.HashPassword(password);
				            playerAccount.Realm = 0;
				            playerAccount.CreationDate = DateTime.Now;
				            playerAccount.LastLogin = DateTime.Now;
				            playerAccount.LastLoginIP = ipAddress;
				            playerAccount.LastClientVersion = ((int)client.Version).ToString();
				            playerAccount.Language = Properties.SERV_LANGUAGE;
				            playerAccount.PrivLevel = 1;

				            if (Log.IsInfoEnabled)
				                Log.Info("New account created: " + userName);

				            GameServer.Database.AddObject(playerAccount);

				            // Log account creation
				            AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountCreate, "", userName);
				        }
				        else
				        {
				            if (Log.IsInfoEnabled)
				                Log.Info("No such account found and autocreation deactivated!");

				            client.IsConnected = false;
				            client.Out.SendLoginDenied(eLoginError.AccountNotFound);
				            GameServer.Instance.Disconnect(client);

				            return;
				        }
				    }
				    else
				    {
				        // check password
				        if (!playerAccount.Password.StartsWith("##"))
				        {
				            playerAccount.Password = Password.HashPassword(playerAccount.Password);
				        }

				        if (!Password.HashPassword(password).Equals(playerAccount.Password))
				        {
				            if (Log.IsInfoEnabled)
				                Log.Info("(" + client.TcpEndpoint + ") Wrong password!");

				            client.IsConnected = false;
				            client.Out.SendLoginDenied(eLoginError.WrongPassword);

				            // Log failure
				            AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountFailedLogin, "", userName);

				            GameServer.Instance.Disconnect(client);

				            return;
				        }

				        // save player infos
				        playerAccount.LastLogin = DateTime.Now;
				        playerAccount.LastLoginIP = ipAddress;
				        playerAccount.LastClientVersion = ((int)client.Version).ToString();
				        if (string.IsNullOrEmpty(playerAccount.Language))
				        {
				            playerAccount.Language = Properties.SERV_LANGUAGE;
				        }

				        GameServer.Database.SaveObject(playerAccount);
				    }

				    //Save the account table
					client.Account = playerAccount;

					// create session ID here to disable double login bug
					if (WorldMgr.CreateSessionID(client) < 0)
					{
						if (Log.IsInfoEnabled) Log.InfoFormat("Too many clients connected, denied login to " + playerAccount.Name);

                        client.IsConnected = false;
						client.Out.SendLoginDenied(eLoginError.TooManyPlayersLoggedIn);
						client.Disconnect();

						return;
					}

					client.Out.SendLoginGranted();
					client.ClientState = GameClient.eClientState.Connecting;

					// Log entry
					AuditMgr.AddAuditEntry(client, AuditType.Account, AuditSubtype.AccountSuccessfulLogin, "", userName);
				}
			}
			catch (DatabaseException e)
			{
				if (Log.IsErrorEnabled) Log.Error("LoginRequestHandler", e);

                client.IsConnected = false;
				client.Out.SendLoginDenied(eLoginError.CannotAccessUserAccount);
				GameServer.Instance.Disconnect(client);
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.Error("LoginRequestHandler", e);

				client.Out.SendLoginDenied(eLoginError.CannotAccessUserAccount);
				GameServer.Instance.Disconnect(client);
			}
		}
	}
}