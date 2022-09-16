using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 60, 长度 = 20, 注释 = "对象出现, 适用于对象主动进入视野")]
public sealed class 对象进入视野 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 1)]
	public byte 出现方式;

	[封包字段描述(下标 = 3, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 7, 长度 = 1)]
	public byte 现身姿态;

	[封包字段描述(下标 = 8, 长度 = 4)]
	public Point 现身坐标;

	[封包字段描述(下标 = 12, 长度 = 2)]
	public ushort 现身高度;

	[封包字段描述(下标 = 14, 长度 = 2)]
	public ushort 现身方向;

	[封包字段描述(下标 = 16, 长度 = 1)]
	public byte 体力比例;

	[封包字段描述(下标 = 18, 长度 = 1)]
	public byte 补充参数;

	static 对象进入视野()
	{
	}
}
