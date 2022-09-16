using System.Text;
using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 角色改名 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 角色名字;

	[字段描述(0, 排序 = 1)]
	public string 新角色名;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(角色名字, out var value) && value is 角色数据 角色数据)
		{
			if (角色数据.网络连接 == null && 角色数据.所属账号.V.网络连接 == null)
			{
				if (Encoding.UTF8.GetBytes(新角色名).Length <= 24)
				{
					if (游戏数据网关.角色数据表.检索表.ContainsKey(新角色名))
					{
						主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 名字已被注册");
						return;
					}
					游戏数据网关.角色数据表.检索表.Remove(角色数据.角色名字.V);
					角色数据.角色名字.V = 新角色名;
					游戏数据网关.角色数据表.检索表.Add(角色数据.角色名字.V, 角色数据);
					主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 角色当前名字: {角色数据}");
				}
				else
				{
					主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色名字太长");
				}
			}
			else
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 账号必须下线");
			}
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色不存在");
		}
	}

	static 角色改名()
	{
	}
}
