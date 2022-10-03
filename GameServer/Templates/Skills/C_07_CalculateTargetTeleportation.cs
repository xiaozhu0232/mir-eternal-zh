using System;

namespace GameServer.Templates
{
	
	public sealed class C_07_计算目标瞬移 : SkillTask  //C_07_CalculateTargetTeleportation
	{
		
 		public C_07_计算目标瞬移()   //C_07_CalculateTargetTeleportation
		{
			
			
		}

		
		public float[] 每级成功概率;

		
		public ushort 瞬移失败提示;

		
		public ushort 失败添加Buff;

		
		public bool 增加技能经验;  //GainSkillExp

		
		public ushort 经验技能编号;  //ExpSkillId
	}
}
