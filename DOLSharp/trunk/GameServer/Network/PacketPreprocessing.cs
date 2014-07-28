using System;
using System.Collections.Generic;
using DawnOfLight.GameServer.Constants;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Network
{
	/// <summary>
	/// Handles preprocessing for incoming game packets.
	/// </summary>
	/// <remarks>
	/// <para>Preprocessing includes things like checking if a certain precondition exists or if a packet meets a 
	/// certain criteria before we actually handle it.
	/// </para>
	/// <para>
	/// Any time that a packet comes through with a preprocessor ID of 0, it means there is no preprocessor associated 
	/// with it, and thus we pass it through. (return true)
	/// </para>
	/// </remarks>
	public static class PacketPreprocessing
	{
		private static readonly Dictionary<ClientPackets, ClientStatus> packetIdToPreprocessorMap;
		private static readonly Dictionary<ClientStatus, Func<GameClient, GamePacketIn, bool>> preprocessors;

		static PacketPreprocessing()
		{
			packetIdToPreprocessorMap = new Dictionary<ClientPackets, ClientStatus>();
            preprocessors = new Dictionary<ClientStatus, Func<GameClient, GamePacketIn, bool>>();

			RegisterPreprocessors(ClientStatus.LoggedIn, (c, p) => c.Account != null);		// player must be logged into an account
			RegisterPreprocessors(ClientStatus.PlayerInGame, (c, p) => c.Player != null);	// player must be logged into a character
		}

		/// <summary>
		/// Registers a packet definition with a preprocessor.
		/// </summary>
		/// <param name="packetId">the ID of the packet in question</param>
		/// <param name="preprocessorId">the ID of the preprocessor for the given packet ID</param>
		public static void RegisterPacketDefinition(ClientPackets packetId, ClientStatus preprocessorId)
		{
			// if they key doesn't exist, add it, and if it does, replace it
			if (!packetIdToPreprocessorMap.ContainsKey(packetId))
			{
				packetIdToPreprocessorMap.Add(packetId, preprocessorId);
			}
			else
			{
				packetIdToPreprocessorMap[packetId] = preprocessorId;
			}
	}

		/// <summary>
		/// Registers a preprocessor.
		/// </summary>
		/// <param name="preprocessorId">the ID for the preprocessor</param>
		/// <param name="preprocessorFunc">the preprocessor delegate to use</param>
		public static void RegisterPreprocessors(ClientStatus preprocessorId, Func<GameClient, GamePacketIn, bool> preprocessorFunc)
		{
			preprocessors.Add(preprocessorId, preprocessorFunc);
		}

		/// <summary>
		/// Checks if a packet can be processed by the server.
		/// </summary>
		/// <param name="client">the client that sent the packet</param>
		/// <param name="packet">the packet in question</param>
		/// <returns>true if the packet passes all preprocessor checks; false otherwise</returns>
		public static bool CanProcessPacket(GameClient client, GamePacketIn packet)
		{
			ClientStatus preprocessorId;
			if(!packetIdToPreprocessorMap.TryGetValue((ClientPackets)packet.ID, out preprocessorId))
				return false;

			if(preprocessorId == 0)
			{
				// no processing, pass thru.
				return true;
			}

			Func<GameClient, GamePacketIn, bool> preprocessor;
			return preprocessors.TryGetValue(preprocessorId, out preprocessor) && preprocessor(client, packet);
		}
	}
}
