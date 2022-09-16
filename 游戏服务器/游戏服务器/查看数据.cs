using System;
using System.Collections.Generic;
using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 查看数据 : GM命令
{
	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		主窗口.添加命令日志("<= @" + GetType().Name + " 命令已经执行, 数据库详情如下:");
		foreach (KeyValuePair<Type, 数据表基类> item in 游戏数据网关.数据类型表)
		{
			主窗口.添加命令日志($"{item.Value.数据类型.Name}  数量: {item.Value.数据表.Count}");
		}
	}

	static 查看数据()
	{
	}
}
