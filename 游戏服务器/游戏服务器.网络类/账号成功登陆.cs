namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 1002, 长度 = 0, 注释 = "客户端登录成功,同步协议")]
public sealed class 账号成功登陆 : 游戏封包
{
	[封包字段描述(下标 = 4, 长度 = 0)]
	public byte[] 协议数据;

	public override bool 是否加密 { get; set; }

	public 账号成功登陆()
	{
		是否加密 = false;
	}

	static 账号成功登陆()
	{
	}
}
