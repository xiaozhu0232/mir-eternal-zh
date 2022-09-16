namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 71, 长度 = 36, 注释 = "同步对象Buff")]
public sealed class 同步对象Buff : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 34)]
	public byte[] 字节描述;

	static 同步对象Buff()
	{
	}
}
