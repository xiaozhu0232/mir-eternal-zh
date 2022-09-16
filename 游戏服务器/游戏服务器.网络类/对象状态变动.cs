namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 118, 长度 = 21, 注释 = "BUFF变动")]
public sealed class 对象状态变动 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort Buff编号;

	[封包字段描述(下标 = 8, 长度 = 4)]
	public int Buff索引;

	[封包字段描述(下标 = 12, 长度 = 1)]
	public byte 当前层数;

	[封包字段描述(下标 = 13, 长度 = 4)]
	public int 剩余时间;

	[封包字段描述(下标 = 17, 长度 = 4)]
	public int 持续时间;

	static 对象状态变动()
	{
	}
}
