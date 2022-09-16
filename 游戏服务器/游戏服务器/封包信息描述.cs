using System;

namespace 游戏服务器;

[AttributeUsage(AttributeTargets.Class)]
public class 封包信息描述 : Attribute
{
	public 封包来源 来源;

	public ushort 编号;

	public ushort 长度;

	public string 注释;

	static 封包信息描述()
	{
	}
}
