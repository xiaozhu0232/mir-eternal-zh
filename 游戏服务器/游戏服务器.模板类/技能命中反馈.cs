using System;

namespace 游戏服务器.模板类;

[Flags]
public enum 技能命中反馈
{
	正常 = 0,
	喷血 = 1,
	格挡 = 2,
	闪避 = 4,
	招架 = 8,
	丢失 = 0x10,
	后仰 = 0x20,
	免疫 = 0x40,
	死亡 = 0x80,
	特效 = 0x100
}
