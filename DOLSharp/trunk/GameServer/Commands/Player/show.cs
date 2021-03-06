using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.commands.Player
{
	[Command(
		"&show",
		ePrivLevel.Player,
		"Show all your cards to the other players (all cards become 'up').",
		"/show")]
	public class ShowCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			CardMgr.Show(client);
		}
	}
}