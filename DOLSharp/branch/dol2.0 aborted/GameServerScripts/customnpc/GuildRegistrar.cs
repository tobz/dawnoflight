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

using NHibernate.Mapping.Attributes;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Guild Registrar
	/// </summary>	
	[Subclass(NameType=typeof(GuildRegistrar), ExtendsType=typeof(GameMob))] 
	public class GuildRegistrar : GameMob
	{
		protected const string FORM_A_GUILD = "form a guild";

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;

			TurnTo(player, 10000);
			SayTo(player, "Hail, " + player.CharacterClass.Name + ". Have you come to [" + FORM_A_GUILD + "]?");

			return true;
		}

		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;

			GamePlayer player = source as GamePlayer;
			if(player == null) return false;

			switch (text)
			{
				case FORM_A_GUILD:
					SayTo(player, "Well, then. This can be done. Gather together eight who would join with you, and bring them here. The price will be one gold. After I am paid, use /gc form <guildname>. Then I will ask you all if you wish to form such a guild. All must choose to form the guild. It's quite simple, really.");
					break;
			}

			return true;
		}
	}
}