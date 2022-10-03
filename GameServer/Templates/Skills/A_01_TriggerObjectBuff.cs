using System;

namespace GameServer.Templates
{
	
	public sealed class A_01_触发对象Buff : SkillTask  //A_01_TriggerObjectBuff : SkillTask 
	{
		
		public A_01_触发对象Buff()
		{
			
			
		}

		
		public bool 角色自身添加;    //角色ItSelf添加

		
		public ushort 触发Buff编号;  //触发Id

		
		public ushort 伴生Buff编号;  //伴生Id

		
		public float Buff触发概率;

		
		public bool 验证铭文技能;

		
		public ushort 所需铭文编号;  // 所需Id

		
		public bool 同组铭文无效;

		
		public bool 验证自身Buff;  //验证ItSelfBuff

		
		public ushort 自身Buff编号;  //Id

		
		public bool 触发成功移除;

		
		public bool 移除伴生Buff;

		
		public ushort 移除伴生编号;

		
		public bool 验证分组Buff;

		
		public ushort Buff分组编号;  //BuffGroupId

		
		public bool 验证目标Buff;  //VerifyTargetBuff

		
		public ushort 目标Buff编号; //目标Id

		
		public byte 所需Buff层数;

		
		public bool 验证目标类型;  //VerifyTargetType

		
		public 指定目标类型 所需目标类型;   //SpecifyTargetType 

		
		public bool 增加技能经验;  //增加技能经验 //GainSkillExp

		
		public ushort 经验技能编号;  //ExpSkillId
	}
}
