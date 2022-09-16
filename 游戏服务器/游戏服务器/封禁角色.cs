using System;
using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 封禁角色 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 角色名字;

	[字段描述(0, 排序 = 1)]
	public float 封禁天数;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(角色名字, out var value) && value is 角色数据 角色数据)
		{
			角色数据.封禁日期.V = DateTime.Now.AddDays(封禁天数);
			角色数据.网络连接?.尝试断开连接(new Exception("角色被封禁, 强制下线"));
			主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 封禁到期时间: {角色数据.封禁日期}");
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色不存在");
		}
	}

	static 封禁角色()
	{
	}
}
