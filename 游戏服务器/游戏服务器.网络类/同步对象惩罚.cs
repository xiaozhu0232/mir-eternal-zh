namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 80, 长度 = 10, 注释 = "同步PK惩罚值")]
public sealed class 同步对象惩罚 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 4)]
	public int PK值惩罚;

	static 同步对象惩罚()
	{
	}
}
