using System;
using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 角色转移 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 角色名字;

	[字段描述(0, 排序 = 1)]
	public string 新账号名;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(角色名字, out var value) && value is 角色数据 角色数据)
		{
			游戏数据 value2;
			if (!角色数据.所属账号.V.角色列表.Contains(角色数据))
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色处于删除状态");
			}
			else if (角色数据.封禁日期.V > DateTime.Now)
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色处于封禁状态");
			}
			else if (角色数据.所属账号.V.封禁日期.V > DateTime.Now)
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 原账号处于封禁状态");
			}
			else if (游戏数据网关.账号数据表.检索表.TryGetValue(新账号名, out value2) && value2 is 账号数据 账号数据)
			{
				if (!(账号数据.封禁日期.V > DateTime.Now))
				{
					if (账号数据.角色列表.Count >= 4)
					{
						主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 转移的账号角色数量已达上限");
					}
					else if (角色数据.所属账号.V.网络连接 == null && 账号数据.网络连接 == null)
					{
						角色数据.所属账号.V.角色列表.Remove(角色数据);
						角色数据.所属账号.V = 账号数据;
						账号数据.角色列表.Add(角色数据);
						主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 角色当前账号:{角色数据.所属账号}");
					}
					else
					{
						主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 双方账号必须下线");
					}
				}
				else
				{
					主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 转移账号处于封禁状态");
				}
			}
			else
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 转移账号不存在或从未登录");
			}
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色不存在");
		}
	}

	static 角色转移()
	{
	}
}
