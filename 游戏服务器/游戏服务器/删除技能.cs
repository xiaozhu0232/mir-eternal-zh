using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 删除技能 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 角色名字;

	[字段描述(0, 排序 = 1)]
	public ushort 技能编号;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (游戏数据网关.角色数据表.检索表.TryGetValue(角色名字, out var value) && value is 角色数据 角色数据)
		{
			角色数据.技能数据.TryGetValue(技能编号, out var v);
			v.删除数据();
		}
		主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 角色不存在");
	}

	static 删除技能()
	{
	}
}
