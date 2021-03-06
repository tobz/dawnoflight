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

using System.Collections.Generic;
using DawnOfLight.Database;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Keeps.Relics;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Spells.Teleport
{
	/// <summary>
	/// The spell used for the Personal Bind Recall Stone.
	/// </summary>
	/// <author>Aredhel</author>
	[SpellHandler("GatewayPersonalBind")]
	public class GatewayPersonalBind : SpellHandler
	{
		public GatewayPersonalBind(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine) { }


		/// <summary>
		/// Can this spell be queued with other spells?
		/// </summary>
		public override bool CanQueue
		{
			get { return false; }
		}


		/// <summary>
		/// Whether this spell can be cast on the selected target at all.
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			GamePlayer player = Caster as GamePlayer;
			if (player == null)
				return false;

			if (player.CurrentRegion.IsRvR || player.CurrentRegion.IsInstance)
			{
				// Actual live message is: You can't use that item!
				player.Out.SendMessage("You can't use that here!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
				return false;
			}

			if (player.IsMoving)
			{
				player.Out.SendMessage("You must be standing still to use this item!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
				return false;
			}

			if (player.InCombat || GameRelic.IsPlayerCarryingRelic(player))
			{
				player.Out.SendMessage("You have been in combat recently and cannot use this item!", ChatType.CT_System, ChatLocation.CL_SystemWindow);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Always a constant casting time
		/// </summary>
		/// <returns></returns>
		public override int CalculateCastingTime()
		{
			return m_spell.CastTime;
		}


		/// <summary>
		/// Apply the effect.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			GamePlayer player = Caster as GamePlayer;
			if (player == null)
				return;

			if (player.InCombat || GameRelic.IsPlayerCarryingRelic(player) || player.IsMoving)
				return;

			SendEffectAnimation(player, 0, false, 1);

			UniPortalEffect effect = new UniPortalEffect(this, 1000);
			effect.Start(player);

			DOLCharacters character = player.DBCharacter;
			player.MoveTo((ushort)character.BindRegion,	character.BindXpos, character.BindYpos, character.BindZpos, (ushort)character.BindHeading);
		}


		public override void CasterMoves()
		{
			InterruptCasting();
			if (Caster is GamePlayer)
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client, "SpellHandler.CasterMove"), ChatType.CT_Important, ChatLocation.CL_SystemWindow);
		}


		public override void InterruptCasting()
		{
			m_startReuseTimer = false;
			base.InterruptCasting();
		}

		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();
				list.Add(string.Format("  {0}", Spell.Description));

				return list;
			}
		}
	}
}
