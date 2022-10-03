using System;

namespace GameServer.Templates
{
	
	[Flags]
	public enum 指定目标类型 //SpecifyTargetType
	{
		无 = 0,  //None
		低级目标 = 1,  //LowLevelTarget
		带盾法师 = 2,  //ShieldMage
		低级怪物 = 4,  //LowLevelMonster
		低血怪物 = 8,  //LowBloodMonster
		普通怪物 = 16,  //Normal
		所有怪物 = 32,  //AllMonsters
		不死生物 = 64,  //Undead
		虫族生物 = 128,  //ZergCreature
		沃玛怪物 = 256,  //WomaMonster
		猪类怪物 = 512,  //PigMonster
		祖玛怪物 = 1024,  //ZumaMonster
		精英怪物 = 2048,  //EliteMonsters 
		所有宠物 = 4096,  //AllPets
		背刺目标 = 8192,  //Backstab
		魔龙怪物 = 16384,  //DragonMonster
		所有玩家 = 32768  //AllPlayers
	}
}
