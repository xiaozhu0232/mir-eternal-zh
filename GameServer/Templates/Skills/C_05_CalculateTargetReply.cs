using System;

namespace GameServer.Templates
{
	
	public sealed class C_05_计算目标回复 : SkillTask  //C_05_CalculateTargetReply
	{
		
		public C_05_计算目标回复()   //C_05_CalculateTargetReply
		{
			
			
		}

		
		public int[] 体力回复次数;

		
		public float[] 道术叠加次数;  //Taoism叠加次数

		
		public byte[] 体力回复基数;  //PhysicalRecoveryBase

		
		public float[] 道术叠加基数;  //Taoism叠加基数

		
		public int[] 立即回复基数;

		
		public float[] 立即回复系数;

		
		public bool 增加技能经验; //GainSkillExp

		
		public ushort 经验技能编号;  //ExpSkillId
	}
}
