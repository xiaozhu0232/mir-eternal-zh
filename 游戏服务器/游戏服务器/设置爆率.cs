using System.Windows.Forms;
using 游戏服务器.Properties;

namespace 游戏服务器;

public sealed class 设置爆率 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public decimal 额外爆率;

	public override 执行方式 执行方式 => 执行方式.只能后台执行;

	public override void 执行命令()
	{
		if (!(额外爆率 < 0m))
		{
			if (额外爆率 >= 1m)
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 额外爆率太高");
				return;
			}
			Settings.Default.怪物额外爆率 = (自定义类.怪物额外爆率 = 额外爆率);
			Settings.Default.Save();
			主窗口.主界面.BeginInvoke((MethodInvoker)delegate
			{
				主窗口.主界面.S_怪物额外爆率.Value = 额外爆率;
			});
			主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 当前额外爆率:{自定义类.怪物额外爆率}");
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 额外爆率太低");
		}
	}

	static 设置爆率()
	{
	}
}
