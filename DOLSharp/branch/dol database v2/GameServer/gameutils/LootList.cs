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
using DOL.Database2;
using DOL.GS;

namespace DOL.GS
{
	/// <summary>
	/// List containing all possible candidates for loot drop
	/// </summary>
	public class LootList
	{
		// number of random items to drop
		int m_dropCount;

		/// <summary>
		/// m_dropCount items will be chosen from randomitemdrops list depending on their chance, to drop for mob loot
		/// </summary>
		ArrayList m_randomItemDrops;

		/// <summary>
		/// Items in fixed drop list will ALWAYS drop for mob loot
		/// </summary>
		ArrayList m_fixedItemDrops;

		public LootList() : this(1) { }

		public LootList(int dropCount)
		{
			m_dropCount = dropCount;
			m_fixedItemDrops = new ArrayList(2);
			m_randomItemDrops = new ArrayList(15);
		}

		public int DropCount
		{
			get { return m_dropCount; }
			set { m_dropCount = value; }
		}

		/// <summary>
		/// Adds dropCount pieces of this item to the list of fixed drops.
		/// </summary>
		/// <param name="loot"></param>
		public void AddFixed(ItemTemplate loot, int dropCount)
		{
			for (int drop = 0; drop < dropCount; drop++)
				m_fixedItemDrops.Add(loot);
		}

		public void AddRandom(int chance, ItemTemplate loot)
		{
			LootEntry entry = new LootEntry(chance, loot);
			m_randomItemDrops.Add(entry);
		}

		/// <summary>
		/// Merges two list into one big list, containing items of both and having the bigger dropCount
		/// </summary>
		/// <param name="list"></param>
		public void AddAll(LootList list)
		{
			if (list.m_randomItemDrops != null)
			{
				m_randomItemDrops.AddRange(list.m_randomItemDrops);
			}

			if (list.m_fixedItemDrops != null)
			{
				m_fixedItemDrops.AddRange(list.m_fixedItemDrops);
			}

			if (list.m_dropCount > m_dropCount)
				m_dropCount = list.m_dropCount;
		}

		/// <summary>
		/// Returns a list of ItemTemplates chosen from Random and Fixed loot.
		/// </summary>
		/// <returns></returns>
		public ItemTemplate[] GetLoot()
		{
			ArrayList loot = new ArrayList(m_fixedItemDrops.Count + m_dropCount);
			loot.AddRange(m_fixedItemDrops);

            int dice;
            ArrayList lootCandidates = new ArrayList();

            // Drop count is the maximum number of items to drop, so we'll
            // do just as many rolls.

            for (int roll = 0; roll < m_dropCount; ++roll)
            {
                dice = Util.Random(1, 100);

                // For each roll make a list of items that have at least this
                // chance to drop.

                lootCandidates.Clear();
                foreach (LootEntry lootEntry in m_randomItemDrops)
                    if (lootEntry.Chance >= dice)
                        lootCandidates.Add(lootEntry.ItemTemplate);

                // Now pick one of these items at random.

                if (lootCandidates.Count > 0)
                    loot.Add(lootCandidates[Util.Random(lootCandidates.Count-1)]);
            }

			return (ItemTemplate[])loot.ToArray(typeof(ItemTemplate));
		}
	}

	/// <summary>
	/// Container class for entries in the randomdroplist
	/// </summary>
	class LootEntry
	{
		int m_chance;
		ItemTemplate m_itemTemplate;

		public LootEntry(int chance, ItemTemplate item)
		{
			m_chance = chance;
			m_itemTemplate = item;
		}

		public ItemTemplate ItemTemplate
		{
			get { return m_itemTemplate; }
		}

        public int Chance
        {
            get { return m_chance; }
        }
	}
}