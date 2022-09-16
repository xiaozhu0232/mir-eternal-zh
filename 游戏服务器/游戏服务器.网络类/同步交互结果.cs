namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 167, 长度 = 0, 注释 = "Npcc交互结果")]
public sealed class 同步交互结果 : 游戏封包
{
	[封包字段描述(下标 = 4, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 8, 长度 = 0)]
	public byte[] 交互文本;

	static 同步交互结果()
	{
	}
}
