namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 267, 长度 = 8, 注释 = "双铭文位切换")]
public sealed class 双铭文位切换 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 2)]
	public ushort 当前栏位;

	[封包字段描述(下标 = 4, 长度 = 2)]
	public ushort 第一铭文;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort 第二铭文;

	static 双铭文位切换()
	{
	}
}
