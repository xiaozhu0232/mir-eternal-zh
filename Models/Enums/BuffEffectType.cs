using System;

namespace GameServer.Templates
{
	[Flags]
	public enum Buff效果类型  //Buff效果类型
	{
		技能标志 = 0,   //技能标志 //SkillSign
		状态标志 = 1,  //状态标志 //StatusFlag
		造成伤害 = 2,  //造成伤害//CausesSomeDamages
		属性增减 = 4,  //属性增减 //StatsIncOrDec
		伤害增减 = 8, //伤害增减 //DamageIncOrDec
 		创建陷阱 = 16,  //创建陷阱 //CreateTrap
		生命回复 = 32,  //生命回复 //LifeRecovery 
		诱惑提升 = 64,  //诱惑提升 //TemptationBoost
		Riding = 128  //骑术//Riding
	}
}
