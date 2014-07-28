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
using System.Reflection;
using DawnOfLight.Database;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.Events;
using DawnOfLight.GameServer.Events.Database;
using DawnOfLight.GameServer.Utilities;
using log4net;

namespace DawnOfLight.GameServer.Network.Handlers.Client
{
	/// <summary>
	/// No longer used after version 1.104
	/// </summary>
    [PacketHandler(PacketType.TCP, ClientPackets.CharacterDeleteRequest, ClientStatus.LoggedIn)]
	public class CharacterDeleteRequestHandler : IPacketHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GamePacketIn packet)
		{
			string charName = packet.ReadString(30);
			DOLCharacters[] chars = client.Account.Characters;

			if (chars == null)
				return;

			for (int i = 0; i < chars.Length; i++)
			{
				if (chars[i].Name.ToLower().Equals(charName.ToLower()))
				{
					if (client.ActiveCharIndex == i)
						client.ActiveCharIndex = -1;

					GameEventMgr.Notify(DatabaseEvent.CharacterDeleted, null, new CharacterEventArgs(chars[i], client));

					// delete items
					try
					{
						var objs = GameServer.Database.SelectObjects<InventoryItem>("OwnerID = '" + GameServer.Database.Escape(chars[i].ObjectId) + "'");
						foreach (var item in objs)
						{
							GameServer.Database.DeleteObject(item);
						}
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Error deleting char items, char OID=" + chars[i].ObjectId, e);
					}

					// delete quests
					try
					{
						var objs = GameServer.Database.SelectObjects<DBQuest>("Character_ID = '" + GameServer.Database.Escape(chars[i].ObjectId) + "'");
						foreach (var quest in objs)
						{
							GameServer.Database.DeleteObject(quest);
						}
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Error deleting char quests, char OID=" + chars[i].ObjectId, e);
					}

					// delete ML steps
					try
					{
						var objs = GameServer.Database.SelectObjects<DBCharacterXMasterLevel>("Character_ID = '" + GameServer.Database.Escape(chars[i].ObjectId) + "'");
						foreach (var mlstep in objs)
						{
							GameServer.Database.DeleteObject(mlstep);
						}
					}
					catch (Exception e)
					{
						if (Log.IsErrorEnabled)
							Log.Error("Error deleting char ml steps, char OID=" + chars[i].ObjectId, e);
					}

					GameServer.Database.DeleteObject(chars[i]);
					client.Account.Characters = null;
					GameServer.Database.FillObjectRelations(client.Account);
					client.Player = null;

					if (client.Account.Characters == null || client.Account.Characters.Length == 0)
					{
						client.Account.Realm = 0;
						GameServer.Database.SaveObject(client.Account);
					}

					// Log deletion
					AuditMgr.AddAuditEntry(client, AuditType.Character, AuditSubtype.CharacterDelete, "", charName);

					break;
				}
			}
		}
	}
}
