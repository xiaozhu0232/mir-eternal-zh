namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 120, 长度 = 0, 注释 = "同步BUFF列表")]
public sealed class 同步状态列表 : 游戏封包
{
	[封包字段描述(下标 = 4, 长度 = 0)]
	public byte[] 字节数据;

	static 同步状态列表()
	{
	}
}
