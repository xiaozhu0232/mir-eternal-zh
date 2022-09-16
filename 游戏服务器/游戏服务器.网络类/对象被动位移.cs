using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 49, 长度 = 17, 注释 = "被动位移")]
public sealed class 对象被动位移 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 1)]
	public byte 第一标记;

	[封包字段描述(下标 = 7, 长度 = 4)]
	public Point 位移坐标;

	[封包字段描述(下标 = 11, 长度 = 2)]
	public ushort 第二标记;

	[封包字段描述(下标 = 13, 长度 = 2)]
	public ushort 位移朝向;

	[封包字段描述(下标 = 15, 长度 = 2)]
	public ushort 位移速度;

	static 对象被动位移()
	{
	}
}
