using System.Windows.Forms;
using 游戏服务器.Properties;

namespace 游戏服务器;

public sealed class 设置经验 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public decimal 经验倍率;

	public override 执行方式 执行方式 => 执行方式.只能后台执行;

	public override void 执行命令()
	{
		if (!(经验倍率 <= 0m))
		{
			if (经验倍率 > 1000000m)
			{
				主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 经验倍率太高");
				return;
			}
			Settings.Default.怪物经验倍率 = (自定义类.怪物经验倍率 = 经验倍率);
			Settings.Default.Save();
			主窗口.主界面.BeginInvoke((MethodInvoker)delegate
			{
				主窗口.主界面.S_怪物经验倍率.Value = 经验倍率;
			});
			主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 当前经验倍率:{自定义类.怪物经验倍率}");
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 经验倍率太低");
		}
	}

	static 设置经验()
	{
	}
}
