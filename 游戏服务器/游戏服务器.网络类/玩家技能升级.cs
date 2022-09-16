namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 125, 长度 = 5, 注释 = "技能升级")]
public sealed class 玩家技能升级 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 2)]
	public ushort 技能编号;

	[封包字段描述(下标 = 4, 长度 = 1)]
	public byte 技能等级;

	static 玩家技能升级()
	{
	}
}
