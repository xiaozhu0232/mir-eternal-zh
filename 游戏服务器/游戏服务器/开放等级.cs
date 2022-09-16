using System.Windows.Forms;
using 游戏服务器.Properties;

namespace 游戏服务器;

public sealed class 开放等级 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public byte 最高等级;

	public override 执行方式 执行方式 => 执行方式.只能后台执行;

	public override void 执行命令()
	{
		if (最高等级 > 自定义类.游戏开放等级)
		{
			Settings.Default.游戏开放等级 = (自定义类.游戏开放等级 = 最高等级);
			Settings.Default.Save();
			主窗口.主界面.BeginInvoke((MethodInvoker)delegate
			{
				主窗口.主界面.S_游戏开放等级.Value = 最高等级;
			});
			主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 当前开放等级:{自定义类.游戏开放等级}");
		}
		else
		{
			主窗口.添加命令日志("<= @" + GetType().Name + " 命令执行失败, 等级低于当前已开放等级");
		}
	}

	static 开放等级()
	{
	}
}
