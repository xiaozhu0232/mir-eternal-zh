namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 255, 长度 = 6, 注释 = "普通铭文洗练")]
public sealed class 玩家普通洗练 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 2)]
	public ushort 铭文位一;

	[封包字段描述(下标 = 4, 长度 = 2)]
	public ushort 铭文位二;

	static 玩家普通洗练()
	{
	}
}
