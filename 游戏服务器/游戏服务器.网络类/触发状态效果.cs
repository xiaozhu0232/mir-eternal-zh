namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 119, 长度 = 22, 注释 = "BUFF效果")]
public sealed class 触发状态效果 : 游戏封包
{
	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort Buff编号;

	[封包字段描述(下标 = 8, 长度 = 4)]
	public int Buff来源;

	[封包字段描述(下标 = 12, 长度 = 4)]
	public int Buff目标;

	[封包字段描述(下标 = 16, 长度 = 4)]
	public int 血量变化;

	static 触发状态效果()
	{
	}
}
