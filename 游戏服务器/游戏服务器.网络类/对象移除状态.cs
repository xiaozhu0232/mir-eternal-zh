namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 117, 长度 = 10, 注释 = "移除BUFF")]
public sealed class 对象移除状态 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 4)]
	public int Buff索引;

	static 对象移除状态()
	{
	}
}
