using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 39, 长度 = 17, 注释 = "进入场景(包括商店/随机卷)")]
public sealed class 玩家进入场景 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 地图编号;

	[封包字段描述(下标 = 6, 长度 = 4)]
	public int 路线编号;

	[封包字段描述(下标 = 10, 长度 = 1)]
	public byte 路线状态;

	[封包字段描述(下标 = 11, 长度 = 4)]
	public Point 当前坐标;

	[封包字段描述(下标 = 15, 长度 = 2)]
	public ushort 当前高度;

	static 玩家进入场景()
	{
	}
}
