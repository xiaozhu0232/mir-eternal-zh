using System.Collections.Generic;

namespace 游戏服务器;

public sealed class 查看命令 : GM命令
{
	public override 执行方式 执行方式 => 执行方式.前台立即执行;

	public override void 执行命令()
	{
		主窗口.添加命令日志("以下为所有支持的GM命令:");
		foreach (KeyValuePair<string, string> item in GM命令.命令格式)
		{
			主窗口.添加命令日志(item.Value);
		}
	}

	static 查看命令()
	{
	}
}
