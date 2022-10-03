using System;

namespace GameServer.Templates
{
	[Flags]
	public enum GameObjectRelationship  //游戏对象关系
	{
		自身 = 1,  //ItSelf
		友方 = 2,  //Friendly
		敌对 = 4  //Hostility
	}
}
