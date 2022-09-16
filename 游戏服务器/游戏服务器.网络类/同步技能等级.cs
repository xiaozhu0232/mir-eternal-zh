namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 127, 长度 = 7, 注释 = "同步技能等级数据")]
public sealed class 同步技能等级 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 2)]
	public ushort 技能编号;

	[封包字段描述(下标 = 4, 长度 = 2)]
	public ushort 当前经验;

	[封包字段描述(下标 = 6, 长度 = 1)]
	public byte 当前等级;

	static 同步技能等级()
	{
	}
}
