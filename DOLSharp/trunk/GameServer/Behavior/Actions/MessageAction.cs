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
using DawnOfLight.GameServer.Behavior.Attributes;
using DawnOfLight.GameServer.Events;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.Behavior.Actions
{
    [Action(ActionType = eActionType.Message)]
    public class MessageAction : AbstractAction<String,eTextType>
    {

        public MessageAction(GameNPC defaultNPC,  Object p, Object q)
            : base(defaultNPC, eActionType.Message, p, q)
        {                           
        }


        public MessageAction(GameNPC defaultNPC, String message, eTextType messageType)
            : this(defaultNPC, (object)message, (object)messageType) { }
        


        public override void Perform(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);
            String message = BehaviourUtils.GetPersonalizedMessage(P, player);
            switch (Q)
            {
                case eTextType.Dialog:
                    player.Out.SendCustomDialog(message, null);
                    break;
                case eTextType.Emote:
                    player.Out.SendMessage(message, ChatType.CT_Emote, ChatLocation.CL_ChatWindow);
                    break;
				case eTextType.Say:
					player.Out.SendMessage(message, ChatType.CT_Say, ChatLocation.CL_ChatWindow);
					break;
				case eTextType.SayTo:
					player.Out.SendMessage(message, ChatType.CT_System, ChatLocation.CL_PopupWindow);
					break;
				case eTextType.Yell:
					player.Out.SendMessage(message, ChatType.CT_Help, ChatLocation.CL_ChatWindow);
					break;
                case eTextType.Broadcast:
                    foreach (GameClient clientz in WorldMgr.GetAllPlayingClients())
                    {
                        clientz.Player.Out.SendMessage(message, ChatType.CT_Broadcast, ChatLocation.CL_ChatWindow);
                    }
                    break;
                case eTextType.Read:
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Behaviour.MessageAction.ReadMessage", message), ChatType.CT_Emote, ChatLocation.CL_PopupWindow);
                    break;  
                case eTextType.None:
                    //nohting
                    break;
            }
        }
    }
}
