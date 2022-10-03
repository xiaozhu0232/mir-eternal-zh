using System;

namespace GameServer
{
	
	public enum 游戏对象类型  //游戏对象类型 GameObjectType
	{
		玩家 = 1,   //Player
		宠物 = 2,   //Pet
		怪物 = 4,  //Monster
		Npcc = 8,  //NPC
		物品 = 16,  //Item
		陷阱 = 32,  //Trap
		Chest = 64,  //宝盒
	}
}
