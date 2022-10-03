using System;

namespace GameServer.Templates
{
	[Flags]
	public enum Buff效果类型  //Buff效果类型
	{
		技能标志 = 0,   //SkillSign
		状态标志 = 1,  //StatusFlag
		造成伤害 = 2,  //CausesSomeDamages
		属性增减 = 4,  //StatsIncOrDec
		伤害增减 = 8,  //DamageIncOrDec
 		创建陷阱 = 16,  //CreateTrap
		生命回复 = 32,  //LifeRecovery 
		诱惑提升 = 64,  //TemptationBoost
		Riding = 128   //Riding
	}
}
