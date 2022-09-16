using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 48, 长度 = 13, 注释 = "角色停止")]
public sealed class 对象角色停止 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 7, 长度 = 4)]
	public Point 对象坐标;

	[封包字段描述(下标 = 11, 长度 = 2)]
	public ushort 对象高度;

	static 对象角色停止()
	{
	}
}
