using System;
using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 找回角色 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 角色名字;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(角色名字, out var value) && value is 角色数据 角色数据)
		{
			if (!(角色数据.删除日期.V == default(DateTime)) && 角色数据.所属账号.V.删除列表.Contains(角色数据))
			{
				if (角色数据.所属账号.V.角色列表.Count < 4)
				{
					if (角色数据.所属账号.V.网络连接 != null)
					{
						主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 账号必须下线");
						return;
					}
					数据监视器<DateTime> 删除日期 = 角色数据.删除日期;
					DateTime dateTime2 = (角色数据.冻结日期.V = default(DateTime));
					DateTime dateTime3 = dateTime2;
					dateTime2 = (删除日期.V = dateTime3);
					角色数据.所属账号.V.删除列表.Remove(角色数据);
					角色数据.所属账号.V.角色列表.Add(角色数据);
					主窗口.添加命令日志("<= @" + GetType().Name + " 命令已经执行, 角色恢复成功");
				}
				else
				{
					主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色列表已满");
				}
			}
			else
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色未被删除");
			}
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色不存在");
		}
	}

	static 找回角色()
	{
	}
}
