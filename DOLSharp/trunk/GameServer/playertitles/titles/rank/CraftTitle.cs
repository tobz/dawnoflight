using DawnOfLight.GameServer.Crafting;
using DawnOfLight.GameServer.GameObjects;

namespace DawnOfLight.GameServer.PlayerTitles.titles.rank
{
	public class CraftTitle : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player)
		{
			return player.CraftTitle;
		}
		public override string GetValue(GamePlayer player)
		{
			return player.CraftTitle;
		}
		public override bool IsSuitable(GamePlayer player)
		{
			if (player.CraftingPrimarySkill != eCraftingSkill.NoCrafting)
			{
				return true;
			}
			return false;
		}
	}
}