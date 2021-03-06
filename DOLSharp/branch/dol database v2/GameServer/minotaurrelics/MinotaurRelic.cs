using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;

using DOL.GS;
using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    public class MinotaurRelic : GameStaticItem
    {
        #region constructor
		public MinotaurRelic() : base() { m_saveInDB = true; }

		public MinotaurRelic(DBMinotaurRelic obj)
			: this()
		{
			LoadFromDatabase(obj);
		}
		#endregion

        #region Declarations
        DBMinotaurRelic m_dbRelic;
        Timer timer = null;
        public RegionTimer respawntimer = null;
        protected int m_spawny;
        protected int m_spawnx;
        protected int m_spawnz;
        protected int m_spawnheading;
        protected int m_spawnregion;
        protected int m_relicSpell;
        protected string m_relicTarget;
        protected double m_xp;
        protected GamePlayer m_owner;
        protected int m_effect;
        protected int m_relicID;
        public ArrayList Playerlist = new ArrayList();

        /// <summary>
        /// gets or sets the current Owner of this Relic
        /// </summary>
        public GamePlayer Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }

        public int RelicID
        {
            get { return m_relicID; }
            set { m_relicID = value; }
        }

        /// <summary>
        /// gets or sets the current XP of this Relic
        /// </summary>
        public double XP
        {
            get { return m_xp; }
            set { m_xp = value; }
        }

        /// <summary>
        /// Get the RelicType 
        /// </summary>
        public int RelicSpell
        {
            get { return m_relicSpell; }
            set { m_relicSpell = value; }
        }

        /// <summary>
        /// Get the RelicTarget
        /// </summary>
        public string RelicTarget
        {
            get { return m_relicTarget; }
            set { m_relicTarget = value; }
        }

        public int SpawnX
        {
            get { return m_spawnx; }
            set { m_spawnx = value; }
        }

        public int SpawnY
        {
            get { return m_spawny; }
            set { m_spawny = value; }
        }

        public int SpawnZ
        {
            get { return m_spawnz; }
            set { m_spawnz = value; }
        }

        public int SpawnHeading
        {
            get { return m_spawnheading; }
            set { m_spawnheading = value; }
        }

        public int SpawnRegion
        {
            get { return m_spawnregion; }
            set { m_spawnregion = value; }
        }

        public int Effect
        {
            get { return m_effect; }
            set { m_effect = value; }
        }
        #endregion

        #region database load/save
        /// <summary>
        /// Loads the GameRelic from Database
        /// </summary>
        /// <param name="obj">The DBRelic-object for this relic</param>
        public override void LoadFromDatabase(DatabaseObject obj)
        {
            InternalID = obj.ObjectId;
            m_dbRelic = obj as DBMinotaurRelic;
            RelicID = m_dbRelic.RelicID;

            Heading = (ushort)m_dbRelic.SpawnHeading;
            CurrentRegionID = (ushort)m_dbRelic.SpawnRegion;
            X = m_dbRelic.SpawnX;
            Y = m_dbRelic.SpawnY;
            Z = m_dbRelic.SpawnZ;

            SpawnHeading = m_dbRelic.SpawnHeading;
            SpawnRegion = m_dbRelic.SpawnRegion;
            Effect = m_dbRelic.Effect;
            SpawnX = m_dbRelic.SpawnX;
            SpawnY = m_dbRelic.SpawnY;
            SpawnZ = m_dbRelic.SpawnZ;

            RelicSpell = m_dbRelic.relicSpell;
            RelicTarget = m_dbRelic.relicTarget;

            Name = m_dbRelic.Name;
            Model = m_dbRelic.Model;

            XP = MinotaurRelicManager.MAX_RELIC_EXP;

            //set still empty fields
            Emblem = 0;
            Level = 99;
        }

        /// <summary>
        /// Saves the current MinotaurRelic to the database
        /// </summary>
        public override void SaveIntoDatabase()
        {
            m_dbRelic.SpawnHeading = Heading;
            m_dbRelic.SpawnRegion = CurrentRegionID;
            m_dbRelic.SpawnX = X;
            m_dbRelic.SpawnY = Y;
            m_dbRelic.SpawnZ = Z;

            m_dbRelic.Effect = Effect;

            m_dbRelic.Name = Name;
            m_dbRelic.Model = Model;
            m_dbRelic.relicSpell = RelicSpell;

            if (InternalID == null)
            {
                GameServer.Database.AddNewObject(m_dbRelic);
                InternalID = m_dbRelic.ObjectId;
            }
            else
                GameServer.Database.SaveObject(m_dbRelic);
        }
        #endregion

        #region Interact
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;

            if (!player.IsAlive)
            {
                player.Out.SendMessage("You cannot pickup " + GetName(0, false) + ". You are dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (this.Owner != null)
            {
                player.Out.SendMessage("This Relic is owned by someone else!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.MinotaurRelic != null)
            {
                player.Out.SendMessage("You already have a Relic!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.Group != null)
            {
                foreach (GamePlayer pl in player.Group.GetPlayersInTheGroup())
                {
                    if (pl.MinotaurRelic != null)
                    {
                        player.Out.SendMessage("Someone in your group already have a Relic!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                    }
                }
            }

            PlayerTakesRelic(player);
            return true;
        }
        #endregion

        #region Relic Actions
        /// <summary>
        /// Called when a Players picks up a Relic
        /// </summary>
        /// <param name="player"></param>
        protected virtual void PlayerTakesRelic(GamePlayer player)
        {
            if (player == null) return;

            RemoveFromWorld();
            SetHandlers(player, true);
            MinotaurRelicManager.StartPlayerRelicEffect(player, this);
            player.MinotaurRelic = this;
            Owner = player;

            player.Out.SendMinotaurRelicWindow(player, Effect, true);
            player.Out.SendMinotaurRelicBarUpdate(player, (int)XP);

            timer = new Timer(new TimerCallback(XPTimerCallBack), null, 1000, 0);

            if (player.MinotaurRelic != null)
            {
                foreach (GamePlayer pl in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                {
                    if (!Playerlist.Contains(pl) && pl != player)
                        Playerlist.Add(pl);
                    pl.Out.SendMinotaurRelicWindow(player, player.MinotaurRelic.Effect, true);
                }
            }
        }

        /// <summary>
        /// Is called whenever the CurrentCarrier is supposed to loose the relic.
        /// </summary>
        /// <param name="player">the player who loses the relic</param>
        /// <param name="stop">True when we should stop the XP timer</param>
        public virtual void PlayerLoosesRelic(GamePlayer player, bool stop)
        {
            player.Out.SendMinotaurRelicWindow(player, 0, false);
            Update(player);
            SetHandlers(player, false);
            player.MinotaurRelic = null;
            Owner = null;
            if (stop && timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            foreach (GamePlayer pl in Playerlist)
            {
                pl.Out.SendMinotaurRelicWindow(player, 0, false);
            }
            Playerlist.Clear();
            
            AddToWorld();
        }

        /// <summary>
        /// Called when the Timer is reached
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        protected void XPTimerCallBack(object state)
        {
            if (XP - MinotaurRelicManager.XP_LOSS_PER_TICK < 0)
                XP = 0;
            else
                XP -= MinotaurRelicManager.XP_LOSS_PER_TICK;

            if (Owner != null)
            {
                Update(Owner);
                Owner.Out.SendMinotaurRelicBarUpdate(Owner, (int)XP);
            }

            if (XP == 0)
            {
                RelicDispose();
                return;
            }

            if (timer != null)
                timer.Change(1000, Timeout.Infinite);
        }

        /// <summary>
        /// Called when the Relic has reached 0 XP and drops
        /// </summary>
        public virtual void RelicDispose()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            if (Owner != null)
                PlayerLoosesRelic(Owner, true);

            RemoveFromWorld();

            if (respawntimer != null)
            {
                respawntimer.Stop();
                respawntimer = null;
            }
            respawntimer = new RegionTimer(this, new RegionTimerCallback(RespawnTimerCallback), 
                Util.Random(MinotaurRelicManager.MIN_RESPAWN_TIMER, MinotaurRelicManager.MAX_RESPAWN_TIMER));
        }

        /// <summary>
        /// Called when the Respawntimer is reached
        /// </summary>
        /// <param name="respawnTimer"></param>
        /// <returns></returns>
        protected virtual int RespawnTimerCallback(RegionTimer respawnTimer)
        {
            if (respawntimer != null)
            {
                respawntimer.Stop();
                respawntimer = null;
            }

            if (ObjectState == eObjectState.Active) return 0;

            X = SpawnX;
            Y = SpawnY;
            Z = SpawnZ;
            Heading = (ushort)SpawnHeading;
            CurrentRegionID = (ushort)SpawnRegion;

            XP = MinotaurRelicManager.MAX_RELIC_EXP;

            AddToWorld();
            return 0;
        }

        public virtual void ManualRespawn()
        {
            if (respawntimer != null)
            {
                respawntimer.Stop();
                respawntimer = null;
            }

            if (ObjectState == eObjectState.Active) return;

            X = SpawnX;
            Y = SpawnY;
            Z = SpawnZ;
            Heading = (ushort)SpawnHeading;
            CurrentRegionID = (ushort)SpawnRegion;

            XP = MinotaurRelicManager.MAX_RELIC_EXP;

            AddToWorld();
        }

        /// <summary>
        /// Updates the Relic on the Warmap and such
        /// </summary>
        /// <param name="living"></param>
        protected virtual void Update(GameLiving living)
        {
            if (living == null) return;

            CurrentRegionID = living.CurrentRegionID;
            X = living.X;
            Y = living.Y;
            Z = living.Z;
            Heading = living.Heading;
            CheckPlayersinRange();
        }

        public virtual void CheckPlayersinRange()
        {
            ArrayList list = new ArrayList();

            foreach (GamePlayer player in Playerlist)
            {
                if (player != null)
                {
                    if (player.CurrentRegionID != this.CurrentRegionID || !WorldMgr.CheckDistance(this, player, WorldMgr.VISIBILITY_DISTANCE))
                        list.Add(player);
                }
            }

            if (list.Count > 0)
            {
                foreach (GamePlayer player in list)
                {
                    if (player != null && Playerlist.Contains(player) && Owner != null && player != Owner)
                    {
                        player.Out.SendMinotaurRelicWindow(Owner, 0, false);
                        Playerlist.Remove(player);
                    }
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Called to set the Events to the Carrier
        /// </summary>
        /// <param name="player"></param>
        /// <param name="start"></param>
        protected virtual void SetHandlers(GamePlayer player, bool start)
        {
            if (start)
            {
                GameEventMgr.AddHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerAbsence));
                GameEventMgr.AddHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(PlayerAbsence));
                GameEventMgr.AddHandler(player, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerAbsence));
                GameEventMgr.AddHandler(player, GamePlayerEvent.GainedRealmPoints, new DOLEventHandler(RealmPointGain));
                GameEventMgr.AddHandler(player, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerAbsence));
            }
            else
            {
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.Quit, new DOLEventHandler(PlayerAbsence));
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.Dying, new DOLEventHandler(PlayerAbsence));
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerAbsence));
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.GainedRealmPoints, new DOLEventHandler(RealmPointGain));
                GameEventMgr.RemoveHandler(player, GamePlayerEvent.RegionChanged, new DOLEventHandler(PlayerAbsence));
            }
        }

        protected void RealmPointGain(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer && args is GainedRealmPointsEventArgs)
            {
                GamePlayer player = sender as GamePlayer;
                GainedRealmPointsEventArgs arg = args as GainedRealmPointsEventArgs;

                if (player.MinotaurRelic == null || arg == null) return;

                if (player.MinotaurRelic.XP < MinotaurRelicManager.MAX_RELIC_EXP)
                    player.MinotaurRelic.XP += (int)arg.RealmPoints / 6;
            }
        }

        protected void PlayerAbsence(DOLEvent e, object sender, EventArgs args)
        {
            if (sender is GamePlayer)
                PlayerLoosesRelic((sender as GamePlayer), false);
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("name: ").Append(Name).Append("\n")
            .Append("RelicID: ").Append(RelicID).Append("\n");

            if (Owner != null)
                sb.Append("Owner: " + Owner.Name);
            else
                sb.Append("Owner: No Owner");

            return sb.ToString();
        }
    }
}