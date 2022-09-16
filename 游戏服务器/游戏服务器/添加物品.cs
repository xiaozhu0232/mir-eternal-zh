using 游戏服务器.模板类;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器;

public sealed class 添加物品 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 角色名字;

	[字段描述(0, 排序 = 1)]
	public string 物品名字;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(角色名字, out var value) && value is 角色数据 角色数据)
		{
			if (游戏物品.检索表.TryGetValue(物品名字, out var value2))
			{
				if (角色数据.角色背包.Count < 角色数据.背包大小.V)
				{
					if (value2.物品持久 == 0)
					{
						主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 不能添加物品");
						return;
					}
					byte b = byte.MaxValue;
					byte b2 = 0;
					while (b2 < 角色数据.背包大小.V)
					{
						if (角色数据.角色背包.ContainsKey(b2))
						{
							b2 = (byte)(b2 + 1);
							continue;
						}
						b = b2;
						break;
					}
					if (value2 is 游戏装备 模板)
					{
						角色数据.角色背包[b] = new 装备数据(模板, 角色数据, 1, b, 随机生成: true);
					}
					else if (value2.持久类型 != 物品持久分类.容器)
					{
						if (value2.持久类型 == 物品持久分类.堆叠)
						{
							角色数据.角色背包[b] = new 物品数据(value2, 角色数据, 1, b, 1);
						}
						else
						{
							角色数据.角色背包[b] = new 物品数据(value2, 角色数据, 1, b, value2.物品持久);
						}
					}
					else
					{
						角色数据.角色背包[b] = new 物品数据(value2, 角色数据, 1, b, 0);
					}
					角色数据.网络连接?.发送封包(new 玩家物品变动
					{
						物品描述 = 角色数据.角色背包[b].字节描述()
					});
					主窗口.添加命令日志("<= @" + GetType().Name + " 命令已经执行, 物品已经添加到角色背包");
				}
				else
				{
					主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色背包已满");
				}
			}
			else
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 物品不存在");
			}
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色不存在");
		}
	}

	static 添加物品()
	{
	}
}
