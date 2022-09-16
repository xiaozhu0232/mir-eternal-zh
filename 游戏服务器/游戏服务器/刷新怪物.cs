using System.Drawing;
using 游戏服务器.地图类;
using 游戏服务器.模板类;

namespace 游戏服务器;

public sealed class 刷新怪物 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public string 怪物名字;

	[字段描述(0, 排序 = 1)]
	public byte 地图编号;

	[字段描述(0, 排序 = 2)]
	public int 地图X坐标;

	[字段描述(0, 排序 = 3)]
	public int 地图Y坐标;

	public override 执行方式 执行方式 => 执行方式.优先后台执行;

	public override void 执行命令()
	{
		if (!游戏怪物.数据表.TryGetValue(怪物名字, out var value))
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 怪物不存在");
			return;
		}
		if (!游戏地图.数据表.TryGetValue(地图编号, out var value2))
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 地图不存在");
			return;
		}
		地图实例 出生地图 = 地图处理网关.分配地图(value2.地图编号);
		new 怪物实例(value, 出生地图, 0, new Point[1]
		{
			new Point(地图X坐标, 地图Y坐标)
		}, 禁止复活: true, 立即刷新: true);
	}

	static 刷新怪物()
	{
		
	}

	public 刷新怪物()
	{
		
	}
}
