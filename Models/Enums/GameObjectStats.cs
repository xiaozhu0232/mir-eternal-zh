using System;

namespace GameServer
{
	
	public enum GameObjectStats
	{
		
		未知属性,    //未知Stat
		
		最小防御 = 2,  //MinDef = 2
		
		最大防御,      //MaxDef
		
		最小魔防,    //MinMCDef
		
		最大魔防,    //MaxMCDef
		
		最小攻击,       //MinDC
		
		最大攻击,      //MaxDC
		
		最小魔法,     //MinMC
		
		最大魔法,     //MaxMC
		
		最小道术,      //MinSC
		
		最大道术,      //MaxSC
		
		最小刺术,     //MinNC
		
		最大刺术,     //MaxNC
		
		最小弓术,     //MinBC
		
		最大弓术,     //MaxBC
		
		最大体力,     //MaxHP
		
		最大魔力,     //MaxMP
		
		行走速度,   //WalkSpeed
		
		奔跑速度,    //RunSpeed
		
		物理准确,   //PhysicallyAccurate
		
		物理敏捷,    //PhysicalAgility
		
		魔法闪避,     //MagicDodge
		
		暴击概率,      
		
		暴击伤害,
		
		药品回血 = 29,
		
		药品回魔,
		
		幸运等级,     //Luck
		
		攻击速度 = 36,   //AttackSpeed
		
		体力恢复,
		
		魔力恢复,
		
		中毒躲避 = 45,
		
		最大腕力 = 50,
		
		最大穿戴,
		
		最大负重,
		
		技能标志 = 56,    //SkillSign
		
		攻杀标志,
		
 		最小圣伤 = 67,     //MinHC
		
		最大圣伤,      //MaxHC
		
		怪物伤害 = 112,
		
		怪物闪避,
		
		怪物破防,
		
		怪物命中
	}
}
