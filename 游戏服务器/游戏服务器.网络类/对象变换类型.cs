namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 73, 长度 = 7, 注释 = "Npc变换类型")]
public sealed class 对象变换类型 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 1)]
	public byte 改变类型;

	static 对象变换类型()
	{
	}
}
