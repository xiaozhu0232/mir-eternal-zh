using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 46, 长度 = 12, 注释 = "角色走动")]
public sealed class 对象角色走动 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort 移动速度;

	[封包字段描述(下标 = 8, 长度 = 4)]
	public Point 移动坐标;

	static 对象角色走动()
	{
	}
}
