using System;
using DawnOfLight.Database;
using DawnOfLight.GameServer.GameObjects;

namespace DawnOfLight.GameServer.Housing
{
	/// <summary>
	/// House item interface.
	/// </summary>
	/// <author>Aredhel</author>
	public interface IHouseHookpointItem
	{
		bool Attach(House house, uint hookpointID, ushort heading);
		bool Attach(House house, DBHouseHookpointItem hookedItem);
		bool Detach(GamePlayer player);
		int Index { get; }
		String TemplateID { get; }
	}
}
