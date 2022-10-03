using System;

namespace GameServer.Templates
{
	
	public enum 技能闪避类型  //技能闪避类型SkillEvasionType
	{
		
		技能无法闪避,  //SkillCannotBeEvaded
		
		可被物理闪避,   //CanBePhsyicallyEvaded
		
		可被魔法闪避,    //CanBeMagicEvaded
		
		可被中毒闪避,    //CanBePoisonEvaded
		
		非怪物可闪避   //NonMonstersCanEvaded
	}
}
