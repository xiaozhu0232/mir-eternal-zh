namespace 游戏服务器.网络类;

[封包信息描述(来源 = 封包来源.客户端, 编号 = 1002, 长度 = 40, 注释 = "创建角色")]
public sealed class 客户创建角色 : 游戏封包
{
	[封包字段描述(下标 = 2, 长度 = 32)]
	public string 名字;

	[封包字段描述(下标 = 34, 长度 = 1)]
	public byte 性别;

	[封包字段描述(下标 = 35, 长度 = 1)]
	public byte 职业;

	[封包字段描述(下标 = 36, 长度 = 1)]
	public byte 发型;

	[封包字段描述(下标 = 37, 长度 = 1)]
	public byte 发色;

	[封包字段描述(下标 = 38, 长度 = 1)]
	public byte 脸型;

	static 客户创建角色()
	{
	}
}
