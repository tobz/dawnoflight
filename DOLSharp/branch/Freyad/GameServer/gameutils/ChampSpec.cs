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
using System.Collections;
using System.Collections.Generic;
using DOL.Database;
using DOL.Events;
namespace DOL.GS
{
    public class ChampSpecMgr
    {
        public static List<ChampSpec> m_championSpecs = new List<ChampSpec>();

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			LoadChampionSpecs();
		}

		/// <summary>
		/// Load or reload the champion specs from the DB
		/// </summary>
		public static void LoadChampionSpecs()
		{
			m_championSpecs.Clear();
			IList<DBChampSpecs> specs = GameServer.Database.SelectAllObjects<DBChampSpecs>();
			foreach (DBChampSpecs spec in specs)
			{
				ChampSpec newspec = new ChampSpec(spec.IdLine, spec.SkillIndex, spec.Index, spec.Cost, spec.SpellID);
				m_championSpecs.Add(newspec);
			}
		}

		public static ChampSpec GetAbilityFromIndex(int idline, int row, int index)
		{
			IList<ChampSpec> specs = ChampSpecMgr.GetAbilityForIndex(idline, row);
			foreach (ChampSpec spec in specs)
			{
				if (spec.IdLine == idline && spec.SkillIndex == row && spec.Index == index)
				{
					return spec;
				}
			}
			return null;
		}

		public static IList<ChampSpec> GetAbilityForIndex(int idline, int skillindex)
		{
			List<ChampSpec> list = new List<ChampSpec>();

			foreach (ChampSpec spec in m_championSpecs)
			{
				if (spec.IdLine == idline && spec.SkillIndex == skillindex)
				{
					list.Add(spec);
					list.Sort(new Sorter());
				}
			}
			return list;
		}


		public class Sorter : IComparer<ChampSpec>
        {
            //Lohx add - for sorting arraylist ascending
            public int Compare(ChampSpec x, ChampSpec y)
            {
                ChampSpec spec1 = (ChampSpec)x;
                ChampSpec spec2 = (ChampSpec)y;
                return spec1.Index.CompareTo(spec2.Index);
            }
        }


    }

    #region ChampSpec class
    public class ChampSpec 
    {
        public ChampSpec(int idline, int skillindex, int index, int cost, int spellid)
        {
            m_idline = idline;
            m_skillIndex = skillindex;
            m_index = index;
            m_cost = cost;
            m_spellid = spellid;
        }
        protected int m_idline;
        public int IdLine
        {
            get { return m_idline; }
            set { m_idline = value; }
        }
        protected int m_spellid;
        public int SpellID
        {
            get { return m_spellid; }
            set { m_spellid = value; }
        }
        protected int m_cost;
        public int Cost
        {
            get { return m_cost; }
            set { m_cost = value; }
        }
        protected int m_index;
        public int Index
        {
            get { return m_index; }
            set { m_index = value; }
        }
        protected int m_skillIndex;
        public int SkillIndex
        {
            get { return m_skillIndex; }
            set { m_skillIndex = value; }
        }

    }
    #endregion

}
