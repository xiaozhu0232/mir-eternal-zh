namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 45, 长度 = 6, 注释 = "同步游戏ping")]
public sealed class 网速测试应答 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 当前时间;

	static 网速测试应答()
	{
	}
}
