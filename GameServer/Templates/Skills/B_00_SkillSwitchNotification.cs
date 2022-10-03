using System;

namespace GameServer.Templates
{
	
	public sealed class B_00_技能切换通知 : SkillTask //B_00_SkillSwitchNotification
	{
		
		public B_00_技能切换通知()   //B_00_SkillSwitchNotification
		{
			
			
		}

		
		public ushort 技能标记编号;  //SkillTagId

		public bool 允许移除标记;

		public int 角色忙绿时间;
	}
}
