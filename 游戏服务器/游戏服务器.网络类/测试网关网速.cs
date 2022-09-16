namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 1007, 长度 = 6, 注释 = "帧同步, 请求Ping")]
public sealed class 测试网关网速 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 客户时间;

	static 测试网关网速()
	{
	}
}
