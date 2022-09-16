namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 65, 长度 = 16, 注释 = "同步Npcc数据")]
public sealed class 同步Npcc数据 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort 对象模板;

	[封包字段描述(下标 = 10, 长度 = 1)]
	public byte 对象质量;

	[封包字段描述(下标 = 11, 长度 = 1)]
	public byte 对象等级;

	[封包字段描述(下标 = 12, 长度 = 4)]
	public int 体力上限;

	public 同步Npcc数据()
	{
		对象质量 = 3;
	}

	static 同步Npcc数据()
	{
	}
}
