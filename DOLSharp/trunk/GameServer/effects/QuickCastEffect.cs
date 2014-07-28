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
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Spells;

namespace DawnOfLight.GameServer.Effects
{
	/// <summary>
	/// The helper class for quckcast ability
	/// </summary>
	public class QuickCastEffect : StaticEffect, IGameEffect
	{
		/// <summary>
		/// Start the quickcast on player
		/// </summary>
		public override void Start(GameLiving living)
		{
			base.Start(living);
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Client, "Effects.QuickCastEffect.YouActivatedQC"), ChatType.CT_System, ChatLocation.CL_SystemWindow);
			m_owner.TempProperties.removeProperty(SpellHandler.INTERRUPT_TIMEOUT_PROPERTY);
		}

		/// <summary>
		/// Called when effect must be canceled
		/// </summary>
		public override void Cancel(bool playerCancel)
		{
			base.Cancel(playerCancel);
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_owner as GamePlayer).Client, "Effects.QuickCastEffect.YourNextSpellNoQCed"), ChatType.CT_System, ChatLocation.CL_SystemWindow);
		}

		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name { get { return LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.QuickCastEffect.Name"); } }

		/// <summary>
		/// Remaining Time of the effect in milliseconds
		/// </summary>
		public override int RemainingTime { get { return 0; } }

		/// <summary>
		/// Icon to show on players, can be id
		/// </summary>
		public override ushort Icon { get { return 0x0190; } }

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo
		{
			get
			{
				var delveInfoList = new List<string>();
                delveInfoList.Add(LanguageMgr.GetTranslation(((GamePlayer)Owner).Client, "Effects.Quickcast.DelveInfo"));

                return delveInfoList;
			}
		}
	}
}
