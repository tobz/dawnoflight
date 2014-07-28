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

using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;

namespace DawnOfLight.GameServer.Utilities
{
	public static class ChatUtil
	{
		public static void SendSystemMessage(GamePlayer target, string message)
		{
			target.Out.SendMessage(message, ChatType.CT_System, ChatLocation.CL_SystemWindow);
		}

		public static void SendSystemMessage(GamePlayer target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target.Client, translationID, args);

			target.Out.SendMessage(translatedMsg, ChatType.CT_System, ChatLocation.CL_SystemWindow);
		}

		public static void SendSystemMessage(GameClient target, string message)
		{
			target.Out.SendMessage(message, ChatType.CT_System, ChatLocation.CL_SystemWindow);
		}

		public static void SendSystemMessage(GameClient target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target, translationID, args);

			target.Out.SendMessage(translatedMsg, ChatType.CT_System, ChatLocation.CL_SystemWindow);
		}

		public static void SendMerchantMessage(GamePlayer target, string message)
		{
			target.Out.SendMessage(message, ChatType.CT_Merchant, ChatLocation.CL_SystemWindow);
		}

		public static void SendMerchantMessage(GamePlayer target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target.Client, translationID, args);

			target.Out.SendMessage(translatedMsg, ChatType.CT_Merchant, ChatLocation.CL_SystemWindow);
		}

		public static void SendMerchantMessage(GameClient target, string message)
		{
			target.Out.SendMessage(message, ChatType.CT_Merchant, ChatLocation.CL_SystemWindow);
		}

		public static void SendMerchantMessage(GameClient target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target, translationID, args);

			target.Out.SendMessage(translatedMsg, ChatType.CT_Merchant, ChatLocation.CL_SystemWindow);
		}

		public static void SendHelpMessage(GamePlayer target, string message)
		{
			target.Out.SendMessage(message, ChatType.CT_Help, ChatLocation.CL_SystemWindow);
		}

		public static void SendHelpMessage(GamePlayer target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target.Client, translationID, args);

			target.Out.SendMessage(translatedMsg, ChatType.CT_Help, ChatLocation.CL_SystemWindow);
		}

		public static void SendHelpMessage(GameClient target, string message)
		{
			target.Out.SendMessage(message, ChatType.CT_Help, ChatLocation.CL_SystemWindow);
		}

		public static void SendHelpMessage(GameClient target, string translationID, params object[] args)
		{
			var translatedMsg = LanguageMgr.GetTranslation(target, translationID, args);

			target.Out.SendMessage(translatedMsg, ChatType.CT_Help, ChatLocation.CL_SystemWindow);
		}

		public static void SendErrorMessage(GamePlayer target, string message)
		{
			SendErrorMessage(target.Client, message);
		}

		public static void SendErrorMessage(GameClient target, string message)
		{
			target.Out.SendMessage(message, ChatType.CT_Important, ChatLocation.CL_SystemWindow);
		}

		public static void SendDebugMessage(GamePlayer target, string message)
		{
			SendDebugMessage(target.Client, message);
		}

		public static void SendDebugMessage(GameClient target, string message)
		{
			if (target.Account.PrivLevel > (int)ePrivLevel.Player)
				target.Out.SendMessage(message, ChatType.CT_Staff, ChatLocation.CL_SystemWindow);
		}
	}
}
