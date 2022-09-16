namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 1010, 长度 = 6, 注释 = "同步网关ping")]
public sealed class 登陆查询应答 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 当前时间;

	static 登陆查询应答()
	{
	}
}
