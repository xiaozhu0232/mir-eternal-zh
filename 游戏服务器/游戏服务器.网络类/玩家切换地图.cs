using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 41, 长度 = 23, 注释 = "切换地图(回城/过图/传送)")]
public sealed class 玩家切换地图 : 游戏封包
{
	[封包字段描述(下标 = 6, 长度 = 4)]
	public int 地图编号;

	[封包字段描述(下标 = 10, 长度 = 4)]
	public int 路线编号;

	[封包字段描述(下标 = 14, 长度 = 4)]
	public Point 对象坐标;

	[封包字段描述(下标 = 18, 长度 = 2)]
	public ushort 对象高度;

	static 玩家切换地图()
	{
	}
}
