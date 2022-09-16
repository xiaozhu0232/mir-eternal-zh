using System;
using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 解封账号 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 账号名字;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (游戏数据网关.账号数据表.检索表.TryGetValue(账号名字, out var value) && value is 账号数据 账号数据)
		{
			账号数据.封禁日期.V = default(DateTime);
			主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 封禁到期时间: {账号数据.封禁日期}");
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 账号不存在");
		}
	}

	static 解封账号()
	{
	}
}
