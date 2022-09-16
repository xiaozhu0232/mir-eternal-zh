namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 322, 长度 = 0, 注释 = "同步特权信息")]
public sealed class 同步特权信息 : 游戏封包
{
	[封包字段描述(下标 = 4, 长度 = 0)]
	public byte[] 字节数组;

	public 同步特权信息()
	{
		字节数组 = new byte[1] { 2 };
	}

	static 同步特权信息()
	{
	}
}
