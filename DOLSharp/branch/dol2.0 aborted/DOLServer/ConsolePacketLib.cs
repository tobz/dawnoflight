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
using DOL.Database;
using DOL.GS;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using log4net;

namespace DOLGameServerConsole
{
	/// <summary>
	/// The packetlib for dummy console clients for /commands
	/// </summary>
	public class ConsolePacketLib : IPacketLib
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void SendMessage(string msg, eChatType type, eChatLoc loc)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(string.Format("({0}, {1}): {2}", type, loc, msg));
			}
		}

		public void SendCustomDialog(string msg, CustomDialogResponse callback)
		{
			if (msg == null)
				msg = "(null)";
			if (callback == null)
			{
				if (log.IsDebugEnabled)
					log.Debug(string.Format("(info dialog): {0}", msg));
			}
			else
			{
				if (log.IsDebugEnabled)
					log.Debug(string.Format("Accepting dialog: {0} {1}\n\"{2}\"", callback.Target, callback.Method, msg));
				callback(null, 1);
			}
		}

		public byte GetPacketCode(ePackets packetCode) { return 0; }
		public void SendTCP(GSTCPPacketOut packet) {}
		public void SendTCP(byte[] buf) {}
		public void SendTCPRaw(GSTCPPacketOut packet) {}
		public void SendUDP(GSUDPPacketOut packet) {}
		public void SendUDP(byte[] buf) {}
		public void SendUDPRaw(GSUDPPacketOut packet) {}
		public void SendVersionAndCryptKey() {}
		public void SendLoginDenied(eLoginError et) {}
		public void SendLoginGranted() {}
		public void SendSessionID() {}
		public void SendPingReply(ulong timestamp, ushort sequence) {}
		public void SendRealm(eRealm realm) {}
		public void SendCharacterOverview(eRealm realm) {}
		public void SendDupNameCheckReply(string name, bool nameExists) {}
		public void SendBadNameCheckReply(string name, bool bad) {}
		public void SendAttackMode(bool attackState) {}
		public void SendCharCreateReply(string name) {}
		public void SendCharStatsUpdate() {}
		public void SendCharResistsUpdate() {}
		public void SendRegions() {}
		public void SendGameOpenReply() {}
		public void SendPlayerPositionAndObjectID() {}
		public void SendPlayerJump(bool headingOnly) {}
		public void SendPlayerInitFinished() {}
		public void SendUDPInitReply() {}
		public void SendTime() {}
		public void SendPlayerCreate(GamePlayer playerToCreate) {}
		public void SendObjectGuildID(GameObject obj, Guild guild) {}
		public void SendPlayerQuit(bool totalOut) {}
		public void SendRemoveObject(GameObject obj, eRemoveType type) {}
		public void SendItemCreate(GameObject obj) {}
		public void SendDebugMode(bool on) {}
		public void SendModelChange(GameObject obj, ushort newModel) {}
		public void SendEmoteAnimation(GameObject obj, eEmote emote) {}
		public void SendNPCCreate(GameObject obj) {}
		public void SendNPCUpdate(GameObject obj) {}
		public void SendLivingEquipementUpdate(GameLiving living) {}
		public void SendRegionChanged() {}
		public void SendUpdatePoints() {}
		public void SendUpdateMoney() {}
		public void SendUpdateMaxSpeed() {}
		public void SendCombatAnimation(GameObject attacker, GameObject defender, ushort weaponID, ushort shieldID, int style, byte stance, byte result, byte targetHealthPercent) {}
		public void SendStatusUpdate() {}
		public void SendSpellCastAnimation(GameLiving spellCaster, ushort spellID, ushort castingTime) {}
		public void SendSpellEffectAnimation(GameLiving spellCaster, GameLiving spellTarget, ushort spellid, ushort boltTime, bool noSound, byte success) {}
		public void SendRiding(GameObject rider, GameObject steed, bool dismount) {}
		public void SendFindGroupWindowUpdate(GamePlayer[] list) {}
		public void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type, bool autoWarpText, string message) {}
		public void SendGroupWindowUpdate() {}
		public void SendGroupMemberUpdate(bool updateIcons, GamePlayer player) {}
		public void SendGroupMembersUpdate(bool updateIcons) {}
		public void SendInventorySlotsUpdate(byte preAction, ICollection slots) {}
		public void SendInventorySlotsUpdate(ICollection slots) {}
		public void SendDoorState(IDoor door) {}
		public void SendMerchantWindow(IGameMerchant merchant) {}
		public void SendTradeWindow() {}
		public void SendCloseTradeWindow() {}
		public void SendPlayerDied(GamePlayer killedPlayer, GameObject killer) {}
		public void SendPlayerRevive(GamePlayer revivedPlayer) {}
		public void SendUpdatePlayer() {}
		public void SendUpdatePlayerSkills() {}
		public void SendUpdateWeaponAndArmorStats() {}
		public void SendCustomTextWindow(string caption, IList text) {}
		public void SendEncumberance() {}
		public void SendAddFriends(string[] friendNames) {}
		public void SendRemoveFriends(string[] friendNames) {}
		public void SendTimerWindow(string title, int seconds) {}
		public void SendCloseTimerWindow() {}
		public void SendTrainerWindow() {}
		public void SendInterruptAnimation(GameLiving living) {}
		public void SendDisableSkill(Skill skill, int duration) {}
		public void SendUpdateIcons(IList changedEffects, ref int lastUpdateEffectsCount) {}
		public void SendLevelUpSound() {}
		public void SendRegionEnterSound(byte soundId) {}
		public void SendSoundEffect(ushort soundId, ushort zoneId, ushort x, ushort y, ushort z, ushort radius) {}
		public void SendDebugMessage(string format, params object[] parameters) {}
		public void SendDebugPopupMessage(string format, params object[] parameters) {}
		public void SendEmblemDialogue() {}
		public void SendWeather(uint x, uint width, ushort speed, ushort fogdiffusion, ushort intensity) {}
		public void SendPlayerModelTypeChange(GamePlayer player, byte modelType) {}
		public void SendObjectDelete(GameObject obj) {}
	    public void SendTaskUpdate() {}
		public void SendQuestListUpdate() {}
		public void SendQuestUpdate(AbstractQuest quest) {}
		public void SendConcentrationList() {}
		public void SendUpdateCraftingSkills() {}
		public void SendChangeTarget(GameObject newTarget) {}
		public void SendPetWindow(GameLiving pet, ePetWindowAction windowAction, eAggressionState aggroState, eWalkState walkState) {}
		/*public void SendKeepInfo(AbstractGameKeep keep) {}
		public void SendKeepComponentInfo(GameKeepComponent keepComponent) {}
		public void SendKeepComponentDetailUpdate(GameKeepComponent keepComponent) {}
		public void SendKeepComponentUpdate(AbstractGameKeep keep,bool levelup) {}
		public void SendKeepClaim(AbstractGameKeep keep) {}
		public void SendKeepComponentInteract(GameKeepComponent component) {}
		public void SendKeepComponentHookPoint(GameKeepComponent component, int selectedHookPointIndex){}
		public void SendKeepDoorUpdate(GameKeepDoor door) {}
		public void SendClearKeepComponentHookPoint(GameKeepComponent component,int selectedHookPointIndex){}
		public void SendHookPointStore(GameKeepHookPoint hookPoint){}*/
		public void SendPlaySound(eSoundType soundType, ushort soundID) {}
		public void SendNPCsQuestEffect(GameNPC npc, bool flag) {}
		/*public void SendSiegeWeaponAnimation(GameSiegeWeapon siegeWeapon){}
		public void SendSiegeWeaponCloseInterface(){}
		public void SendSiegeWeaponInterface(GameSiegeWeapon siegeWeapon){}
		public void SendHouse(House house) {}
		public void SendGarden(House house) {}
		public void SendRemoveGarden(House house) {}
		public void SendEnterHouse(House house) {}
		public void SendFurniture(House house) {}
		public void SendMovingObjectCreate(GameMovingObject obj) {}
		public void SendComponentUpdate(GameKeepComponent keepcomponent){}
		public void SendWarmapUpdate(IList list) {}
		public void SendWarmapBonuses() {}*/
		public void SendCheckLOS(GameObject Checker, GameObject Target, CheckLOSResponse callback) {}
		public void SendLivingDataUpdate(GameLiving living, bool updateStrings) {}
		public void SendPlayerTitles() {}
		public void SendPlayerTitleUpdate(GamePlayer player) {}
	}
}
