using System;

namespace GameServer.Templates
{
	public enum Buff判定类型   //BuffJudgmentType
	{
		所有技能伤害,    //AllSkillDamage
		所有物理伤害,    //AllPhysicalDamage
		所有魔法伤害,    //AllMagicDamage
		所有特定伤害 = 4,   //AllSpecificInjuries
		来源技能伤害 = 8,  //SourceSkillDamage
		来源物理伤害 = 16, //SourcePhysicalDamage
		来源魔法伤害 = 32,  //SourceMagicDamage
		来源特定伤害 = 64  //SourceSpecificDamage
	}
}
