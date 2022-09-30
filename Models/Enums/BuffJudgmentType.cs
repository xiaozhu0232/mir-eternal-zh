using System;

namespace GameServer.Templates
{
	public enum BuffJudgmentType   //Buff判定类型
	{
		AllSkillDamage,    //所有技能伤害
		AllPhysicalDamage,    //所有物理伤害
		AllMagicDamage,    //所有魔法伤害
		AllSpecificInjuries = 4,   //所有特定伤害
		SourceSkillDamage = 8,  //来源技能伤害
		SourcePhysicalDamage = 16, //来源物理伤害
		SourceMagicDamage = 32,  //来源魔法伤害
		SourceSpecificDamage = 64  //来源特定伤害
	}
}
