using System.Drawing;

namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.服务器, 编号 = 94, 长度 = 25, 注释 = "开始释放技能(技能信息,目标,坐标,速率)")]
public sealed class 开始释放技能 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	[封包字段描述(下标 = 6, 长度 = 2)]
	public ushort 技能编号;

	[封包字段描述(下标 = 8, 长度 = 1)]
	public byte 技能等级;

	[封包字段描述(下标 = 9, 长度 = 1)]
	public byte 技能铭文;

	[封包字段描述(下标 = 10, 长度 = 4)]
	public int 目标编号;

	[封包字段描述(下标 = 14, 长度 = 4)]
	public Point 锚点坐标;

	[封包字段描述(下标 = 18, 长度 = 2)]
	public ushort 锚点高度;

	[封包字段描述(下标 = 20, 长度 = 2)]
	public ushort 加速率一;

	[封包字段描述(下标 = 22, 长度 = 2)]
	public ushort 加速率二;

	[封包字段描述(下标 = 24, 长度 = 1)]
	public byte 动作编号;

	public 开始释放技能()
	{
		加速率一 = 10000;
		加速率二 = 10000;
	}

	static 开始释放技能()
	{
	}
}
