using System;

namespace GameServer.Templates
{
	
	public sealed class C_06_计算宠物召唤 : SkillTask  //C_06_CalculatePetSummoning
	{
		
		public C_06_计算宠物召唤()   //C_06_CalculatePetSummoning
		{
			
			
		}

		
		public string 召唤宠物名字;  //PetName

		
		public bool 怪物召唤同伴;  //Companion

		
		public byte[] 召唤宠物数量;  //SpawnCount

		
		public byte[] 宠物等级上限;  //LevelCap

		
		public bool 增加技能经验;  //GainSkillExp

		
		public ushort 经验技能编号;  //ExpSkillId

		
		public bool 宠物绑定武器;  //PetBoundWeapons

		
		public bool 检查技能铭文;  //CheckSkillInscriptions
	}
}
