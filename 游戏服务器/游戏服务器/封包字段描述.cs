using System;

namespace 游戏服务器;

[AttributeUsage(AttributeTargets.Field)]
public class 封包字段描述 : Attribute
{
	public ushort 下标;

	public ushort 长度;

	public bool 反向;

	static 封包字段描述()
	{
	}
}
