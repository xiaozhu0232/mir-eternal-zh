using System;

namespace GameServer.Templates
{
	
	public sealed class A_00_触发子类技能 : SkillTask  //A_00_TriggerSubSkills : SkillTask 
	{
		
		public A_00_触发子类技能()
		{
			
			
		}

		
		public 技能触发方式 技能触发方式;  //SkillTriggerMethod

		
		public string 触发技能名字;     //触发SkillName

		
		public string 反手技能名字;  // 反手SkillName

		
		public bool 计算触发概率;  //CalculateTriggerProbability

		
		public bool 计算幸运概率;  //计算幸运概率//CalculateLuckyProbability

		
		public float 技能触发概率;

		
		public ushort 增加概率Buff;

		
		public float Buff增加系数;

		
		public bool 验证自身Buff;  //验证自身Buff //验证ItSelfBuff

		
		public ushort 自身Buff编号;  //Id

		
		public bool 触发成功移除;

		
		public bool 验证铭文技能;

		
		public ushort 所需铭文编号; //所需铭文编号 //所需Id

		
		public bool 同组铭文无效;
	}
}
