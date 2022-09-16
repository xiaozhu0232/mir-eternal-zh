namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 50, 长度 = 10, 注释 = "对象转动")]
public sealed class 对象转动方向 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort 转向耗时;

	[封包字段描述(下标 = 8, 长度 = 2)]
	public ushort 对象朝向;

	static 对象转动方向()
	{
	}
}
