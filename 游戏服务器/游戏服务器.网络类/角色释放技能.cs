using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 34, 长度 = 16, 注释 = "释放技能")]
public sealed class 角色释放技能 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 2)]
	public ushort 技能编号;

	[封包字段描述(下标 = 4, 长度 = 1)]
	public byte 动作编号;

	[封包字段描述(下标 = 6, 长度 = 4)]
	public int 目标编号;

	[封包字段描述(下标 = 10, 长度 = 4)]
	public Point 锚点坐标;

	[封包字段描述(下标 = 14, 长度 = 2)]
	public ushort 锚点高度;

	static 角色释放技能()
	{
	}
}
