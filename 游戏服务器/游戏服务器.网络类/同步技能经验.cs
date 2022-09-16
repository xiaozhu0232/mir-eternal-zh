namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 126, 长度 = 6, 注释 = "同步技能经验")]
public sealed class 同步技能经验 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 2)]
	public ushort 技能编号;

	[封包字段描述(下标 = 4, 长度 = 2)]
	public ushort 当前经验;

	static 同步技能经验()
	{
	}
}
