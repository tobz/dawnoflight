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
using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Loot;
using DOL.GS.PacketHandler;
using DOL.GS.Utils;
using NHibernate.Expression;

namespace DOL.GS.Scripts
{
	[Cmd("&mob", //command to handle
		(uint) ePrivLevel.GM, //minimum privelege level
		"Various mob creation commands!", //command description
		//Usage
		"'/mob nfastcreate <Model> <Realm> <Level> <Number> <Radius> <Name>' to create n mob with fixed level,model and name placed around creator within the provided radius",
		"'/mob nrandcreate <Number> <Radius>' to create <Number> mob in <Radius> around player",
		"'/mob fastcreate <Model> <Level> <Name>' to create mob with fixed level,model",
		"'/mob create' to create an empty mob",
		"'/mob model <newMobModel>' to set the mob model to newMobModel",
		"'/mob size <newMobSize>' to set the mob size to newMobSize",
		"'/mob name <newMobName>' to set the mob name to newMobName",
		"'/mob guild <newMobGuild>' to set the mob guildname to newMobGuild",
		"'/mob aggroLevel <level>' set mob aggro level 0..100%",
		"'/mob aggroRange <distance>' set mob aggro range",
		"'/mob damagetype <eDamageType>' set mob damage type",
		"'/mob movehere'",
		"'/mob remove' to remove this mob from the DB",
		"'/mob save' to save this mob in the DB",
		"'/mob ghost' makes this mob ghostlike",
		"'/mob stealth' makes this mob stealth",
		"'/mob fly' makes this mob able to fly by changing the Z coordinate",
		"'/mob noname' still possible to target this mob but removes the name from above mob",
		"'/mob notarget' makes it impossible to target this mob and removes the name from above it",
		"'/mob kill' Kills the mob without removing it from the DB",
		"'/mob regen' regenerates the mob health to maximum ",
		"'/mob info' amplified informations output about the mob",
		"'/mob realm' changing the mob's realm",
		"'/mob speed' changing the mob's max speed",
		"'/mob level' changing the mob's level",
		"'/mob brain' changing the mob's brain",
		"'/mob respawn <newDuration>' changing the time between each mob respawn",
		"'/mob equipinfo' to show mob inventory infos",
		//"'/mob equiptemplate create' to create an empty inventory template",
		//"'/mob equiptemplate add <slot> <model> [color] [effect]' to add an item to this mob inventory template",
		//"'/mob equiptemplate remove <slot>' to remove item from the specified slot in this mob inventory template",
		//"'/mob equiptemplate clear' to remove the inventory template from mob",
		//"'/mob equiptemplate close' to finish the inventory template you are creating",
		//"'/mob equiptemplate save <templateID> [replace]' to save the inventory template with a new name",
		"'/mob equiptemplate load <templateID>' to assign a inventory template to this mob",
		"'/mob lootlist <lootListID>' to assign a loot list to the mob",
		"'/mob viewloot' to view the selected mob's loot table",
		"'/mob removeloot <ItemTemplateID>' to remove loot from the mob's unique drop table",
		"'/mob copy' copies a mob exactly and places it at your location"
		)]
	public class MobCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return 1;
			}
			string param = "";
			if (args.Length > 2)
				param = String.Join(" ", args, 2, args.Length - 2);

			GameMob targetMob = null;
			if (client.Player.TargetObject != null && client.Player.TargetObject is GameMob)
				targetMob = (GameMob) client.Player.TargetObject;

			if (args[1] != "create" 
				&& args[1] != "fastcreate" 
				&& args[1] != "nrandcreate" 
				&& args[1] != "nfastcreate" 
				&& targetMob == null)
			{
				if (client.Player.TargetObject != null) {
					client.Out.SendMessage("Cannot use "+client.Player.TargetObject+" for /mob command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);				
					return 1;
				}
				client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			switch (args[1])
			{
				case "nrandcreate":
					{
						ushort number = 1;
						ushort radius = 10;
						if (args.Length < 4)
						{
							client.Out.SendMessage("Usage: /mob nrandcreate <Number> <Radius>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						try
						{
							radius = Convert.ToUInt16(args[3]);
							number = Convert.ToUInt16(args[2]);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Usage: /mob  nrandcreate <Number> <Radius>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						for (int i = 0; i < number; ++i)
						{
							GameMob mob = new GameMob();
							//Fill the object variables
							Point pos = client.Player.Position;
							pos.X = FastMath.Abs(pos.X + Util.Random(-radius, radius));
							pos.Y = FastMath.Abs(pos.Y + Util.Random(-radius, radius));
							mob.Position = pos;
							mob.Region = client.Player.Region;
							mob.Heading = client.Player.Heading;
							mob.Level = (byte) Util.Random(10, 50);
							mob.Realm = (byte) Util.Random(1, 3);
							mob.Name = "rand_" + i;
							mob.Model = (byte) Util.Random(100, 200);
							mob.CurrentSpeed = 0;
							mob.MaxSpeedBase = 200;
							mob.GuildName = "";
							mob.Size = 50;

							mob.OwnBrain = new StandardMobBrain();
							mob.OwnBrain.Body = mob;
							((IAggressiveBrain)mob.Brain).AggroLevel = 100;
							((IAggressiveBrain)mob.Brain).AggroRange = 1500;
							
							mob.AddToWorld();
						}
						client.Out.SendMessage("Created " + number + " mobs !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				case "nfastcreate":
					{
						byte level = 1;
						ushort model = 408;
						byte realm = 0;
						ushort number = 1;
						string name = "New Mob";
						ushort radius = 10;
						if (args.Length < 8)
						{
							client.Out.SendMessage("Usage: /mob nfastcreate <Model> <Realm> <Level> <Number> <Radius> <Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						try
						{
							name = String.Join(" ", args, 7, args.Length - 7);
							radius = Convert.ToUInt16(args[6]);
							number = Convert.ToUInt16(args[5]);
							level = Convert.ToByte(args[4]);
							realm = Convert.ToByte(args[3]);
							model = Convert.ToUInt16(args[2]);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Usage: /mob nfastcreate <Model> <Level> <Number> <Radius> <Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						for (int i = 0; i < number; ++i)
						{
							GameMob mob = new GameMob();

							//Fill the object variables
							Point pos = client.Player.Position;
							pos.X = FastMath.Abs(pos.X + Util.Random(-radius, radius));
							pos.Y = FastMath.Abs(pos.Y + Util.Random(-radius, radius));
							mob.Position = pos;
							mob.Region = client.Player.Region;
							mob.Heading = client.Player.Heading;
							mob.Level = level;
							mob.Realm = realm;
							mob.Name = name;
							mob.Model = model;
							mob.CurrentSpeed = 0;
							mob.MaxSpeedBase = 200;
							mob.GuildName = "";
							mob.Size = 50;

							mob.OwnBrain = new StandardMobBrain();
							mob.OwnBrain.Body = mob;
							((IAggressiveBrain)mob.Brain).AggroLevel = 100;
							((IAggressiveBrain)mob.Brain).AggroRange = 1500;
							
							
							mob.AddToWorld();
							client.Out.SendMessage("Mob created: OID =" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						break;
					}
				case "fastcreate":
					{
						byte level = 1;
						ushort model = 408;
						string name = "New Mob";

						if (args.Length < 5)
						{
							client.Out.SendMessage("Usage: /mob fastcreate <Model> <Level> <Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						try
						{
							name = String.Join(" ", args, 4, args.Length - 4);
							level = Convert.ToByte(args[3]);
							model = Convert.ToUInt16(args[2]);
						}
						catch
						{
							client.Out.SendMessage("Usage: /mob fastcreate <Model> <Level> <Name>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						name = CheckName(name, client);
						GameMob mob = new GameMob();
						//Fill the object variables
						mob.Position = client.Player.Position;
						mob.Region = client.Player.Region;
						mob.Heading = client.Player.Heading;
						mob.Level = level;
						mob.Realm = 0;
						mob.Name = name;
						mob.Model = model;
						mob.CurrentSpeed = 0;
						mob.MaxSpeedBase = 200;
						mob.GuildName = "";
						mob.Size = 50;

						mob.OwnBrain = new StandardMobBrain();
						mob.OwnBrain.Body = mob;
						((IAggressiveBrain)mob.Brain).AggroLevel = 100;
						((IAggressiveBrain)mob.Brain).AggroRange = 1500;
							
						mob.AddToWorld();
						client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "info":
					{
						ArrayList info = new ArrayList();
						info.Add(" + Class: " + targetMob.GetType().ToString());
						info.Add(" + Realm: " + targetMob.Realm);
						info.Add(" + Level: " + targetMob.Level);
						info.Add(" + Brain: " + (targetMob.Brain == null ? "(null)" : targetMob.Brain.GetType().ToString()));
						IAggressiveBrain aggroBrain = targetMob.Brain as IAggressiveBrain;
						if (aggroBrain != null)
						{
							info.Add(" + Aggro level: " + aggroBrain.AggroLevel);
							info.Add(" + Aggro range: " + aggroBrain.AggroRange);
						}
						else
						{
							info.Add(" + Not aggressive brain");
						}
						info.Add(" + Damage type: " + targetMob.MeleeDamageType);
						info.Add(" + Position: " + targetMob.Position);
						info.Add(" + Guild: " + targetMob.GuildName);
						info.Add(" + Model: " + targetMob.Model + " sized to " + targetMob.Size);
						info.Add(string.Format(" + Flags: {0} (0x{1})", ((GameNPC.eFlags) targetMob.Flags).ToString("G"), targetMob.Flags.ToString("X")));
						info.Add(" + Active weapon slot: " + targetMob.ActiveWeaponSlot);
						info.Add(" + Visible weapon slot: " + targetMob.VisibleActiveWeaponSlots);
						info.Add(" + MaxSpeed : " + targetMob.MaxSpeedBase);
						if(targetMob.Inventory != null)
						{
							info.Add(" + Inventory Template ID: " + targetMob.Inventory.InventoryID);
						}
						else
						{
							info.Add(" + Inventory Template ID: null");
						}
						info.Add(" + Inventory: " + targetMob.Inventory);

						client.Out.SendCustomTextWindow("[ " + targetMob.Name + " ]", info);
					}
					break;
				case "level":
					{
						byte level;
						try
						{
							level = Convert.ToByte(args[2]);
							targetMob.Level = level;
							client.Out.SendMessage("Mob level changed to: " + targetMob.Level, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
				case "realm":
					{
						byte realm;
						try
						{
							realm = Convert.ToByte(args[2]);
							targetMob.Realm = realm;
							client.Out.SendMessage("Mob realm changed to: " + targetMob.Realm, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
				case "speed":
					{
						int maxSpeed;
						try
						{
							maxSpeed = Convert.ToUInt16(args[2]);
							targetMob.MaxSpeedBase = maxSpeed;
							client.Out.SendMessage("Mob MaxSpeed changed to: " + targetMob.MaxSpeedBase, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
				case "regen":
					{
						try
						{
							targetMob.ChangeHealth(null, targetMob.MaxHealth, false);
							client.Out.SendMessage("Mob health regenerated (" + targetMob.Health + "/" + targetMob.MaxHealth + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;
				case "kill":
					{
						try
						{
							targetMob.Die(client.Player);
							client.Out.SendMessage("Mob '" + targetMob.Name + "' killed", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
				case "create":
					{
						string theType = "DOL.GS.GameMob";
						if (args.Length > 2)
							theType = args[2];

						//Create a new mob
						GameMob mob = null;
						try
						{
							client.Out.SendDebugMessage(Assembly.GetAssembly(typeof (GameServer)).FullName);
							mob = (GameMob) Assembly.GetAssembly(typeof (GameServer)).CreateInstance(theType, false);
						}
						catch (Exception e)
						{
							client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}
						if (mob == null)
						{
							try
							{
								client.Out.SendDebugMessage(Assembly.GetExecutingAssembly().FullName);
								mob = (GameMob) Assembly.GetExecutingAssembly().CreateInstance(theType, false);
							}
							catch (Exception e)
							{
								client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
						}
						if (mob == null)
						{
							client.Out.SendMessage("There was an error creating an instance of " + theType + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 0;
						}
						//Fill the object variables
						mob.Position = client.Player.Position;
						mob.Region = client.Player.Region;
						mob.Heading = client.Player.Heading;
						mob.Level = 1;
						mob.Realm = 0;
						mob.Name = "New Mob";
						mob.Model = 408;
						//Fill the living variables
						mob.CurrentSpeed = 0;
						mob.MaxSpeedBase = 200;
						mob.GuildName = "";
						mob.Size = 50;

						PeaceBrain brain = new PeaceBrain();
						brain.Body = mob;
						mob.OwnBrain = brain;

						mob.AddToWorld();
						client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "model":
					{
						ushort model;
						try
						{
							model = Convert.ToUInt16(args[2]);
							targetMob.Model = model;
							client.Out.SendMessage("Mob model changed to: " + targetMob.Model, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				case "respawn":
				{
					int interval;
					try
					{
						interval = Convert.ToInt16(args[2]);
						targetMob.RespawnInterval = interval;
						client.Out.SendMessage("Mob respawn interval changed to: " + interval, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					catch (Exception)
					{
						client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
				}
					break;

				case "size":
					{
						ushort size;
						try
						{
							size = Convert.ToUInt16(args[2]);
							if (targetMob == null || size > 255)
							{
								client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 1;
							}

							targetMob.Size = (byte) size;
							client.Out.SendMessage("Mob size changed to: " + targetMob.Size, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				case "aggroLevel":
					{
						try
						{
							IAggressiveBrain aggroBrain = targetMob.Brain as IAggressiveBrain;
							if (aggroBrain != null)
							{
								int aggro = int.Parse(args[2]);
								client.Out.SendMessage("Mob aggro changed to: " + aggro + " (was " + aggroBrain.AggroLevel + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								aggroBrain.AggroLevel = aggro;
							}
							else
							{
								DisplayMessage(client, "Not aggressive brain.");
							}
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				case "aggroRange":
					{
						try
						{
							IAggressiveBrain aggroBrain = targetMob.Brain as IAggressiveBrain;
							if (aggroBrain != null)
							{
								int range = int.Parse(args[2]);
								DisplayMessage(client, "Mob aggro range changed to: {0} (was {1})", range, aggroBrain.AggroRange);
								aggroBrain.AggroRange = range;
							}
							else
							{
								DisplayMessage(client, "Not aggressive brain.");
							}
						}
						catch
						{
							DisplayError(client, "Type /mob for command overview.");
							return 1;
						}
					}
					break;

				case "damagetype":
					{
						try
						{
							eDamageType damage = (eDamageType) Enum.Parse(typeof (eDamageType), args[2], true);
							DisplayMessage(client, "Mob damage type changed to: {0} (was {1})", damage, targetMob.MeleeDamageType);
							targetMob.MeleeDamageType = damage;
						}
						catch
						{
							DisplayError(client, "Type /mob for command overview.");
						}
					}
					break;

				case "name":
					{
						if (param != "" && targetMob != null)
						{
							targetMob.Name = CheckName(param, client);
							client.Out.SendMessage("Mob name changed to: " + targetMob.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
					break;

				case "brain":
					{
						try
						{
							ABrain brain = null;
							string brainType = args[2];
							try
							{
								client.Out.SendDebugMessage(Assembly.GetAssembly(typeof (GameServer)).FullName);
								brain = (ABrain) Assembly.GetAssembly(typeof (GameServer)).CreateInstance(brainType, false);
							}
							catch (Exception e)
							{
								client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							}
							if (brain == null)
							{
								try
								{
									client.Out.SendDebugMessage(Assembly.GetExecutingAssembly().FullName);
									brain = (ABrain) Assembly.GetExecutingAssembly().CreateInstance(brainType, false);
								}
								catch (Exception e)
								{
									client.Out.SendMessage(e.ToString(), eChatType.CT_System, eChatLoc.CL_PopupWindow);
								}
							}
							if (brain == null)
							{
								client.Out.SendMessage("There was an error creating an instance of " + brainType + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
							targetMob.SetOwnBrain(brain);
						}
						catch
						{
							DisplayError(client, "Type /mob for command overview.");
							return 1;
						}
					}
					break;

				case "guild":
					{
						if (param != "")
						{
							targetMob.GuildName = CheckGuildName(param, client);
							client.Out.SendMessage("Mob guild changed to: " + targetMob.GuildName, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
					break;

				case "movehere":
					{
						targetMob.MoveTo(client.Player.Region, client.Player.Position, (ushort)client.Player.Heading);
						client.Out.SendMessage("Target Mob '" + targetMob.Name + "' moved to your location!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;

				case "attack":
					foreach (GamePlayer player in targetMob.GetInRadius(typeof(GamePlayer), 3000))
					{
						if (player.Name == args[2])
						{
							targetMob.StartAttack(player);
							break;
						}
					}
					break;

				case "remove":
					{
						targetMob.RemoveFromWorld();
						if(targetMob.PersistantGameObjectID != 0) GameServer.Database.DeleteObject(targetMob);
						client.Out.SendMessage("Target Mob removed from DB!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;

				case "save":
				{
					if(targetMob.PersistantGameObjectID != 0) GameServer.Database.SaveObject(targetMob);
					else GameServer.Database.AddNewObject(targetMob);
					client.Out.SendMessage("Target Mob saved in DB!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
					break;

				case "equipinfo":
					{
						if (targetMob.Inventory == null)
						{
							client.Out.SendMessage("Mob inventory not found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

						client.Out.SendMessage("--------------------------------------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						string message = string.Format("			         Inventory: {0}, equip template ID: {1}", targetMob.Inventory.GetType().ToString(), targetMob.Inventory.InventoryID);
						client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("--------------------------------------------------------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						client.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						foreach (NPCEquipment item in targetMob.Inventory.AllItems)
						{
							client.Out.SendMessage("Slot Description : [" + GlobalConstants.SlotToName(item.SlotPosition) + "]", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("         Slot: " + item.SlotPosition, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("        Model: " + item.Model, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("        Color: " + item.Color, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							if(item is NPCArmor)
								client.Out.SendMessage("ModelExtension: " + ((NPCArmor)item).ModelExtension, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							else if(item is NPCWeapon)
								client.Out.SendMessage("    GlowEffect: " + ((NPCWeapon)item).GlowEffect, eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("------------", eChatType.CT_System, eChatLoc.CL_PopupWindow);
							client.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						}
					}
					break;

				case "equiptemplate":
					{
						if (args.Length < 3)
						{
							client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

					/*	if (args[2].ToLower() == "create")
						{
							try
							{
								if (targetMob.Inventory != null)
								{
									client.Out.SendMessage("Target mob inventory is set to " + targetMob.Inventory.GetType() + ", remove it first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 1;
								}

								targetMob.Inventory = new GameNpcInventoryTemplate();

								client.Out.SendMessage("Inventory template created.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							catch
							{
								client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 1;
							}
							break;
						}
						else */if (args[2].ToLower() == "load")
						{
							if (args.Length > 3)
							{
								try
								{
									GameNpcInventory inv = NPCInventoryMgr.GetNPCInventory(int.Parse(args[3]));
									if(inv == null)
									{
										client.Out.SendMessage("Mob inventory template with id '"+int.Parse(args[3])+"' not found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
									targetMob.Inventory = inv;
									targetMob.UpdateNPCEquipmentAppearance();
									GameServer.Database.SaveObject(targetMob);
									client.Out.SendMessage("Mob equipment loaded!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								catch
								{
									client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 1;
								}
							}
							else
							{
								client.Out.SendMessage("Usage: /mob equiptemplate load <templateID>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							break;
						}

						/*GameNpcInventoryTemplate template = targetMob.Inventory as GameNpcInventoryTemplate;
						if (template == null)
						{
							client.Out.SendMessage("Target mob is not using GameNpcInventoryTemplate.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}

						switch (args[2])
						{
							case "add":
								{
									if (args.Length >= 5)
									{
										try
										{
											int slot = GlobalConstants.NameToSlot(args[3]);
											if (slot == 0)
											{
												client.Out.SendMessage("No such slot available!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												return 1;
											}

											int model = Convert.ToInt32(args[4]);
											int color = 0;
											int effect = 0;

											if (args.Length >= 6)
												color = Convert.ToInt32(args[5]);
											if (args.Length >= 7)
												effect = Convert.ToInt32(args[6]);


											if (! template.AddNPCEquipment((eInventorySlot) slot, model, color, effect))
											{
												client.Out.SendMessage("Couldn't add new item to slot " + slot + ". Template could be closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												return 1;
											}

											client.Out.SendMessage("Item added to the mob inventory template!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										}
										catch
										{
											client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 1;
										}
									}
									else
									{
										client.Out.SendMessage("Usage: /mob equiptemplate add <slot> <model> [color] [effect]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
								}
								break;

							case "remove":
								{
									if (args.Length > 3)
									{
										try
										{
											int slot = GlobalConstants.NameToSlot(args[3]);
											if (slot == 0)
											{
												client.Out.SendMessage("No such slot available!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												return 1;
											}

											if (!template.RemoveNPCEquipment((eInventorySlot) slot))
											{
												client.Out.SendMessage("Couldn't remove item from slot " + slot + ". Template could be closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												return 1;
											}
											client.Out.SendMessage("Mob inventory template slot " + slot + " cleaned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										}
										catch
										{
											client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 1;
										}
									}
									else
									{
										client.Out.SendMessage("Usage: /mob equiptemplate remove <slot>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
								}
								break;

							case "close":
								{
									if (template.IsClosed)
									{
										client.Out.SendMessage("Template is already closed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
									targetMob.Inventory = template.CloseTemplate();
									client.Out.SendMessage("Inventory template closed succesfully.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								break;

							case "clear":
								{
									targetMob.Inventory = null;
									targetMob.EquipmentTemplateID = null;
									targetMob.SaveIntoDatabase();
									client.Out.SendMessage("Mob equipment removed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								break;

							case "save":
								{
									if (args.Length > 3)
									{
										bool replace = (args.Length > 4 && args[4].ToLower() == "replace") ? true : false;
										if (!replace && null != GameServer.Database.SelectObject(typeof(NPCEquipment), Expression.Eq("TemplateID",args[3])))
										{
											client.Out.SendMessage("Template with name '" + args[3] + "' already exists. Use 'replace' flag if you want to overwrite it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 1;
										}
										if (!targetMob.Inventory.SaveIntoDatabase(args[3]))
										{
											client.Out.SendMessage("Error saving template with name " + args[3], eChatType.CT_System, eChatLoc.CL_SystemWindow);
											return 1;
										}
										targetMob.EquipmentTemplateID = args[3];
										targetMob.SaveIntoDatabase();
										client.Out.SendMessage("Target mob equipment template is saved as '" + args[3] + "'", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										return 1;
									}
									else
									{
										client.Out.SendMessage("Usage: /mob equiptemplate save <templateID> [replace]", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									}
								}
								break;

							default:
								client.Out.SendMessage("Type /mob for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								break;
						}

						targetMob.UpdateNPCEquipmentAppearance();*/
					}
					break;

				case "ghost":
					{
						targetMob.Flags ^= (byte) GameNPC.eFlags.GHOST;
						client.Out.SendMessage("Mob GHOST flag is set to " + ((targetMob.Flags & (uint) GameNPC.eFlags.GHOST) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "stealth":
				{
					targetMob.Flags ^= (byte) GameNPC.eFlags.STEALTH;
					client.Out.SendMessage("Mob TRANSPARENT flag is set to " + ((targetMob.Flags & (uint) GameNPC.eFlags.STEALTH) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
					break;
				case "fly":
					{
						targetMob.Flags ^= (byte) GameNPC.eFlags.FLYING;
						client.Out.SendMessage("Mob FLYING flag is set to " + ((targetMob.Flags & (uint) GameNPC.eFlags.FLYING) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "noname":
					{
						targetMob.Flags ^= (byte) GameNPC.eFlags.DONTSHOWNAME;
						client.Out.SendMessage("Mob DONTSHOWNAME flag is set to " + ((targetMob.Flags & (uint) GameNPC.eFlags.DONTSHOWNAME) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "notarget":
					{
						targetMob.Flags ^= (byte) GameNPC.eFlags.CANTTARGET;
						client.Out.SendMessage("Mob CANTTARGET flag is set to " + ((targetMob.Flags & (uint) GameNPC.eFlags.CANTTARGET) != 0), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
				case "lootList":
					{
						try
						{
							targetMob.LootListID = int.Parse(args[2]);
							DisplayMessage(client, "Mob loot list id changed to: {0})", targetMob.LootListID);
						}
						catch
						{
							DisplayError(client, "Type /mob for command overview.");
							return 1;
						}
					}
					break;
				case "removeloot":
					{
					/*	string lootTemplateID = args[2];
						string name = targetMob.Name;
						if (lootTemplateID.ToLower().ToString() == "all")
						{
							IList template = GameServer.Database.SelectObjects(typeof(DBLootTemplate), Expression.Eq("TemplateName",name));
							if (template != null)
							{
								foreach (DBLootTemplate loot in template)
								{
									GameServer.Database.DeleteObject(loot);
								}
								client.Out.SendMessage("Removed all items from " + targetMob.Name,
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								client.Out.SendMessage("No items found on " + targetMob.Name,
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						else
						{
							IList template = GameServer.Database.SelectObjects(typeof(DBLootTemplate), Expression.And(Expression.Eq("TemplateName",name), Expression.Eq("ItemTemplateID",lootTemplateID)));
							if (template != null)
							{
								foreach (DBLootTemplate loot in template)
								{
									GameServer.Database.DeleteObject(loot);
								}
								client.Out.SendMessage(lootTemplateID + " removed from " + targetMob.Name,
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								client.Out.SendMessage(lootTemplateID + " does not exist on " + name,
									eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}*/
					}
					break;
				case "viewloot":
					{
						LootList mobLootList = GameServer.Database.SelectObject(typeof(LootList), Expression.Eq("LootListID", targetMob.LootListID)) as LootList;
						ArrayList info = new ArrayList();

						if(mobLootList != null)
						{
							foreach(ILoot loot in mobLootList.AllLoots)
							{
								info.Add("- LootID = "+loot.LootID + "(Drop chances: " + loot.Chance +")");
								info.Add("		LootType = "+loot.GetType());
								if(loot is ItemLoot && ((ItemLoot)loot).ItemTemplate != null)
								{
									info.Add("			ItemTemplateID = "+((ItemLoot)loot).ItemTemplate.ItemTemplateID+" (Name: "+((ItemLoot)loot).ItemTemplate.Name +")");
								}
							}
						}

						client.Out.SendCustomTextWindow("[ Loot list "+ targetMob.LootListID +" ]", info);
					}
					break;
				case "copy":
					{
						//Create a new mob
						GameMob mob = null;

						foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
						{
							mob = (GameMob)assembly.CreateInstance(targetMob.GetType().FullName, true);
							if (mob != null)
								break;
						}

						if (mob == null)
						{
							client.Out.SendMessage("There was an error creating an instance of " + targetMob.GetType().FullName + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 0;
						}

						//Fill the object variables
						mob.Position = client.Player.Position;
						mob.Region = client.Player.Region;
						mob.Heading = client.Player.Heading;
						mob.Level = targetMob.Level;
						mob.Realm = targetMob.Realm;
						mob.Name = targetMob.Name;
						mob.Model = targetMob.Model;
						mob.Flags = targetMob.Flags;
						mob.MeleeDamageType = targetMob.MeleeDamageType;
						mob.RespawnInterval = targetMob.RespawnInterval;
						//Fill the living variables
						mob.MaxSpeedBase = targetMob.MaxSpeedBase;
						mob.GuildName = targetMob.GuildName;
						mob.Size = targetMob.Size;
						mob.Inventory = targetMob.Inventory;
						mob.LootListID = targetMob.LootListID;
						ABrain brain = null;
						foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
						{
							brain = (ABrain)assembly.CreateInstance(targetMob.Brain.GetType().FullName, true);
							if (brain != null)
								break;
						}
						if (brain == null)
						{
							client.Out.SendMessage("Cannot create brain, standard brain being applied", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							brain = new StandardMobBrain();
						}
						if(brain is StandardMobBrain && targetMob.Brain is StandardMobBrain)
						{
							((StandardMobBrain)brain).AggroLevel = ((StandardMobBrain)targetMob.Brain).AggroLevel;
							((StandardMobBrain)brain).AggroRange = ((StandardMobBrain)targetMob.Brain).AggroRange;
						}
						mob.OwnBrain = brain;
						brain.Body = mob;

						mob.AddToWorld();
						client.Out.SendMessage("Mob created: OID=" + mob.ObjectID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					break;
			}
			return 1;
		}

		private string CheckName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("WARNING: name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}

		private string CheckGuildName(string name, GameClient client)
		{
			if (name.Length > 47)
				client.Out.SendMessage("WARNING: guild name length=" + name.Length + " but only first 47 chars will be shown.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return name;
		}
	}
}