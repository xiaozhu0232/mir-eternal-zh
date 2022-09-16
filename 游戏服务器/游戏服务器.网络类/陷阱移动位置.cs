using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 124, 长度 = 18, 注释 = "陷阱移动")]
public sealed class 陷阱移动位置 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 陷阱编号;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort 移动速度;

	[封包字段描述(下标 = 8, 长度 = 4)]
	public int 未知参数;

	[封包字段描述(下标 = 12, 长度 = 4)]
	public Point 移动坐标;

	[封包字段描述(下标 = 16, 长度 = 4)]
	public ushort 移动高度;

	static 陷阱移动位置()
	{
	}
}
