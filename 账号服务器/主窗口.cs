using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using 账号服务器.Properties;

namespace 账号服务器
{

	public partial class 主窗口 : Form
	{
		public static uint 新注册账号数;

		public static uint 生成门票总数;

		public static long 已接收字节数;

		public static long 已发送字节数;

		public static 主窗口 主界面;

		public static string 游戏区服 = "";

		public static string 数据目录 = ".\\Accounts";

		public static Dictionary<string, 账号数据> 账号数据;

		public static Dictionary<string, IPEndPoint> 区服数据;


		public 主窗口()
		{
			InitializeComponent();
			主界面 = this;
			本地监听端口.Value = Settings.Default.本地监听端口;
			门票发送端口.Value = Settings.Default.门票发送端口;
			if (!File.Exists(".\\server"))
			{
				日志文本框.AppendText("已为您创建配置文件，请自行修改内容。\r\n");
				File.WriteAllText(".\\server", "58.218.166.22,8701/雷霆");
				//Process.Start("notepad.exe", ".\\server");
			}
			/*
			  if (!Directory.Exists(数据目录))
			{
				日志文本框.AppendText("未找到账号配置文件夹, 请注意导入\r\n");
			}
			*/
		}

		public static void 更新已注册账号数()
		{
			主界面?.BeginInvoke((MethodInvoker)delegate
			{
				主界面.已注册账号.Text = $"已注册账号: {账号数据.Count}";
			});
		}

		public static void 更新新注册账号数()
		{
			更新已注册账号数();
			主界面?.BeginInvoke((MethodInvoker)delegate
			{
				主界面.新注册账号.Text = $"新注册账号: {新注册账号数}";
			});
		}

		public static void 更新已生成门票数()
		{
			主界面?.BeginInvoke((MethodInvoker)delegate
			{
				主界面.生成门票数.Text = $"生成门票数: {生成门票总数}";
			});
		}

		public static void 更新已接收字节数()
		{
			主界面?.BeginInvoke((MethodInvoker)delegate
			{
				主界面.已接收字节.Text = $"已接收字节: {已接收字节数}";
			});
		}

		public static void 更新已发送字节数()
		{
			主界面?.BeginInvoke((MethodInvoker)delegate
			{
				主界面.已发送字节.Text = $"已发送字节: {已发送字节数}";
			});
		}

		public static void 添加日志(string 内容)
		{
			主界面?.BeginInvoke((MethodInvoker)delegate
			{
				主界面.日志文本框.AppendText(内容 + "\r\n");
				主界面.日志文本框.ScrollToCaret();
			});
		}

		public static void 添加账号(账号数据 账号)
		{
			if (!账号数据.ContainsKey(账号.账号名字))
			{
				账号数据 账号2 = (账号数据[账号.账号名字] = 账号);
				保存账号(账号2);
			}
		}

		public static void 保存账号(账号数据 账号)
		{
			File.WriteAllText(数据目录 + "\\" + 账号.账号名字 + ".txt", 序列化类.序列化(账号));
		}

		private void 启动服务_Click(object sender, EventArgs e)
		{
			if (区服数据 == null || 区服数据.Count == 0)
			{
				加载配置按钮_Click(sender, e);
			}
			if (区服数据 == null || 区服数据.Count == 0)
			{
				添加日志("服务器配置为空, 启动失败");
				return;
			}
			if (账号数据 == null || 账号数据.Count == 0)
			{
				加载账号按钮_Click(sender, e);
			}
			if (网络通信.启动服务())
			{
				停止服务按钮.Enabled = true;
				Button button = 加载配置按钮;
				bool enabled = (加载账号按钮.Enabled = false);
				button.Enabled = enabled;
				启动服务按钮.Enabled = false;
				本地监听端口.Enabled = false;
				门票发送端口.Enabled = false;
				Settings.Default.本地监听端口 = (ushort)本地监听端口.Value;
				Settings.Default.门票发送端口 = (ushort)门票发送端口.Value;
				Settings.Default.Save();
			}
		}

		private void 停止服务_Click(object sender, EventArgs e)
		{
			网络通信.结束服务();
			停止服务按钮.Enabled = false;
			Button button = 加载配置按钮;
			bool enabled = (加载账号按钮.Enabled = true);
			button.Enabled = enabled;
			Button button2 = 启动服务按钮;
			NumericUpDown numericUpDown = 本地监听端口;
			bool flag3 = (门票发送端口.Enabled = true);
			enabled = (numericUpDown.Enabled = flag3);
			button2.Enabled = enabled;
		}

		private void 隐藏窗口_Click(object sender, FormClosingEventArgs e)
		{
			if (MessageBox.Show("确定关闭服务器?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
			{
				最小化到托盘.Visible = false;
				Environment.Exit(0);
				return;
			}
			最小化到托盘.Visible = true;
			Hide();
			if (e != null)
			{
				e.Cancel = true;
			}
			最小化到托盘.ShowBalloonTip(1000, "", "服务器已转为后台运行.", ToolTipIcon.Info);
		}

		private void 恢复窗口_Click(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				base.Visible = true;
				最小化到托盘.Visible = false;
			}
		}

		private void 恢复窗口_Click(object sender, EventArgs e)
		{
			base.Visible = true;
			最小化到托盘.Visible = false;
		}

		private void 结束进程_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("确定关闭服务器?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
			{
				网络通信.结束服务();
				最小化到托盘.Visible = false;
				Environment.Exit(0);
			}
		}

		private void 打开配置按钮_Click(object sender, EventArgs e)
		{
			if (!File.Exists(".\\server"))
			{
				添加日志("配置文件不存在, 已自动创建");
				File.WriteAllText(".\\server", "58.218.166.22,8701/雷霆");
			}
			添加日志("配置文件已打开,请按如下格式配置：58.218.166.22,8701/雷霆");
			Process.Start("notepad.exe", ".\\server");
		}

		private void 加载配置按钮_Click(object sender, EventArgs e)
		{
			if (!File.Exists(".\\server"))
			{
				return;
			}
			区服数据 = new Dictionary<string, IPEndPoint>();
			//游戏区服 = File.ReadAllText(".\\server", Encoding.Unicode).Trim('\r', '\n', ' ');
			游戏区服 = File.ReadAllText(".\\server", Encoding.UTF8).Trim('\r', '\n', ' ');
			string[] array = 游戏区服.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				string[] array2 = text.Split(new char[2] { ',', '/' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length != 3)
				{
					MessageBox.Show("server 配置错误, 解析失败的行: " + text);
					return;
				}
				区服数据.Add(array2[2], new IPEndPoint(IPAddress.Parse(array2[0]), Convert.ToInt32(array2[1])));
			}
			添加日志("服务器配置已加载, 当前配置: " + 游戏区服);
		}

		private void 查看账号按钮_Click(object sender, EventArgs e)
		{
			if (!Directory.Exists(数据目录))
			{
				添加日志("账号目录不存在, 已自动创建");
				Directory.CreateDirectory(数据目录);
			}
			else
			{
				Process.Start("explorer.exe", 数据目录);
			}
		}

		private void 加载账号按钮_Click(object sender, EventArgs e)
		{
			账号数据 = new Dictionary<string, 账号数据>();
			if (!Directory.Exists(数据目录))
			{
				添加日志("账号目录不存在, 已自动创建");
				Directory.CreateDirectory(数据目录);
				return;
			}
			object[] array = 序列化类.反序列化(数据目录, typeof(账号数据));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is 账号数据 账号数据2)
				{
					账号数据[账号数据2.账号名字] = 账号数据2;
				}
			}
			添加日志($"账号数据已加载, 当前账号数: {账号数据.Count}");
			已注册账号.Text = $"已注册账号: {账号数据.Count}";
		}

        private void 主窗口_Load(object sender, EventArgs e)
        {

        }
    }
}
