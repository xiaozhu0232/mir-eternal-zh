using System;

namespace 游戏服务器.模板类;

[Flags]
public enum 游戏对象关系
{
	自身 = 1,
	友方 = 2,
	敌对 = 4
}
