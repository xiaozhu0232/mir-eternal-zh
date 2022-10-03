using System;

namespace GameServer.Templates
{
	public enum 技能触发方式  //SkillTriggerMethod
	{
		原点位置绝对触发,  //OriginAbsolutePosition
		锚点位置绝对触发,   //AnchorAbsolutePosition
		刺杀位置绝对触发,  //AssassinationAbsolutePosition
		目标命中绝对触发,  //TargetHitDefinitely
		怪物死亡绝对触发,  //MonsterDeathDefinitely
		怪物死亡换位触发,  //MonsterDeathTransposition
		怪物命中绝对触发,   //MonsterHitDefinitely
		怪物命中概率触发,  //MonsterHitProbability
		无目标锚点位触发,   //NoTargetPosition
 		目标位置绝对触发,   //TargetPositionAbsolute
		正手反手随机触发,  //ForehandAndBackhandRandom
		目标死亡绝对触发,  //TargetDeathDefinitely
		目标闪避绝对触发   //TargetMissDefinitely
	}
}
