using System;

namespace GameServer.Templates
{
	[Flags]
	public enum GameObjectState  //游戏对象状态
	{
		正常状态 = 0,    //Normal
		硬直状态 = 1,      //Stiff
		忙绿状态 = 2,   //BusyGreen
		中毒状态 = 4,    //Poisoned
		残废状态 = 8,    //Disabled
		定身状态 = 16,   //Inmobilized
		麻痹状态 = 32,     //Paralyzed
		霸体状态 = 64,      //Hegemony
		无敌状态 = 128,    //Invencible
		隐身状态 = 256,   //Invisibility
		潜行状态 = 512,   //StealthStatus
		失神状态 = 1024,     //Absence
		暴露状态 = 2048,     //Exposed 
		Riding = 4096       //Riding //骑术
	}
}
