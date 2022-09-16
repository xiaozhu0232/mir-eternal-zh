using System;
using System.Threading;
using System.Windows.Forms;

namespace 游戏登录器
{
	internal static class Program
	{
		private static Mutex myMutex;

		[STAThread]
		private static void Main()
		{
			myMutex = new Mutex(initiallyOwned: false, "CY_Launcher_Mutex", out var createdNew);
			if (createdNew)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(defaultValue: false);
				Application.Run(new 登录界面());
			}
			else
			{
				MessageBox.Show("登录器已经在运行中");
				Environment.Exit(0);
			}
		}
	}
}
