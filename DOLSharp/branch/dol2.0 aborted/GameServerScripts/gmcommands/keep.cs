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
/*using System;
using DOL.GS.PacketHandler;
using DOL.GS.Database;
using DOL.AI.Brain;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&keep", //command to handle
		(uint) ePrivLevel.GM, //minimum privelege level
		"Various keep creation commands!", //command description
		"'/keep fastcreate <type>' to create a keep with base template",
		"'/keep fastcreate ' to show all template available in fast create",
		"'/keep create <keepid> <name>' to create a keep ",
		"'/keep name <Name>' to change name",
		"'/keep keepid <keepID>' to assign keepid to keep",
		"'/keep level <level>' to change base level of keep",
		//"'/keep movehere' to move keep to player position",
		//"'/keep addcomponent <compx> <compy> <comphead> <skin> <height>' to add component to current keep",
		"'/keep save' to save keep into DB")]
	public class KeepCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		protected string TEMP_KEEP_LAST = "TEMP_KEEP_LAST";
		public enum eKeepTypes:int
		{
			DunCrauchonBledmeerFasteCaerBenowyc = 0,
			DunCrimthainnNottmoorFasteCaerBerkstead = 1,
			DunBolgHlidskialfFasteCaerErasleigh = 2,
			DunnGedGlenlockFasteCaerBoldiam = 3,
			DundaBehnnBlendrakeFasteCaerSursbrooke = 4,
			DunScathaigFensalirFasteCaerRenaris = 5,
			DunAilinneArvakrFasteCaerHurbury = 6,
			FortBrolorn=7,
			BG1_4= 8,
			ClaimBG5_9 = 9,
			BG5_9= 10,
			CaerClaret = 11,
			BG10_14= 12,
			CKBG15_19 = 13,
			BG15_19= 14,
			CKBG20_24 = 15,
			BG20_24= 16,
			CKBG25_29 = 17,
			BG25_29= 18,
			CKBG30_34 = 19,
			BG30_34= 20,
			CKBG35_39 = 21,
			BG35_39= 22,
			TBG35_39= 23,
			TestCKBG40_44 = 24,
			TestBG40_44= 25,
			TestTBG40_44= 26,
			CKBG40_44 = 27,
			BG40_44= 28,
			TBG40_44= 29,
		}

		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return 1;
			}
			AbstractGameKeep myKeep = (AbstractGameKeep)client.Player.TempProperties.getObjectProperty(TEMP_KEEP_LAST, null);
			if (myKeep == null) myKeep = KeepMgr.getKeepCloseToSpot((ushort)client.Player.RegionId, client.Player.Position, WorldMgr.OBJ_UPDATE_DISTANCE );
			switch (args[1])
			{
				case "fastcreate":
				{
					if (args.Length == 2)
					{
						client.Out.SendMessage("type of keep :",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						int i =0;
						foreach(string str in Enum.GetNames(typeof(eKeepTypes)))
						{
							client.Out.SendMessage("#" + i +" : "+ str,eChatType.CT_System,eChatLoc.CL_SystemWindow);
							i++;
						}
						return 1;
					}
					int keepType = 0;
					try
					{
						keepType = Convert.ToInt32(args[2]);
					}
					catch
					{
						DisplaySyntax(client);
						return 1;
					}
					GameKeep keep = new GameKeep();
					keep.Name = "blank";
					keep.KeepID = 0;
					keep.Level = 0;
					keep.Region = client.Player.Region;
					keep.Position = client.Player.Position;
					keep.Heading = client.Player.Heading;
					//todo add keep component to list in keep classB
					GameKeepComponent keepComp = null;
					switch ((eKeepTypes)keepType)
					{
						case eKeepTypes.DunCrauchonBledmeerFasteCaerBenowyc:
						{
							keepComp = new GameKeepComponent(0, 0, 254, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							keepComp = new GameKeepComponent(1, 2, 251, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 2, 1, 4, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 3, 8, 250, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 4, 8, 7, 251, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 5, 2, 8, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 6, 1, 249, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 7, 7, 8, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 8, 7, 249, 1, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 9, 9, 9, 2, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 10, 13, 248, 4, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 11, 2, 249, 7, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 12, 1, 8, 5, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 13, 8, 7, 8, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 14, 8, 250, 8, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 15, 2, 6, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 16, 1, 253, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 17, 9, 3, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 18, 9, 0, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 19, 10, 4, 7, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 20, 14, 2, 9, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.DunCrimthainnNottmoorFasteCaerBerkstead:
						{
							 
							keepComp = new GameKeepComponent( 0, 0, 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 1, 1, 4, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 2, 2, 251, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 3, 8, 7, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 4, 8, 250, 250, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 5, 7, 250, 253, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 6, 7, 7, 251, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 7, 1, 7, 254, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 8, 1, 6, 1, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 9, 1, 5, 4, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 10, 9, 249, 0, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 11, 9, 249, 3, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 12, 4, 7, 7, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 13, 9, 0, 8, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 14, 9, 3, 8, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 15, 10, 251, 6, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 16, 9, 253, 8, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 17, 5, 250, 7, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 18, 20, 250, 4, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 19, 13, 249, 6, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.DunBolgHlidskialfFasteCaerErasleigh:
						{
							 
							keepComp = new GameKeepComponent( 0, 0, 253, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 1, 4, 246, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 2, 3, 3, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 3, 9, 248, 253, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 4, 9, 248, 3, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 5, 7, 249, 0, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 6, 9, 248, 6, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 7, 9, 248, 9, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 8, 1, 250, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 9, 9, 248, 250, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 10, 9, 255, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 11, 13, 2, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 12, 3, 6, 8, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 13, 7, 6, 5, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 14, 7, 3, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 15, 2, 4, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 16, 2, 5, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 17, 2, 6, 2, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 18, 10, 250, 8, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 19, 4, 249, 13, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 20, 2, 5, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 21, 2, 252, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 22, 20, 249, 6, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.DunnGedGlenlockFasteCaerBoldiam:
						{
							 
							keepComp = new GameKeepComponent( 0, 3, 250, 246, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 1, 2, 5, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 2, 9, 9, 250, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 3, 6, 254, 247, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 4, 4, 251, 243, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 5, 0, 255, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 6, 4, 8, 246, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 7, 9, 248, 255, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 8, 9, 248, 2, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 9, 2, 249, 5, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 10, 4, 250, 9, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 11, 1, 253, 7, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 12, 1, 0, 8, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 13, 1, 3, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 14, 4, 7, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 15, 1, 8, 253, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 16, 1, 7, 0, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 17, 1, 6, 3, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 18, 1, 5, 6, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 19, 10, 250, 4, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 20, 1, 249, 249, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 21, 13, 248, 252, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 22, 20, 249, 2, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.DundaBehnnBlendrakeFasteCaerSursbrooke:
						{
							 
							keepComp = new GameKeepComponent( 0, 4, 11, 247, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 1, 9, 5, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 2, 0, 252, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 3, 9, 2, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 4, 9, 249, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 5, 4, 245, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 6, 9, 247, 253, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 7, 9, 247, 0, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 8, 9, 12, 251, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 9, 9, 12, 254, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 10, 4, 14, 4, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 11, 4, 248, 7, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 12, 9, 251, 5, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 13, 7, 254, 4, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 14, 2, 10, 5, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 15, 1, 1, 5, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 16, 7, 4, 5, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 17, 9, 8, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 18, 9, 12, 1, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 19, 9, 247, 3, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 20, 13, 7, 6, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 21, 10, 10, 252, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 22, 17, 6, 250, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.DunScathaigFensalirFasteCaerRenaris:
						{
							 
							keepComp = new GameKeepComponent( 0, 0, 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 1, 9, 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 2, 9, 4, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 3, 4, 247, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 4, 4, 7, 246, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 5, 9, 249, 252, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 6, 9, 249, 255, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 7, 9, 8, 250, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 8, 9, 8, 253, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 9, 7, 7, 0, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 10, 7, 250, 2, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 11, 13, 8, 3, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 12, 9, 8, 6, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 13, 9, 249, 5, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 14, 9, 249, 8, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 15, 4, 10, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 16, 4, 250, 12, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent( 17, 9, 6, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 9, 253, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 9, 0, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(20, 9, 3, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(21, 10, 3, 8, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(22, 18, 252, 6, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.DunAilinneArvakrFasteCaerHurbury:
						{
							 
							keepComp = new GameKeepComponent(0, 0, 254, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1, 6, 4, 247, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2, 6, 253, 247, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3, 4, 8, 243, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4, 9, 248, 249, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5, 3, 250, 246, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6, 3, 7, 246, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7, 4, 246, 246, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8, 9, 248, 252, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9, 7, 249, 255, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9, 248, 2, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 7, 6, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 9, 9, 247, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9, 9, 250, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 7, 8, 253, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9, 9, 0, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 7, 8, 3, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(17, 1, 8, 6, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 2, 249, 8, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 7, 253, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(20, 13, 3, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(21, 3, 7, 9, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(22, 9, 248, 5, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(23, 3, 250, 9, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(24, 9, 0, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(25, 10, 251, 6, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(26, 14, 249, 4, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.FortBrolorn:
						{
							 
							keepComp = new GameKeepComponent(0, 3, 5, 255, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1, 3, 251, 255, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2, 3, 250, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3, 3, 6, 3, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4, 8, 2, 10, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5, 9, 253, 9, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6, 9, 252, 254, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7, 9, 3, 7, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8, 9, 255, 254, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9, 6, 251, 6, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 6, 5, 6, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 2, 6, 0, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 1, 250, 2, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9, 2, 254, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 8, 254, 10, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 19, 1, 11, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG1_4:
						{
							 
							keepComp = new GameKeepComponent(0, 0, 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1, 1, 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2, 2, 4, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3, 4, 7, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4, 4, 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5, 9, 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6, 9, 8, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7, 7, 7, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8, 7, 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9, 9, 8, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9, 249, 1, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4, 10, 2, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4, 250, 5, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9, 6, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9, 253, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9, 3, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9, 0, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.ClaimBG5_9:
						{
							 
							keepComp = new GameKeepComponent(0, 5, 5, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1, 5, 251, 249, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2, 7, 251, 255, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3, 9, 250, 252, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4, 9, 6, 250, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5, 7, 5, 253, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6, 9, 6, 0, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7, 5, 5, 3, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8, 9, 4, 4, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9, 1, 252, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 2, 2, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 9, 1, 4, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 9, 254, 4, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 5, 251, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9, 250, 2, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 19, 255, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG5_9:
						{
							 
							keepComp = new GameKeepComponent(0, 0, 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1, 1, 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2, 2, 4, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3, 4, 7, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4, 4, 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5, 9, 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6, 9, 8, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7, 7, 7, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8, 7, 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9, 9, 8, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9, 249, 1, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4, 10, 2, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4, 250, 5, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9, 6, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9, 253, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9, 3, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9, 0, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.CaerClaret:
						{
							 
							keepComp = new GameKeepComponent(0, 0, 254, 252, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1, 4, 4, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2, 4, 250, 252, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3, 9, 252, 255, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4, 9, 252, 2, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5, 9, 5, 253, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6, 9, 5, 0, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7, 4, 253, 6, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8, 9, 0, 4, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9, 4, 7, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9, 3, 4, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG10_14:
						{
							 
							keepComp = new GameKeepComponent(0, 0, 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1, 1, 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2, 2, 4, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3, 4, 7, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4, 4, 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5, 9, 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6, 9, 8, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7, 7, 7, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8, 7, 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9, 9, 8, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9, 249, 1, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4, 10, 2, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4, 250, 5, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9, 6, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9, 253, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9, 3, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9, 0, 3, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.CKBG15_19:
						{
							 
							keepComp = new GameKeepComponent(0,  4 , 247, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  4 , 007, 247, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  4 , 250, 010, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 010, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  0 , 254, 251, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  2 , 004, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  1 , 251, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  1 , 253, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  2 , 006, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 000, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 003, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 1 , 007, 251, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 1 , 250, 006, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 2 , 250, 253, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 2 , 007, 004, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 0 , 004, 006, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 007, 254, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(17, 9 , 007, 001, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 9 , 250, 003, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 9 , 250, 000, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG15_19:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  1 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  2 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 007, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 008, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  7 , 007, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  7 , 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 008, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 249, 001, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4 , 010, 002, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4 , 250, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9 , 006, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9 , 253, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9 , 003, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 000, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.CKBG20_24:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 253, 251, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  9 , 003, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  9 , 250, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  9 , 006, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 246, 251, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  4 , 009, 248, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 248, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  9 , 010, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  9 , 010, 002, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 248, 001, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 248, 004, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 9 , 010, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 9 , 010, 005, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9 , 248, 007, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 4 , 249, 011, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 4 , 012, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 255, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(17, 9 , 005, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 9 , 008, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 9 , 252, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(20, 9 , 002, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(21, 10, 253, 005, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG20_24:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  1 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  2 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 007, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 008, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  7 , 007, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  7 , 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 008, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 249, 001, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4 , 010, 002, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4 , 250, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9 , 006, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9 , 253, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9 , 003, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 000, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.CKBG25_29:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  9 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  9 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  9 , 003, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  9 , 000, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 246, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 011, 003, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  4 , 247, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  4 , 007, 246, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  4 , 247, 009, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 4 , 013, 006, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 1 , 249, 252, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 1 , 248, 255, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 1 , 247, 002, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 2 , 008, 250, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 2 , 009, 253, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 2 , 010, 000, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(17, 2 , 009, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 2 , 253, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 1 , 006, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(20, 1 , 250, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(21,10 , 006, 253, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG25_29:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  1 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  2 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 007, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 008, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  7 , 007, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  7 , 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 008, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 249, 001, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4 , 010, 002, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4 , 250, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9 , 006, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9 , 253, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9 , 003, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 000, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.CKBG30_34:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 255, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  9 , 005, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  9 , 252, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  9 , 249, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 008, 247, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  4 , 245, 250, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 247, 253, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  9 , 247, 003, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  9 , 247, 000, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 009, 251, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 009, 254, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 1 , 008, 001, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 1 , 007, 004, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 2 , 248, 006, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 3 , 249, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 3 , 006, 007, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 7 , 252, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(17, 7 , 005, 007, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 9 , 255, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 9 , 002, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(20,10 , 250, 004, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG30_34:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  1 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  2 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 007, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 008, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  7 , 007, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  7 , 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 008, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 249, 001, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4 , 010, 002, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4 , 250, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9 , 006, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9 , 253, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9 , 003, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 000, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.CKBG35_39:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  9 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  9 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 007, 246, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 247, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 008, 250, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 249, 252, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  9 , 249, 255, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  9 , 008, 253, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 008, 003, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 7 , 250, 002, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 7 , 007, 000, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 9 , 249, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13,10 , 253, 253, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9 , 008, 006, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9 , 249, 008, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 4 , 010, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(17, 4 , 250, 012, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 9 , 253, 010, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 9 , 006, 010, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(20, 7 , 003, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(21, 7 , 000, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG35_39:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  1 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  2 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 007, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 008, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  7 , 007, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  7 , 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 008, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 249, 001, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4 , 010, 002, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4 , 250, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9 , 006, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9 , 253, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9 , 003, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 000, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.TBG35_39:
						{
							 
							keepComp = new GameKeepComponent(0,  11 , 253, 004, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.TestCKBG40_44:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 004, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  9 , 001, 246, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  9 , 251, 246, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  9 , 248, 246, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  7 , 254, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  4 , 244, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  5 , 010, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  9 , 246, 250, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  9 , 246, 253, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 011, 248, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 011, 254, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 9 , 011, 001, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 7 , 247, 000, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 7 , 010, 251, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 4 , 013, 004, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 6 , 009, 007, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 6 , 249, 007, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(17, 3 , 006, 008, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 3 , 252, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 9 , 255, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(20, 9 , 005, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(21, 7 , 002, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(22, 4 , 248, 007, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(23, 2 , 247, 003, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(24,10 , 254, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.TestBG40_44:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  1 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  2 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 007, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 008, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  7 , 007, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  7 , 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 008, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 249, 001, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4 , 010, 002, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4 , 250, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9 , 006, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9 , 253, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9 , 003, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 000, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.TestTBG40_44:
						{
							keep.AddKeepComponents(keepComp);
							keepComp = new GameKeepComponent(0,  11 , 253, 004, 0, 0, 100, keep.KeepID);
							 
						}break;
						case eKeepTypes.CKBG40_44:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 004, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  9 , 001, 246, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  9 , 251, 246, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  9 , 248, 246, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  7 , 254, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  4 , 244, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  5 , 010, 247, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  9 , 246, 250, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  9 , 246, 253, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 011, 248, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 011, 254, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 9 , 011, 001, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 7 , 247, 000, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 7 , 010, 251, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 4 , 013, 004, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 6 , 009, 007, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 6 , 249, 007, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(17, 3 , 006, 008, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(18, 3 , 252, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(19, 9 , 255, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(20, 9 , 005, 009, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(21, 7 , 002, 008, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(22, 4 , 248, 007, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(23, 2 , 247, 003, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(24,10 , 254, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.BG40_44:
						{
							 
							keepComp = new GameKeepComponent(0,  0 , 254, 249, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(1,  1 , 251, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(2,  2 , 004, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(3,  4 , 007, 245, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(4,  4 , 247, 248, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(5,  9 , 249, 251, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(6,  9 , 008, 249, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(7,  7 , 007, 252, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(8,  7 , 250, 254, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(9,  9 , 008, 255, 3, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(10, 9 , 249, 001, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(11, 4 , 010, 002, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(12, 4 , 250, 005, 1, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(13, 9 , 006, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(14, 9 , 253, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(15, 9 , 003, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
							 
							keepComp = new GameKeepComponent(16, 9 , 000, 003, 2, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						case eKeepTypes.TBG40_44:
						{
							 
							keepComp = new GameKeepComponent(0,  11 , 253, 004, 0, 0, 100, keep.KeepID);
							keep.AddKeepComponents(keepComp);
						}break;
						default:
							client.Out.SendMessage("Wrong type of keep",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return 1;
					}
					client.Player.TempProperties.setProperty(TEMP_KEEP_LAST, keep);	
					client.Out.SendMessage("You have created a keep",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				case "create":
				{
					if (args.Length < 4)
					{
						DisplaySyntax(client);
						return 1;
					}
					int keepid = 0;
					try
					{
						keepid = Convert.ToInt32(args[2]);
					}
					catch
					{
						DisplaySyntax(client);
						return 1;
					}
					GameKeep keep = new GameKeep();
					keep.Name = String.Join(" ",args,3,args.Length-3);
					keep.KeepID = keepid;
					keep.Level = 0;
					keep.Region = client.Player.Region;
					keep.Position = client.Player.Position;
					keep.Heading = client.Player.Heading;
					client.Player.TempProperties.setProperty(TEMP_KEEP_LAST, keep);
					client.Out.SendMessage("You have created a keep",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				case "name":
				{
					if (args.Length < 3)
					{
						DisplaySyntax(client);
						return 1;
					}
					if (myKeep == null)
					{
						client.Out.SendMessage("You must create a keep first",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 1;
					}
					myKeep.Name = String.Join(" ",args,2,args.Length-2);
					client.Out.SendMessage("You change the name of current keep to "+ myKeep.Name,eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
				case "keepid":
				{
					if (args.Length < 3)
					{
						DisplaySyntax(client);
						return 1;
					}
					if (myKeep == null)
					{
						client.Out.SendMessage("You must create a keep first",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 1;
					}
					int keepid = 0;
					try
					{
						keepid = Convert.ToInt32(args[2]);
					}
					catch
					{
						DisplaySyntax(client);
						return 1;
					}
					myKeep.KeepID = keepid;
					client.Out.SendMessage("You change the id of current keep to "+ keepid,eChatType.CT_System,eChatLoc.CL_SystemWindow);

				}break;
				case "level":
				{
					if (args.Length < 3)
					{
						DisplaySyntax(client);
						return 1;
					}
					if (myKeep == null)
					{
						client.Out.SendMessage("You must create a keep first",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 1;
					}
					int keepLevel = 0;
					try
					{
						keepLevel = Convert.ToInt32(args[2]);
					}
					catch
					{
						DisplaySyntax(client);
						return 1;
					}
					myKeep.Level = keepLevel;
					client.Out.SendMessage("You change the level of current keep to "+ keepLevel,eChatType.CT_System,eChatLoc.CL_SystemWindow);

				}break;
				case "movehere":
				{
					
				}break;
				case "addcomponent":
				{
					
				}break;
				case "save":
				{
					if (myKeep == null)
					{
						client.Out.SendMessage("You must create a keep first",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						return 1;
					}
					myKeep.SaveIntoDatabase();
					client.Out.SendMessage("Keep save in Database",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}break;
					
				default :
				{
					DisplaySyntax(client);
					return 1;
				}
			}
			return 1;
		}
	}
}*/