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
//#define OUTPUT_DEBUG_INFO

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using DOL.GS;
using DOL.Events;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, 0x01 ^ 168, "Handles player position updates")]
	public class PlayerPositionUpdateHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

#if OUTPUT_DEBUG_INFO
		private static int lastX = 0;
		private static int lastY = 0;
		private static int lastUpdateTick = 0;
#endif
		/// <summary>
		/// The tolerance for speedhack etc. ... 180% of max speed
		/// This might sound much, but actually it is not because even
		/// without speedhack the distance varies very much ...
		/// It seems the client lets you walk curves FASTER than straight ahead!
		/// Very funny ;-)
		/// </summary>
		public const int DIST_TOLERANCE = 180;
		/// <summary>
		/// Stores the last update tick inside the player
		/// </summary>
		public const string LASTUPDATETICK = "PLAYERPOSITION_LASTUPDATETICK";
		/// <summary>
		/// Stores the count of times the player is above speedhack tolerance!
		/// If this value reaches 10 or more, a logfile entry is written.
		/// </summary>
		public const string SPEEDHACKCOUNTER = "SPEEDHACKCOUNTER";

		//static int lastZ=int.MinValue;
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client.Player.ObjectState != GameObject.eObjectState.Active)
				return 1;

			int packetVersion;
			if (client.Version > GameClient.eClientVersion.Version171)
			{
				packetVersion = 172;
			}
			else
			{
				packetVersion = 168;
			}

#if OUTPUT_DEBUG_INFO
			int oldSpeed = client.Player.CurrentSpeed;
#endif
			//read the state of the player
			packet.Skip(2); //PID
			ushort data = packet.ReadShort();
			int speed = (data & 0x1FF);

//			if(!GameServer.ServerRules.IsAllowedDebugMode(client) 
//				&& (speed > client.Player.MaxSpeed + SPEED_TOL))


			if ((data & 0x200) != 0)
				speed = -speed;
			client.Player.CurrentSpeed = speed;

			//Don't use the "sit" flag (data&0x1000) because it is
			//useful only for displaying sit status to other clients,
			//but NOT useful at all for changing sit states, use
			//speed instead!
			//Movement can only make players stand, but not sit them down...
			//moved to player.CurrentSpeed:
			//if(client.Player.CurrentSpeed!=0 && client.Player.Sitting)
			//	client.Player.Sit(false);

			client.Player.Strafing = ((data & 0xe000) != 0);


			int realZ = packet.ReadShort();
			ushort xOffsetInZone = packet.ReadShort();
			ushort yOffsetInZone = packet.ReadShort();
			ushort currentZoneID;
			if (packetVersion == 168)
			{
				currentZoneID = (ushort) packet.ReadByte();
				packet.Skip(1); //0x00 padding for zoneID
			}
			else
			{
				currentZoneID = packet.ReadShort();
			}

			Zone newZone = WorldMgr.GetZone(currentZoneID);
			if (newZone == null)
			{
				if (log.IsErrorEnabled)
					log.Error(client.Player.Name + "'s position in unknown zone! => " + currentZoneID);

				// move to bind point if not on it
				if (client.Player.CurrentRegionID != client.Player.PlayerCharacter.BindRegion
					|| client.Player.X != client.Player.PlayerCharacter.BindXpos
					|| client.Player.Y != client.Player.PlayerCharacter.BindYpos)
				{
					client.Out.SendMessage("Unknown zone, moving to bind point.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					client.Player.MoveTo(
						(ushort) client.Player.PlayerCharacter.BindRegion,
						client.Player.PlayerCharacter.BindXpos,
						client.Player.PlayerCharacter.BindYpos,
						(ushort) client.Player.PlayerCharacter.BindZpos,
						(ushort) client.Player.PlayerCharacter.BindHeading
						);
				}

				return 1; // TODO: what should we do? player lost in space
			}

			// move to bind if player fell through the floor
			if (realZ == 0)
			{
				client.Player.MoveTo(
					(ushort) client.Player.PlayerCharacter.BindRegion,
					client.Player.PlayerCharacter.BindXpos,
					client.Player.PlayerCharacter.BindYpos,
					(ushort) client.Player.PlayerCharacter.BindZpos,
					(ushort) client.Player.PlayerCharacter.BindHeading
					);
				return 1;
			}

			int realX = newZone.XOffset + xOffsetInZone;
			int realY = newZone.YOffset + yOffsetInZone;
			//DOLConsole.WriteLine("zx="+newZone.XOffset+" zy="+newZone.YOffset+" XOff="+xOffsetInZone+" YOff="+yOffsetInZone+" curZ="+currentZoneID);


			Zone lastPositionUpdateZone = client.Player.LastPositionUpdateZone;
			if (newZone != lastPositionUpdateZone)
			{
				//If the region changes -> make sure we don't take any falling damage
				if (lastPositionUpdateZone != null
					&& newZone.ZoneRegion.ID != lastPositionUpdateZone.ZoneRegion.ID)
				{
					client.Player.MaxLastZ = int.MinValue;
				}
				client.Out.SendMessage("You have entered " + newZone.Description + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Player.LastPositionUpdateZone = newZone;
			}

			int lastTick = client.Player.TempProperties.getIntProperty(LASTUPDATETICK, 0);
			if (lastTick != 0 && client.Account.PrivLevel == 1)
			{
				int tickDiff = Environment.TickCount - lastTick;
				int maxDist = 0;
				if (client.Player.Steed != null)
					maxDist = tickDiff*client.Player.Steed.MaxSpeed/1000; //If mounted, take mount speed
				else
					maxDist = tickDiff*client.Player.MaxSpeed/1000; //else take player speed

				maxDist = (maxDist*DIST_TOLERANCE/100);

				int plyDist = WorldMgr.GetDistance(
					client.Player.PlayerCharacter.Xpos,
					client.Player.PlayerCharacter.Ypos,
					0,
					realX,
					realY,
					0);

				//We ignore distances below 100 coordinates because below that the toleranze
				//is nonexistent... eg. sometimes bumping into a wall would cause you to move
				//more than you actually should etc...
				if (plyDist > 100 && plyDist > maxDist)
				{
					int counter = client.Player.TempProperties.getIntProperty(SPEEDHACKCOUNTER, 0);
					counter++;
					if (counter%10 == 0)
					{
						StringBuilder builder = new StringBuilder();
						builder.Append("SPEEDHACK[");
						builder.Append(counter);
						builder.Append("]: ");
						builder.Append("CharName=");
						builder.Append(client.Player.Name);
						builder.Append(" Account=");
						builder.Append(client.Account.Name);
						builder.Append(" IP=");
						builder.Append(client.TcpEndpoint);
						GameServer.Instance.LogCheatAction(builder.ToString());
					}
					client.Player.TempProperties.setProperty(SPEEDHACKCOUNTER, counter);
				}
			}
			client.Player.TempProperties.setProperty(LASTUPDATETICK, Environment.TickCount);


			// Begin ---------- New Area System -----------					
			if (client.Player.CurrentRegion.Time > client.Player.AreaUpdateTick) // check if update is needed
			{
				IList oldAreas = client.Player.CurrentAreas;
				IList newAreas = newZone.GetAreasOfSpot(client.Player);

				// Check for left areas
				if (oldAreas != null)
				{
					foreach (IArea area in oldAreas)
					{
						if (!newAreas.Contains(area))
						{
							area.OnPlayerLeave(client.Player);
						}
					}
				}
				// Check for entered areas
				foreach (IArea area in newAreas)
				{
					if (oldAreas == null || !oldAreas.Contains(area))
					{
						area.OnPlayerEnter(client.Player);
					}
				}
				// set current areas to new one...
				client.Player.CurrentAreas = newAreas;
				client.Player.AreaUpdateTick = client.Player.CurrentRegion.Time + 2000; // update every 2 seconds
			}
			// End ---------- New Area System -----------


			ushort headingflag = packet.ReadShort();
			client.Player.Heading = (ushort) (headingflag & 0xFFF);
			ushort flyingflag = packet.ReadShort();
			byte flags = (byte) packet.ReadByte();
			
			if ((flags & 0x40) != 0)
			{
				client.Player.IsPlayerMoved = false;
			}
			// ignore position changes until client confirms that he knows that he was moved
			if (!client.Player.IsPlayerMoved)
			{
				client.Player.X = realX;
				client.Player.Y = realY;
				client.Player.Z = realZ;
				// used to predict current position, should be before
				// any calculation (like fall damage)
				client.Player.MovementStartTick = Environment.TickCount;
			}
			
			client.Player.TargetInView = ((flags & 0x10) != 0);
			client.Player.GroundTargetInView = ((flags & 0x08) != 0);
			//7  6  5  4  3  2  1 0
			//15 14 13 12 11 10 9 8
			//                1 1     

			lock (client.Player.LastUniqueLocations)
			{
				GameLocation[] locations = client.Player.LastUniqueLocations;
				GameLocation loc = locations[0];
				if (loc.X != realX || loc.Y != realY || loc.Z != realZ || loc.RegionID != client.Player.CurrentRegionID)
				{
					loc = locations[locations.Length - 1];
					Array.Copy(locations, 0, locations, 1, locations.Length - 1);
					locations[0] = loc;
					loc.X = realX;
					loc.Y = realY;
					loc.Z = realZ;
					loc.Heading = client.Player.Heading;
					loc.RegionID = client.Player.CurrentRegionID;
				}
			}


			//DOLConsole.WriteLine("Player position: X="+realX+" Y="+realY+ "heading="+client.Player.Heading);

			/*
			int flyspeed = (flyingflag&0xFFF);
			if((flyingflag&0x1000)!=0)
				flyspeed = -flyspeed;

			if(flyspeed<0)
			{
				if(client.Player.Z>lastZ)
					lastZ=client.Player.Z;
			}
			else
			{
				lastZ=int.MinValue;
			}

			DOLConsole.WriteLine("Flying Speed="+flyspeed+" rest = "+(flyingflag>>15)+" "+((flyingflag>>14)&0x01)+" "+((flyingflag>>13)&0x01)+"  "+((flyingflag>>12)&0x01));
			*/
			//**************//
			//FALLING DAMAGE//
			//**************//
			if (GameServer.ServerRules.CanTakeFallDamage(client.Player))
			{
				int maxLastZ = client.Player.MaxLastZ;
				/* Are we on the ground? */
				if ((flyingflag >> 15) != 0)
				{
					int safeFallLevel = client.Player.GetAbilityLevel(Abilities.SafeFall);
					int fallSpeed = (flyingflag & 0xFFF) - 100 * safeFallLevel; // 0x7FF fall speed and 0x800 bit = fall speed overcaped
					if (fallSpeed > 400)
					{
						client.Out.SendMessage("You take falling damage!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
						int fallPercent = Math.Min(99, (fallSpeed - 401) / 6);
						if (fallPercent > 0)
						{
							if (safeFallLevel > 0)
								client.Out.SendMessage("The damage was lessened by your Safe Fall ability!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
							client.Out.SendMessage("You take " + fallPercent + "% of your max hits in damage.", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

							client.Player.Endurance -= client.Player.MaxEndurance * fallPercent / 100;
							client.Player.TakeDamage(null, eDamageType.Falling, (int)(0.01 * fallPercent * (client.Player.MaxHealth - 1)), 0);

							//Update the player's health to all other players around
							foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
								player.Out.SendCombatAnimation(null, client.Player, 0, 0, 0, 0, 0, client.Player.HealthPercent);

							//							client.Player.ChangeHealth(client.Player, GameLiving.eHealthChangeType.Unknown, -0.01*fallPercent*(client.Player.MaxHealth - 1));
						}
						client.Out.SendMessage("You lose endurance!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
					}
					client.Player.MaxLastZ = client.Player.Z;
				}
				else
				{
					// always set Z if on the ground
					if (flyingflag == 0)
						client.Player.MaxLastZ = client.Player.Z;
					// set Z if in air and higher than old Z
					else if (maxLastZ < client.Player.Z)
						client.Player.MaxLastZ = client.Player.Z;
				}
			}
			//**************//

#if OUTPUT_DEBUG_INFO
			if (lastX != 0 && lastY != 0)
			{
				if (log.IsDebugEnabled)
				{
					int timediff = Environment.TickCount - lastUpdateTick;
					int distance = WorldMgr.GetDistance(lastX, lastY, 0, realX, realY, 0);
					log.Debug("Distanze = " + distance + "Speed=" + oldSpeed + " coords/sec=" + (distance*1000/timediff));
				}
			}
			lastX = realX;
			lastY = realY;
			lastUpdateTick = Environment.TickCount;
#endif


			byte[] con168 = packet.ToArray();

			//Riding is set here!
			if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
			{
				client.Player.Heading = client.Player.Steed.Heading;

				con168[2] |= 24; //Set ride flag 00011000
				con168[12] = (byte) (client.Player.Steed.ObjectID >> 8); //heading = steed ID
				con168[13] = (byte) (client.Player.Steed.ObjectID & 0xFF);
			}
			else if (!client.Player.Alive)
			{
				con168[2] &= 0xE3; //11100011
				con168[2] |= 0x14; //Set dead flag 00010100
			}
			//diving is set here
			con168[16] &= 0xFB; //11 11 10 11
			if ((con168[16] & 0x02) != 0x00)
			{
				con168[16] |= 0x04;
			}
			con168[16] &= 0xFD; //11 11 11 01
			//stealth is set here 
			if (client.Player.IsStealthed)
			{
				con168[16] |= 0x02;
			}

			// zone ID has changed in 1.72, fix bytes 11 and 12
			byte[] con172 = (byte[]) con168.Clone();
			if (packetVersion == 168)
			{
				// client sent v168 pos update packet, fix 172 version
				con172[10] = 0;
				con172[11] = con168[10];
			}
			else
			{
				// client sent v172 pos update packet, fix 168 version
				con168[10] = con172[11];
				con168[11] = 0;
			}

			GSUDPPacketOut outpak168 = new GSUDPPacketOut(client.Out.GetPacketCode(ePackets.PlayerPosition));
			//Now copy the whole content of the packet
			outpak168.Write(con168, 0, con168.Length);
			outpak168.WritePacketLength();

			GSUDPPacketOut outpak172 = new GSUDPPacketOut(client.Out.GetPacketCode(ePackets.PlayerPosition));
			//Now copy the whole content of the packet
			outpak172.Write(con172, 0, con172.Length);
			outpak172.WritePacketLength();
			
			byte[] pak168 = outpak168.GetBuffer();
			byte[] pak172 = outpak172.GetBuffer();
			outpak168 = null;
			outpak172 = null;

			foreach (GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				//No position updates for ourselves
				if (player == client.Player)
					continue;

				//Check stealth
				if (!client.Player.IsStealthed || player.CanDetect(client.Player))
				{
					//forward the position packet like normal!
					if (player.Client.Version > GameClient.eClientVersion.Version171)
						player.Out.SendUDP(pak172);
					else
						player.Out.SendUDP(pak168);
				}
				else
					player.Out.SendObjectDelete(client.Player); //remove the stealthed player from view
			}

			//Notify the GameEventMgr of the moving player
			GameEventMgr.Notify(GamePlayerEvent.Moving, client.Player);

			//client.Player.NotifyPositionChange();

			return 1;
		}
	}
}