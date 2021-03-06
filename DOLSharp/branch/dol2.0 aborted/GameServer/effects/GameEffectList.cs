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
using System.Reflection;
using DOL.AI.Brain;
using log4net;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Holds &amp; manages multiple effects on livings
	/// when iterating over this effect list lock the list!
	/// </summary>
	public class GameEffectList : IEnumerable
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Stores all effects
		/// </summary>
		protected ArrayList m_effects;

		/// <summary>
		/// The owner of this list
		/// </summary>
		protected readonly GameLiving m_owner;

		/// <summary>
		/// The current unique effect ID
		/// </summary>
		protected ushort m_runningID = 1;

		/// <summary>
		/// The count of started changes to the list
		/// </summary>
		protected sbyte m_changesCount;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="owner"></param>
		public GameEffectList(GameLiving owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			this.m_owner = owner;
		}

		/// <summary>
		/// add a new effect to the effectlist, it does not start the effect
		/// </summary>
		/// <param name="effect">The effect to add to the list</param>
		/// <returns>true if the effect was added</returns>
		public virtual bool Add(IGameEffect effect) 
		{
			if (!m_owner.Alive || m_owner.ObjectState!=eObjectState.Active)
					return false;	// dead owners don't get effects

			lock(this) 
			{
				if (m_effects == null)
					m_effects = new ArrayList(5);
				effect.InternalID = m_runningID++;
				if (m_runningID == 0)
					m_runningID = 1;
				m_effects.Add(effect);
			}

			OnEffectsChanged(effect);

			return true;
		}

		/// <summary>
		/// remove effect
		/// </summary>
		/// <param name="effect">The effect to remove from the list</param>
		/// <returns>true if the effect was removed</returns>
		public virtual bool Remove(IGameEffect effect) 
		{
			ArrayList changedEffects = new ArrayList();
			lock(this) 
			{
				if (m_effects == null)
					return false;
				int index = m_effects.IndexOf(effect);
				if (index < 0)
					return false;
				m_effects.RemoveAt(index);
				for (int i = index; i < m_effects.Count; i++)
				{
					changedEffects.Add(m_effects[i]);
				}
			}

			BeginChanges();
			for (int i = 0; i < changedEffects.Count; i++)
			{
				OnEffectsChanged((IGameEffect)changedEffects[i]);
			}
			CommitChanges();
			return true;
		}

		/// <summary>
		/// Cancels all effects
		/// </summary>
		public virtual void CancelAll()
		{
			IList fx = null;
			lock(this)
			{
				if (m_effects == null)
					return;
				fx = (ArrayList)m_effects.Clone();
				m_effects.Clear();
			}
			BeginChanges();
			foreach (IGameEffect effect in fx)
				effect.Cancel(false);
			CommitChanges();
		}

		/// <summary>
		/// Called when an effect changed
		/// </summary>
		public virtual void OnEffectsChanged(IGameEffect changedEffect) 
		{
			if (m_changesCount > 0)
				return;
			UpdateChangedEffects();
		}

		/// <summary>
		/// Begins multiple changes to the list that should not send updates
		/// </summary>
		public void BeginChanges()
		{
			lock (this)
			{
				m_changesCount++;
			}
		}

		/// <summary>
		/// Updates all list changes to the owner since BeginChanges was called
		/// </summary>
		public virtual void CommitChanges()
		{
			bool update;
			lock (this)
			{
				if (--m_changesCount < 0)
				{
					if (log.IsWarnEnabled)
						log.Warn("changes count is less than zero, forgot BeginChanges()?\n"+Environment.StackTrace);
					m_changesCount = 0;
				}
				update = m_changesCount == 0;
			}
			if (update)
				UpdateChangedEffects();
		}

		/// <summary>
		/// Updates the changed effects.
		/// </summary>
		protected virtual void UpdateChangedEffects()
		{
			if (m_owner is GameNPC)
			{
				IControlledBrain npc = ((GameNPC)m_owner).Brain as IControlledBrain;
				if (npc != null)
					npc.UpdatePetWindow();
			}
		}

		/// <summary>
		/// Find the first occurence of an effect with given type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>effect or null</returns>
		public virtual IGameEffect GetOfType(Type effectType)
		{
			lock(this) 
			{
				if (m_effects == null) return null;
				foreach (IGameEffect effect in m_effects) 
					if (effect.GetType().Equals(effectType)) return effect;
			}
			return null;			
		}

		/// <summary>
		/// Find effects of specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns>resulting effectlist</returns>
		public virtual IList GetAllOfType(Type effectType)
		{
			ArrayList list = new ArrayList();
			lock(this) 
			{
				if (m_effects == null) return list;
				foreach (IGameEffect effect in m_effects)
					if (effect.GetType().Equals(effectType)) list.Add(effect);
			}
			return list;
		}

		/// <summary>
		/// Count effects of a specific type
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns></returns>
		public int CountOfType(Type effectType) 
		{
			int count = 0;
			lock(this) 
			{
				if (m_effects == null) return count;
				foreach (IGameEffect effect in m_effects)
					if (effect.GetType().Equals(effectType)) count++;
			}
			return count;
		}

		/// <summary>
		/// Gets count of all stored effects
		/// </summary>
		public int Count
		{
			get { return m_effects == null ? 0 : m_effects.Count; }
		}

		#region IEnumerable Member	
		/// <summary>
		/// Returns an enumerator for the effects
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			if (m_effects == null)
				return new ArrayList(0).GetEnumerator();
			return m_effects.GetEnumerator();
		}
		#endregion
	}
}
