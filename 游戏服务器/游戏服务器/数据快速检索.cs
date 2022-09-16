using System;

namespace 游戏服务器;

[AttributeUsage(AttributeTargets.Class)]
public class 数据快速检索 : Attribute
{
	public string 检索字段;

	static 数据快速检索()
	{
	}
}
