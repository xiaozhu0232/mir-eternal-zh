using System;
using System.Text.RegularExpressions;
using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 封禁网卡 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 物理地址;

	[字段描述(0, 排序 = 1)]
	public float 封禁天数;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (Regex.IsMatch(物理地址, "^([0-9a-fA-F]{2}(?:[:-]?[0-9a-fA-F]{2}){5})$"))
		{
			系统数据.数据.封禁网卡(物理地址, DateTime.Now.AddDays(封禁天数));
			主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 封禁到期时间: {DateTime.Now.AddDays(封禁天数)}");
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 地址格式错误");
		}
	}

	static 封禁网卡()
	{
	}
}
