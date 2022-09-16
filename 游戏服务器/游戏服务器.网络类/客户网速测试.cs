namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 23, 长度 = 6, 注释 = "帧同步, 请求Ping")]
public sealed class 客户网速测试 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 客户时间;

	static 客户网速测试()
	{
	}
}
