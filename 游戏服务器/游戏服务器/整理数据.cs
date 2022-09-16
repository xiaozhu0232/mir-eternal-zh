using System.Threading.Tasks;
using System.Windows.Forms;
using 游戏服务器.数据类;

namespace 游戏服务器;

public sealed class 整理数据 : GM命令
{
	public override 执行方式 执行方式 => 执行方式.只能空闲执行;

	public override void 执行命令()
	{
		if (MessageBox.Show("整理数据需要重新排序所有客户数据以便节省ID资源\r\n\r\n此操作不可逆, 请做好数据备份\r\n\r\n确定要执行吗?", "危险操作", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
		{
			return;
		}
		主窗口.添加命令日志("<= @" + GetType().Name + " 开始执行命令, 过程中请勿关闭窗口");
		主窗口.主界面.BeginInvoke((MethodInvoker)delegate
		{
			主窗口.主界面.设置页面.Enabled = false;
			主窗口.主界面.下方控件页.Enabled = false;
		});
		Task.Run(delegate
		{
			游戏数据网关.整理数据(保存数据: true);
			主窗口.主界面.BeginInvoke((MethodInvoker)delegate
			{
				主窗口.主界面.设置页面.Enabled = true;
				主窗口.主界面.下方控件页.Enabled = true;
			});
		});
	}

	static 整理数据()
	{
	}
}
