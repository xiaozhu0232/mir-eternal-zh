using System;

namespace GameServer.Templates
{
	
	public sealed class C_00_计算技能锚点 : SkillTask  //C_00_CalculateSkillAnchor
	{
		
		public C_00_计算技能锚点()   //C_00_CalculateSkillAnchor
		{
			
			
		}

		
		public bool 计算当前位置;

		
		public bool 计算当前方向;

		
		public int 技能最远距离; //MaxDistance

		
		public int 技能最近距离;
	}
}
