using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 14, 长度 = 10, 注释 = "上传角色位置")]
public sealed class 上传角色位置 : 游戏封包
{
	[封包字段描述(下标 = 4, 长度 = 4)]
	public Point 坐标;

	[封包字段描述(下标 = 8, 长度 = 2)]
	public ushort 高度;

	static 上传角色位置()
	{
	}
}
