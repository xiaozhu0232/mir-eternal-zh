using System;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 582, 长度 = 48, 注释 = "查询行会名字")]
public sealed class 行会名字应答 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 行会编号;

	[封包字段描述(下标 = 6, 长度 = 25)]
	public string 行会名字;

	[封包字段描述(下标 = 31, 长度 = 4)]
	public int 会长编号;

	[封包字段描述(下标 = 35, 长度 = 4)]
	public DateTime 创建时间;

	[封包字段描述(下标 = 39, 长度 = 1)]
	public byte 行会人数;

	[封包字段描述(下标 = 40, 长度 = 1)]
	public byte 行会等级;

	static 行会名字应答()
	{
	}
}
