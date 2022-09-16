namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 39, 长度 = 5, 注释 = "装备技能")]
public sealed class 角色装备技能 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 1)]
	public byte 技能栏位;

	[封包字段描述(下标 = 3, 长度 = 2)]
	public ushort 技能编号;

	static 角色装备技能()
	{
	}
}
