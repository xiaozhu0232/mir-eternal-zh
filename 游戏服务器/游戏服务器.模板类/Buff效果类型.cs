using System;

namespace 游戏服务器.模板类;

[Flags]
public enum Buff效果类型
{
	技能标志 = 0,
	状态标志 = 1,
	造成伤害 = 2,
	属性增减 = 4,
	伤害增减 = 8,
	创建陷阱 = 0x10,
	生命回复 = 0x20,
	诱惑提升 = 0x40
}
