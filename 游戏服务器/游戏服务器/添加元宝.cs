using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器;

public sealed class 添加元宝 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 角色名字;

	[字段描述(0, 排序 = 1)]
	public int 元宝数量;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(角色名字, out var value) && value is 角色数据 角色数据)
		{
			角色数据.元宝数量 += 元宝数量;
			角色数据.网络连接?.发送封包(new 同步元宝数量
			{
				元宝数量 = 角色数据.元宝数量
			});
			主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 当前元宝数量: {角色数据.元宝数量}");
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色不存在");
		}
	}

	static 添加元宝()
	{
	}
}
