namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 33, 长度 = 4, 注释 = "角色开关技能")]
public sealed class 角色开关技能 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 2)]
	public ushort 技能编号;

	static 角色开关技能()
	{
	}
}
