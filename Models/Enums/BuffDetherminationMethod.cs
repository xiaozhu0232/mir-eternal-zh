using System;

namespace GameServer.Templates
{
	public enum BuffDetherminationMethod   //Buff判定方式
	{
		主动攻击增伤,     //ActiveAttackDamageBoost 
		主动攻击减伤,     //ActiveAttackDamageReduction
		被动受伤增伤,     //PassiveInjuryIncrease
		被动受伤减伤      //PassiveInjuryReduction
	}
}
