using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 18, 长度 = 6, 注释 = "角色走动")]
public sealed class 客户角色走动 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public Point 坐标;

	static 客户角色走动()
	{
	}
}
