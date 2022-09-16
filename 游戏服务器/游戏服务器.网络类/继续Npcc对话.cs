namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 119, 长度 = 6, 注释 = "提交选项继续NPC对话")]
public sealed class 继续Npcc对话 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 4)]
	public int 对话编号;

	static 继续Npcc对话()
	{
	}
}
