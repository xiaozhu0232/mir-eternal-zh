namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 264, 长度 = 16, 注释 = "高级铭文洗练")]
public sealed class 玩家高级洗练 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 2)]
	public ushort 洗练结果;

	[封包字段描述(下标 = 4, 长度 = 2)]
	public ushort 铭文位一;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort 铭文位二;

	static 玩家高级洗练()
	{
	}
}
