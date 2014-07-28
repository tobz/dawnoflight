using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnOfLight.GameServer.Constants
{
    public enum eServerPackets : byte
    {
        InventoryUpdate = 0x02,
        HouseUserPermissions = 0x03,
        CharacterJump = 0x04,
        HousingPermissions = 0x05,
        HouseEnter = 0x08,
        HousingItem = 0x09,
        HouseExit = 0x0A,
        HouseTogglePoints = 0x0F,
        MovingObjectCreate = 0x12,
        EquipmentUpdate = 0x15,
        VariousUpdate = 0x16,
        MerchantWindow = 0x17,
        HouseDecorationRotate = 0x18,
        SpellEffectAnimation = 0x1B,
        ConsignmentMerchantMoney = 0x1E,
        MarketExplorerWindow = 0x1F,
        PositionAndObjectID = 0x20,
        DebugMode = 0x21,
        CryptKey = 0x22,
        SessionID = 0x28,
        PingReply = 0x29,
        LoginGranted = 0x2A,
        CharacterInitFinished = 0x2B,
        LoginDenied = 0x2C,
        GameOpenReply = 0x2D,
        UDPInitReply = 0x2F,
        MinotaurRelicMapRemove = 0x45,
        MinotaurRelicMapUpdate = 0x46,
        WarMapClaimedKeeps = 0x49,
        WarMapDetailUpdate = 0x4A,
        PlayerCreate172 = 0x4B,
        VisualEffect = 0x4C,
        ControlledHorse = 0x4E,
        MinotaurRelicRealm = 0x59,
        XFire = 0x5C,
        KeepComponentInteractResponse = 0x61,
        KeepClaim = 0x62,
        KeepComponentHookpointStore = 0x63,
        KeepComponentHookpointUpdate = 0x65,
        WarmapBonuses = 0x66,
        KeepComponentUpdate = 0x67,
        KeepInfo = 0x69,
        KeepRealmUpdate = 0x6A,
        KeepRemove = 0x6B,
        KeepComponentInfo = 0x6C,
        KeepComponentDetailUpdate = 0x6D,
        GroupMemberUpdate = 0x70,
        SpellCastAnimation = 0x72,
        InterruptSpellCast = 0x73,
        AttackMode = 0x74,
        ConcentrationList = 0x75,
        TrainerWindow = 0x7B,
        Time = 0x7E,
        UpdateIcons = 0x7F,
        Dialog = 0x81,
        QuestEntry = 0x83,
        FindGroupUpdate = 0x86,
        PetWindow = 0x88,
        PlayerRevive = 0x89,
        PlayerModelTypeChange = 0x8D,
        CharacterPointsUpdate = 0x91,
        Weather = 0x92,
        DoorState = 0x99,
        ClientRegions = 0x9E,
        ObjectUpdate = 0xA1,
        RemoveObject = 0xA2,
        Quit = 0xA4,
        PlayerPosition = 0xA9,
        CharacterStatusUpdate = 0xAD,
        PlayerDeath = 0xAE,
        Message = 0xAF,
        MaxSpeed = 0xB6,
        RegionChanged = 0xB7,
        PlayerHeading = 0xBA,
        CombatAnimation = 0xBC,
        Encumberance = 0xBD,
        BadNameCheckReply = 0xC3,
        DetailWindow = 0xC4,
        AddFriend = 0xC5,
        RemoveFriend = 0xC6,
        Riding = 0xC8,
        SoundEffect = 0xC9,
        DupNameCheckReply = 0xCC,
        HouseCreate = 0xD1,
        HouseChangeGarden = 0xD2,
        PlaySound = 0xD3,
        PlayerCreate = 0xD4,
        DisableSkills = 0xD6,
        DelveInfo = 0xD8,
        ObjectCreate = 0xD9,
        NPCCreate = 0xDA,
        ModelChange = 0xDB,
        ObjectGuildID = 0xDE,
        ChangeGroundTarget = 0xDF,
        ObjectDelete = 0xE1,
        EmblemDialogue = 0xE2,
        SiegeWeaponAnimation = 0xE3,
        TradeWindow = 0xEA,
        ObjectDataUpdate = 0xEE,
        RegionSound = 0xEF,
        CharacterCreateReply = 0xF0,
        TimerWindow = 0xF3,
        SiegeWeaponInterface = 0xF5,
        ChangeTarget = 0xF6,
        HelpWindow = 0xF7,
        EmoteAnimation = 0xF9,
        MoneyUpdate = 0xFA,
        StatsUpdate = 0xFB,
        CharacterOverview = 0xFD,
        Realm = 0xFE,
        MasterLevelWindow = 0x13,
    }

    public enum eClientPackets : byte
    {
        PlayerCancelsEffect = 0xF8,			// 0x50 ^ 168
        PlayerAttackRequest = 0x74,			// 0xDC ^ 168
        PlayerAppraiseItemRequest = 0xE0,	// 0x48 ^ 168
        PetWindow = 0x8A,
        ObjectInteractRequest = 0x7A,		// 0xD2 ^ 168
        InviteToGroup = 0x87,				// 0x2F ^ 168
        HouseEnterLeave = 0x0B,
        DoorRequest = 0x99,					// 0x31 ^ 168
        DisbandFromGroup = 0xA8,			// 0x37 ^ 168
        DialogResponse = 0x82,				// 0x2A ^ 168
        CheckLOSRequest = 0xD0,
        WorldInit = 0xD4,					// 0x7C ^ 168
        UseSpell = 0x7D,					// 0xD5 ^ 168
        UseSlot = 0x71,						// 0xD9 ^ 168
        UseSkill = 0xBB,					// 0x13 ^ 168
        RemoveConcentrationEffect = 0x76,	// 0xDE ^ 168
        PlayerRegionChangeRequest = 0x90,	// 0x38 ^ 168
        QuestRewardChosen = 0x40,
        PlayerTarget = 0xB0,				// 0x18 ^ 168
        PlayerSitRequest = 0xC7,			// 0x6F ^ 168
        PlayerInitRequest = 0xE8,			// 0x40 ^ 168
        PlayerGroundTarget = 0xEC,			// 0x44 ^ 168
        PlayerDismountRequest = 0xC8,		// 0x60 ^ 168
        PlayerHeadingUpdate = 0xBA,			// 0x12 ^ 168  also known as Short State
        PlayerPickupHouseItem = 0x0D,
        PlayerMoveItem = 0x75 ^ 168,
    }
}
