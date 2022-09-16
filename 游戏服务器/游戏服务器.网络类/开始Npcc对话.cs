namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 117, 长度 = 6, 注释 = "点击Npc开始与之对话")]
public sealed class 开始Npcc对话 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对象编号;

	static 开始Npcc对话()
	{
	}
}
