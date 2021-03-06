// Kick by Akira (akira@dataloggin.com)
//
//

using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
	  "&kick",
	  new string[] { "&k" },
	  (int)ePrivLevel.GM,
		"Kicks the player offline of whom you select. USE: KICK <NAME>",
		 "Use: KICK <NAME>")]
	public class KickCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			GameClient clientc = WorldMgr.GetClientByPlayerName(args[1], true, false);
			if (clientc == null)
			{
				client.Out.SendMessage("No one with that name is online to Kick!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
				return 0;
			}
			for (int i = 0; i < 8; i++)
			{
				string message = "You have been removed from the server";
				if (client != null && client.Player != null)
					message += " by GM " + client.Player.Name;
				clientc.Out.SendMessage(message + "!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
				clientc.Out.SendMessage(message + "!", eChatType.CT_Help, eChatLoc.CL_ChatWindow);
			}

			clientc.Out.SendPlayerQuit(true);
			clientc.Player.SaveIntoDatabase();
			clientc.Player.Quit(true);
			return 0;
		}
	}
}