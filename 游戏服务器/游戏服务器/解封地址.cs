using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 解封地址 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 对应地址;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (系统数据.数据.网络封禁.ContainsKey(对应地址))
		{
			系统数据.数据.解封网络(对应地址);
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令已经执行, 地址已经解除封禁");
		}
		else if (!系统数据.数据.网卡封禁.ContainsKey(对应地址))
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 对应地址未处于封禁状态");
		}
		else
		{
			系统数据.数据.解封网卡(对应地址);
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令已经执行, 地址已经解除封禁");
		}
	}

	static 解封地址()
	{
	}
}
