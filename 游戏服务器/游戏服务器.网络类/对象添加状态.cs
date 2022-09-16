namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 116, 长度 = 21, 注释 = "添加BUFF")]
public sealed class 对象添加状态 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort Buff编号;

	[封包字段描述(下标 = 8, 长度 = 4)]
	public int Buff索引;

	[封包字段描述(下标 = 12, 长度 = 4)]
	public int Buff来源;

	[封包字段描述(下标 = 16, 长度 = 4)]
	public int 持续时间;

	[封包字段描述(下标 = 20, 长度 = 1)]
	public byte Buff层数;

	static 对象添加状态()
	{
	}
}
