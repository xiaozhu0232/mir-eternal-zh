using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 123, 长度 = 30, 注释 = "陷阱出现")]
public sealed class 陷阱进入视野 : 游戏封包
{
	[封包字段描述(下标 = 4, 长度 = 4)]
	public int 地图编号;

	[封包字段描述(下标 = 8, 长度 = 4)]
	public int 来源编号;

	[封包字段描述(下标 = 12, 长度 = 2)]
	public ushort 陷阱编号;

	[封包字段描述(下标 = 14, 长度 = 4)]
	public Point 陷阱坐标;

	[封包字段描述(下标 = 18, 长度 = 2)]
	public ushort 陷阱高度;

	[封包字段描述(下标 = 20, 长度 = 2)]
	public ushort 持续时间;

	[封包字段描述(下标 = 22, 长度 = 2)]
	public ushort 未知参数;

	[封包字段描述(下标 = 24, 长度 = 4)]
	public Point 未知坐标;

	[封包字段描述(下标 = 28, 长度 = 2)]
	public ushort 未知高度;

	static 陷阱进入视野()
	{
	}
}
