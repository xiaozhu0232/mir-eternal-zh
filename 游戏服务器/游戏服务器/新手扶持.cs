using System.Windows.Forms;
using 游戏服务器.Properties;

namespace 游戏服务器;

public sealed class 新手扶持 : GM命令
{
	[字段描述(0, 排序 = 0)]
	public byte 扶持等级;

	public override 执行方式 执行方式 => 执行方式.只能后台执行;

	public override void 执行命令()
	{
		Settings.Default.新手扶持等级 = (自定义类.新手扶持等级 = 扶持等级);
		Settings.Default.Save();
		主窗口.主界面.BeginInvoke((MethodInvoker)delegate
		{
			主窗口.主界面.S_新手扶持等级.Value = 扶持等级;
		});
		主窗口.添加命令日志($"<= @{GetType().Name} 命令已经执行, 当前扶持等级:{自定义类.新手扶持等级}");
	}

	static 新手扶持()
	{
	}
}
