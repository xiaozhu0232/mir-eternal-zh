using System;

namespace GameServer.Templates
{
	
	public sealed class A_02_触发陷阱技能 : SkillTask //A_02_TriggerTrapSkills
	{
		
		public A_02_触发陷阱技能()
		{
			
			
		}

		
		public string 触发陷阱技能; //TriggerTrapSkills

		
		public 技能范围类型 触发陷阱数量; //技能范围类型 NumberTrapsTriggered

		
		public bool 增加技能经验;  //GainSkillExp

		
		public ushort 经验技能编号;  //ExpSkillId
	}
}
