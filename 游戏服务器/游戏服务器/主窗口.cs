using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using 游戏服务器.Properties;
using 游戏服务器.地图类;
using 游戏服务器.模板类;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器;

public class 主窗口 : Form
{
	public static 主窗口 主界面;

	private static DataTable 角色数据表;

	private static DataTable 技能数据表;

	private static DataTable 装备数据表;

	private static DataTable 背包数据表;

	private static DataTable 仓库数据表;

	private static DataTable 地图数据表;

	private static DataTable 怪物数据表;

	private static DataTable 掉落数据表;

	private static DataTable 封禁数据表;

	private static Dictionary<角色数据, DataRow> 角色数据行;

	private static Dictionary<DataRow, 角色数据> 数据行角色;

	private static Dictionary<游戏地图, DataRow> 地图数据行;

	private static Dictionary<游戏怪物, DataRow> 怪物数据行;

	private static Dictionary<DataRow, 游戏怪物> 数据行怪物;

	private static Dictionary<string, DataRow> 封禁数据行;

	private static Dictionary<DataGridViewRow, DateTime> 公告数据表;

	private static Dictionary<角色数据, List<KeyValuePair<ushort, 技能数据>>> 角色技能表;

	private static Dictionary<角色数据, List<KeyValuePair<byte, 装备数据>>> 角色装备表;

	private static Dictionary<角色数据, List<KeyValuePair<byte, 物品数据>>> 角色背包表;

	private static Dictionary<角色数据, List<KeyValuePair<byte, 物品数据>>> 角色仓库表;

	private static Dictionary<游戏怪物, List<KeyValuePair<游戏物品, long>>> 怪物掉落表;

	private IContainer components;

	private RichTextBox 系统日志;

	private Label 帧数统计;

	private Button 保存系统日志;

	private Button 清空系统日志;

	private Label 发送统计;

	private Label 接收统计;

	private Label 已经上线统计;

	private Label 连接总数统计;

	private Label 已经登录统计;

	private Button 保存聊天日志;

	private Button 清空聊天日志;

	private TabControl 角色详情选项卡;

	private TabPage 角色数据_技能;

	private TabPage 角色数据_装备;

	private TabPage 角色数据_背包;

	private TabPage 角色数据_仓库;

	public TabControl 主选项卡;

	public DataGridView 角色浏览表;

	public DataGridView 技能浏览表;

	private DataGridView 装备浏览表;

	public DataGridView 背包浏览表;

	public DataGridView 仓库浏览表;

	private System.Windows.Forms.Timer 界面定时更新;

	private TabPage 系统日志页面;

	private TabPage 聊天日志页面;

	private RichTextBox 聊天日志;

	public DataGridView 地图浏览表;

	public DataGridView 怪物浏览表;

	private DataGridView 掉落浏览表;

	private GroupBox S_网络设置分组;

	private Label S_监听端口标签;

	private NumericUpDown S_客户连接端口;

	private Label S_接收端口标签;

	private NumericUpDown S_门票接收端口;

	private Label S_屏蔽时间标签;

	private NumericUpDown S_异常屏蔽时间;

	private GroupBox S_游戏设置分组;

	private Label S_特修折扣标签;

	private NumericUpDown S_装备特修折扣;

	private Label S_怪物爆率标签;

	private Label S_开放等级标签;

	private Label S_限定封包标签;

	private NumericUpDown S_封包限定数量;

	private Label S_掉线判定标签;

	private NumericUpDown S_掉线判定时间;

	private Label S_经验倍率标签;

	private Label S_收益等级标签;

	private NumericUpDown S_减收益等级差;

	private Label S_收益衰减标签;

	private NumericUpDown S_收益减少比率;

	private Label S_诱惑时长标签;

	private NumericUpDown S_怪物诱惑时长;

	private Label S_物品归属标签;

	private NumericUpDown S_物品归属时间;

	private Label S_物品清理标签;

	private NumericUpDown S_物品清理时间;

	private GroupBox S_游戏数据分组;

	private TextBox S_数据备份目录;

	private TextBox S_游戏数据目录;

	private Label S_备份目录标签;

	private Label S_数据目录标签;

	private Button S_合并客户数据;

	private TextBox S_合并数据目录;

	private Label S_合并目录标签;

	private Button S_浏览数据目录;

	private Button S_浏览合并目录;

	private Button S_浏览备份目录;

	private Button S_重载系统数据;

	private TextBox GM命令文本;

	private Label GM命令标签;

	public Button 启动按钮;

	public Button 停止按钮;

	private Button S_重载客户数据;

	public Button 保存按钮;

	private Label 对象统计;

	private Label S_注意事项标签2;

	private Label S_注意事项标签1;

	private Label S_注意事项标签5;

	private Label S_注意事项标签4;

	private Label S_注意事项标签3;

	private Label S_注意事项标签6;

	private Label S_注意事项标签8;

	private Label S_注意事项标签7;

	private TabPage 命令日志页面;

	private RichTextBox 命令日志;

	private Button 清空命令日志;

	private System.Windows.Forms.Timer 保存数据提醒;

	public TabPage 设置页面;

	public Panel 下方控件页;

	private ContextMenuStrip 角色右键菜单;

	private ToolStripMenuItem 右键菜单_复制角色名字;

	private ToolStripMenuItem 右键菜单_复制账号名字;

	private ToolStripMenuItem 右键菜单_复制网络地址;

	private ToolStripMenuItem 右键菜单_复制物理地址;

	public TabPage 封禁页面;

	private TabPage 公告页面;

	public TabPage 日志页面;

	public TabPage 地图页面;

	public TabPage 怪物页面;

	public TabPage 角色页面;

	private DataGridView 封禁浏览表;

	public NumericUpDown S_怪物额外爆率;

	public NumericUpDown S_怪物经验倍率;

	public NumericUpDown S_游戏开放等级;

	private Button 删除公告按钮;

	private Button 添加公告按钮;

	public System.Windows.Forms.Timer 定时发送公告;

	public DataGridView 公告浏览表;

	public Button 开始公告按钮;

	public Button 停止公告按钮;

	private DataGridViewTextBoxColumn 公告状态;

	private DataGridViewTextBoxColumn 公告间隔;

	private DataGridViewTextBoxColumn 公告次数;

	private DataGridViewTextBoxColumn 剩余次数;

	private DataGridViewTextBoxColumn 公告计时;

	private DataGridViewTextBoxColumn 公告内容;

	private Label S_新手扶持标签;

	public NumericUpDown S_新手扶持等级;

	public TabControl 日志选项卡;

	public static void 加载系统数据()
	{
		添加系统日志("正在加载系统数据...");
		地图数据表 = new DataTable("地图数据表");
		地图数据行 = new Dictionary<游戏地图, DataRow>();
		地图数据表.Columns.Add("地图编号", typeof(string));
		地图数据表.Columns.Add("地图名字", typeof(string));
		地图数据表.Columns.Add("限制等级", typeof(string));
		地图数据表.Columns.Add("玩家数量", typeof(string));
		地图数据表.Columns.Add("固定怪物总数", typeof(uint));
		地图数据表.Columns.Add("存活怪物总数", typeof(uint));
		地图数据表.Columns.Add("怪物复活次数", typeof(uint));
		地图数据表.Columns.Add("怪物掉落次数", typeof(long));
		地图数据表.Columns.Add("金币掉落总数", typeof(long));
		主界面?.地图浏览表.BeginInvoke((MethodInvoker)delegate
		{
			主界面.地图浏览表.DataSource = 地图数据表;
		});
		怪物数据表 = new DataTable("怪物数据表");
		怪物数据行 = new Dictionary<游戏怪物, DataRow>();
		数据行怪物 = new Dictionary<DataRow, 游戏怪物>();
		怪物数据表.Columns.Add("模板编号", typeof(string));
		怪物数据表.Columns.Add("怪物名字", typeof(string));
		怪物数据表.Columns.Add("怪物等级", typeof(string));
		怪物数据表.Columns.Add("怪物经验", typeof(string));
		怪物数据表.Columns.Add("怪物级别", typeof(string));
		怪物数据表.Columns.Add("移动间隔", typeof(string));
		怪物数据表.Columns.Add("漫游间隔", typeof(string));
		怪物数据表.Columns.Add("仇恨范围", typeof(string));
		怪物数据表.Columns.Add("仇恨时长", typeof(string));
		主界面?.怪物浏览表.BeginInvoke((MethodInvoker)delegate
		{
			主界面.怪物浏览表.DataSource = 怪物数据表;
		});
		掉落数据表 = new DataTable("掉落数据表");
		怪物掉落表 = new Dictionary<游戏怪物, List<KeyValuePair<游戏物品, long>>>();
		掉落数据表.Columns.Add("物品名字", typeof(string));
		掉落数据表.Columns.Add("掉落数量", typeof(string));
		主界面?.掉落浏览表.BeginInvoke((MethodInvoker)delegate
		{
			主界面.掉落浏览表.DataSource = 掉落数据表;
		});
		系统数据网关.加载数据();
		添加系统日志("系统数据加载完成");
	}

	public static void 加载客户数据()
	{
		添加系统日志("正在加载客户数据...");
		角色数据表 = new DataTable("角色数据表");
		技能数据表 = new DataTable("技能数据表");
		装备数据表 = new DataTable("装备数据表");
		背包数据表 = new DataTable("装备数据表");
		仓库数据表 = new DataTable("装备数据表");
		角色数据表 = new DataTable("角色数据表");
		角色数据行 = new Dictionary<角色数据, DataRow>();
		数据行角色 = new Dictionary<DataRow, 角色数据>();
		角色数据表.Columns.Add("角色名字", typeof(string));
		角色数据表.Columns.Add("角色封禁", typeof(string));
		角色数据表.Columns.Add("所属账号", typeof(string));
		角色数据表.Columns.Add("账号封禁", typeof(string));
		角色数据表.Columns.Add("冻结日期", typeof(string));
		角色数据表.Columns.Add("删除日期", typeof(string));
		角色数据表.Columns.Add("登录日期", typeof(string));
		角色数据表.Columns.Add("离线日期", typeof(string));
		角色数据表.Columns.Add("网络地址", typeof(string));
		角色数据表.Columns.Add("物理地址", typeof(string));
		角色数据表.Columns.Add("角色职业", typeof(string));
		角色数据表.Columns.Add("角色性别", typeof(string));
		角色数据表.Columns.Add("所属行会", typeof(string));
		角色数据表.Columns.Add("元宝数量", typeof(string));
		角色数据表.Columns.Add("消耗元宝", typeof(string));
		角色数据表.Columns.Add("金币数量", typeof(string));
		角色数据表.Columns.Add("转出金币", typeof(string));
		角色数据表.Columns.Add("背包大小", typeof(string));
		角色数据表.Columns.Add("仓库大小", typeof(string));
		角色数据表.Columns.Add("师门声望", typeof(string));
		角色数据表.Columns.Add("本期特权", typeof(string));
		角色数据表.Columns.Add("本期日期", typeof(string));
		角色数据表.Columns.Add("上期特权", typeof(string));
		角色数据表.Columns.Add("上期日期", typeof(string));
		角色数据表.Columns.Add("剩余特权", typeof(string));
		角色数据表.Columns.Add("当前等级", typeof(string));
		角色数据表.Columns.Add("当前经验", typeof(string));
		角色数据表.Columns.Add("双倍经验", typeof(string));
		角色数据表.Columns.Add("当前战力", typeof(string));
		角色数据表.Columns.Add("当前地图", typeof(string));
		角色数据表.Columns.Add("当前坐标", typeof(string));
		角色数据表.Columns.Add("当前PK值", typeof(string));
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.角色浏览表.DataSource = 角色数据表;
			for (int i = 0; i < 主界面.角色浏览表.Columns.Count; i++)
			{
				主界面.角色浏览表.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			}
		});
		角色技能表 = new Dictionary<角色数据, List<KeyValuePair<ushort, 技能数据>>>();
		技能数据表.Columns.Add("技能名字", typeof(string));
		技能数据表.Columns.Add("技能编号", typeof(string));
		技能数据表.Columns.Add("当前等级", typeof(string));
		技能数据表.Columns.Add("当前经验", typeof(string));
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.技能浏览表.DataSource = 技能数据表;
		});
		角色装备表 = new Dictionary<角色数据, List<KeyValuePair<byte, 装备数据>>>();
		装备数据表.Columns.Add("穿戴部位", typeof(string));
		装备数据表.Columns.Add("穿戴装备", typeof(string));
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.装备浏览表.DataSource = 装备数据表;
		});
		角色背包表 = new Dictionary<角色数据, List<KeyValuePair<byte, 物品数据>>>();
		背包数据表.Columns.Add("背包位置", typeof(string));
		背包数据表.Columns.Add("背包物品", typeof(string));
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.背包浏览表.DataSource = 背包数据表;
		});
		角色仓库表 = new Dictionary<角色数据, List<KeyValuePair<byte, 物品数据>>>();
		仓库数据表.Columns.Add("仓库位置", typeof(string));
		仓库数据表.Columns.Add("仓库物品", typeof(string));
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.仓库浏览表.DataSource = 仓库数据表;
		});
		封禁数据表 = new DataTable();
		封禁数据行 = new Dictionary<string, DataRow>();
		封禁数据表.Columns.Add("网络地址", typeof(string));
		封禁数据表.Columns.Add("物理地址", typeof(string));
		封禁数据表.Columns.Add("到期时间", typeof(string));
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.封禁浏览表.DataSource = 封禁数据表;
		});
		游戏数据网关.加载数据();
		添加系统日志("客户数据加载完成");
	}

	public static void 服务启动回调()
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.定时发送公告.Enabled = true;
			主界面.界面定时更新.Enabled = true;
			主界面.停止按钮.Enabled = true;
			主界面.启动按钮.Enabled = false;
			主界面.S_重载系统数据.Enabled = false;
			主界面.S_重载客户数据.Enabled = false;
			主界面.S_合并客户数据.Enabled = false;
			主界面.S_客户连接端口.Enabled = false;
			主界面.S_门票接收端口.Enabled = false;
			主界面.S_浏览数据目录.Enabled = false;
			主界面.S_浏览备份目录.Enabled = false;
			主界面.S_浏览合并目录.Enabled = false;
		});
	}

	public static void 服务停止回调()
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.定时发送公告.Enabled = true;
			主界面.设置页面.Enabled = true;
			主界面.启动按钮.Enabled = true;
			主界面.界面定时更新.Enabled = false;
			主界面.停止按钮.Enabled = false;
			foreach (KeyValuePair<DataGridViewRow, DateTime> item in 公告数据表)
			{
				item.Key.ReadOnly = false;
				item.Key.Cells["公告状态"].Value = "";
				item.Key.Cells["公告计时"].Value = "";
				item.Key.Cells["剩余次数"].Value = 0;
			}
			if (主界面.公告浏览表.SelectedRows.Count != 0)
			{
				主界面.开始公告按钮.Enabled = true;
				主界面.停止公告按钮.Enabled = false;
			}
			公告数据表.Clear();
		});
	}

	public static void 添加系统日志(string 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.系统日志.AppendText($"[{DateTime.Now:F}]: {内容}" + "\r\n");
			主界面.系统日志.ScrollToCaret();
			主界面.保存系统日志.Enabled = true;
			主界面.清空系统日志.Enabled = true;
		});
	}

	public static void 添加聊天日志(string 前缀, byte[] 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.聊天日志.AppendText($"[{DateTime.Now:F}]: {前缀 + Encoding.UTF8.GetString(内容).Trim(default(char))}" + "\r\n");
			主界面.聊天日志.ScrollToCaret();
			主界面.保存聊天日志.Enabled = true;
			主界面.清空聊天日志.Enabled = true;
		});
	}

	public static void 添加命令日志(string 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.命令日志.AppendText($"[{DateTime.Now:F}]: {内容}" + "\r\n");
			主界面.命令日志.ScrollToCaret();
			主界面.清空命令日志.Enabled = true;
		});
	}

	public static void 更新连接总数(uint 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.连接总数统计.Text = $"连接总数: {内容}";
		});
	}

	public static void 更新已经登录(uint 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.已经登录统计.Text = $"已经登录: {内容}";
		});
	}

	public static void 更新已经上线(uint 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.已经上线统计.Text = $"已经上线: {内容}";
		});
	}

	public static void 更新后台帧数(uint 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.帧数统计.Text = $"后台帧数: {内容}";
		});
	}

	public static void 更新接收字节(long 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.接收统计.Text = $"已经接收: {内容}";
		});
	}

	public static void 更新发送字节(long 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.发送统计.Text = $"已经发送: {内容}";
		});
	}

	public static void 更新对象统计(int 激活对象, int 次要对象, int 对象总数)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			主界面.对象统计.Text = $"对象统计: {激活对象} / {次要对象} / {对象总数}";
		});
	}

	public static void 添加数据显示(角色数据 数据)
	{
		if (!角色数据行.ContainsKey(数据))
		{
			角色数据行[数据] = 角色数据表.NewRow();
			角色数据表.Rows.Add(角色数据行[数据]);
		}
	}

	public static void 修改数据显示(角色数据 数据, string 表头文本, string 表格内容)
	{
		if (角色数据行.ContainsKey(数据))
		{
			角色数据行[数据][表头文本] = 表格内容;
		}
	}

	public static void 添加角色数据(角色数据 角色)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (!角色数据行.ContainsKey(角色))
			{
				DataRow dataRow = 角色数据表.NewRow();
				dataRow["角色名字"] = 角色;
				dataRow["所属账号"] = 角色.所属账号;
				dataRow["账号封禁"] = ((!(角色.所属账号.V.封禁日期.V != default(DateTime))) ? null : 角色.所属账号.V.封禁日期);
				dataRow["角色封禁"] = ((角色.封禁日期.V != default(DateTime)) ? 角色.封禁日期 : null);
				dataRow["冻结日期"] = ((角色.冻结日期.V != default(DateTime)) ? 角色.冻结日期 : null);
				dataRow["删除日期"] = ((角色.删除日期.V != default(DateTime)) ? 角色.删除日期 : null);
				dataRow["登录日期"] = ((角色.登录日期.V != default(DateTime)) ? 角色.登录日期 : null);
				dataRow["离线日期"] = ((角色.网络连接 == null) ? 角色.离线日期 : null);
				dataRow["网络地址"] = 角色.网络地址;
				dataRow["物理地址"] = 角色.物理地址;
				dataRow["角色职业"] = 角色.角色职业;
				dataRow["角色性别"] = 角色.角色性别;
				dataRow["所属行会"] = 角色.所属行会;
				dataRow["元宝数量"] = 角色.元宝数量;
				dataRow["消耗元宝"] = 角色.消耗元宝;
				dataRow["金币数量"] = 角色.金币数量;
				dataRow["转出金币"] = 角色.转出金币;
				dataRow["背包大小"] = 角色.背包大小;
				dataRow["仓库大小"] = 角色.仓库大小;
				dataRow["师门声望"] = 角色.师门声望;
				dataRow["本期特权"] = 角色.本期特权;
				dataRow["本期日期"] = 角色.本期日期;
				dataRow["上期特权"] = 角色.上期特权;
				dataRow["上期日期"] = 角色.上期日期;
				dataRow["剩余特权"] = 角色.剩余特权;
				dataRow["当前等级"] = 角色.当前等级;
				dataRow["当前经验"] = 角色.当前经验;
				dataRow["双倍经验"] = 角色.双倍经验;
				dataRow["当前战力"] = 角色.当前战力;
				dataRow["当前地图"] = ((!游戏地图.数据表.TryGetValue((byte)角色.当前地图.V, out var value)) ? ((object)角色.当前地图) : ((object)value.地图名字));
				dataRow["当前PK值"] = 角色.当前PK值;
				dataRow["当前坐标"] = $"{角色.当前坐标.V.X}, {角色.当前坐标.V.Y}";
				角色数据行[角色] = dataRow;
				数据行角色[dataRow] = 角色;
				角色数据表.Rows.Add(dataRow);
			}
		});
	}

	public static void 移除角色数据(角色数据 角色)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (角色数据行.TryGetValue(角色, out var value))
			{
				数据行角色.Remove(value);
				角色数据表.Rows.Remove(value);
				角色技能表.Remove(角色);
				角色背包表.Remove(角色);
				角色装备表.Remove(角色);
				角色仓库表.Remove(角色);
			}
		});
	}

	public static void 界面更新处理(object sender, EventArgs e)
	{
		技能数据表.Rows.Clear();
		装备数据表.Rows.Clear();
		背包数据表.Rows.Clear();
		仓库数据表.Rows.Clear();
		掉落数据表.Rows.Clear();
		if (主界面 == null)
		{
			return;
		}
		if (主界面.角色浏览表.Rows.Count > 0 && 主界面.角色浏览表.SelectedRows.Count > 0)
		{
			DataRow row = (主界面.角色浏览表.Rows[主界面.角色浏览表.SelectedRows[0].Index].DataBoundItem as DataRowView).Row;
			if (数据行角色.TryGetValue(row, out var value))
			{
				if (角色技能表.TryGetValue(value, out var value2))
				{
					foreach (KeyValuePair<ushort, 技能数据> item in value2)
					{
						DataRow dataRow = 技能数据表.NewRow();
						dataRow["技能名字"] = item.Value.铭文模板.技能名字;
						dataRow["技能编号"] = item.Value.技能编号;
						dataRow["当前等级"] = item.Value.技能等级;
						dataRow["当前经验"] = item.Value.技能经验;
						技能数据表.Rows.Add(dataRow);
					}
				}
				if (角色装备表.TryGetValue(value, out var value3))
				{
					foreach (KeyValuePair<byte, 装备数据> item2 in value3)
					{
						DataRow dataRow2 = 装备数据表.NewRow();
						dataRow2["穿戴部位"] = (装备穿戴部位)item2.Key;
						dataRow2["穿戴装备"] = item2.Value;
						装备数据表.Rows.Add(dataRow2);
					}
				}
				if (角色背包表.TryGetValue(value, out var value4))
				{
					foreach (KeyValuePair<byte, 物品数据> item3 in value4)
					{
						DataRow dataRow3 = 背包数据表.NewRow();
						dataRow3["背包位置"] = item3.Key;
						dataRow3["背包物品"] = item3.Value;
						背包数据表.Rows.Add(dataRow3);
					}
				}
				if (角色仓库表.TryGetValue(value, out var value5))
				{
					foreach (KeyValuePair<byte, 物品数据> item4 in value5)
					{
						DataRow dataRow4 = 仓库数据表.NewRow();
						dataRow4["仓库位置"] = item4.Key;
						dataRow4["仓库物品"] = item4.Value;
						仓库数据表.Rows.Add(dataRow4);
					}
				}
			}
		}
		if (主界面.怪物浏览表.Rows.Count <= 0 || 主界面.怪物浏览表.SelectedRows.Count <= 0)
		{
			return;
		}
		DataRow row2 = (主界面.怪物浏览表.Rows[主界面.怪物浏览表.SelectedRows[0].Index].DataBoundItem as DataRowView).Row;
		if (!数据行怪物.TryGetValue(row2, out var value6) || !怪物掉落表.TryGetValue(value6, out var value7))
		{
			return;
		}
		foreach (KeyValuePair<游戏物品, long> item5 in value7)
		{
			DataRow dataRow5 = 掉落数据表.NewRow();
			dataRow5["物品名字"] = item5.Key.物品名字;
			dataRow5["掉落数量"] = item5.Value;
			掉落数据表.Rows.Add(dataRow5);
		}
	}

	public static void 更新角色数据(角色数据 角色, string 表头, object 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (角色数据行.TryGetValue(角色, out var value))
			{
				value[表头] = 内容;
			}
		});
	}

	public static void 更新角色技能(角色数据 角色, List<KeyValuePair<ushort, 技能数据>> 技能)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			角色技能表[角色] = 技能;
		});
	}

	public static void 更新角色装备(角色数据 角色, List<KeyValuePair<byte, 装备数据>> 装备)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			角色装备表[角色] = 装备;
		});
	}

	public static void 更新角色背包(角色数据 角色, List<KeyValuePair<byte, 物品数据>> 物品)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			角色背包表[角色] = 物品;
		});
	}

	public static void 更新角色仓库(角色数据 角色, List<KeyValuePair<byte, 物品数据>> 物品)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			角色仓库表[角色] = 物品;
		});
	}

	public static void 添加地图数据(地图实例 地图)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (!地图数据行.ContainsKey(地图.地图模板))
			{
				DataRow dataRow = 地图数据表.NewRow();
				dataRow["地图编号"] = 地图.地图编号;
				dataRow["地图名字"] = 地图.地图模板;
				dataRow["限制等级"] = 地图.限制等级;
				dataRow["玩家数量"] = 地图.玩家列表.Count;
				dataRow["固定怪物总数"] = 地图.固定怪物总数;
				dataRow["存活怪物总数"] = 地图.存活怪物总数;
				dataRow["怪物复活次数"] = 地图.怪物复活次数;
				dataRow["怪物掉落次数"] = 地图.怪物掉落次数;
				dataRow["金币掉落总数"] = 地图.金币掉落总数;
				地图数据行[地图.地图模板] = dataRow;
				地图数据表.Rows.Add(dataRow);
			}
		});
	}

	public static void 更新地图数据(地图实例 地图, string 表头, object 内容)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (地图数据行.TryGetValue(地图.地图模板, out var value))
			{
				switch (表头)
				{
				case "金币掉落总数":
				case "怪物掉落次数":
					value[表头] = Convert.ToInt64(value[表头]) + (int)内容;
					break;
				default:
					value[表头] = 内容;
					break;
				case "存活怪物总数":
				case "怪物复活次数":
					value[表头] = Convert.ToUInt32(value[表头]) + (int)内容;
					break;
				}
			}
		});
	}

	public static void 添加怪物数据(游戏怪物 怪物)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (!怪物数据行.ContainsKey(怪物))
			{
				DataRow dataRow = 怪物数据表.NewRow();
				dataRow["模板编号"] = 怪物.怪物编号;
				dataRow["怪物名字"] = 怪物.怪物名字;
				dataRow["怪物等级"] = 怪物.怪物等级;
				dataRow["怪物级别"] = 怪物.怪物级别;
				dataRow["怪物经验"] = 怪物.怪物提供经验;
				dataRow["移动间隔"] = 怪物.怪物移动间隔;
				dataRow["仇恨范围"] = 怪物.怪物仇恨范围;
				dataRow["仇恨时长"] = 怪物.怪物仇恨时间;
				怪物数据行[怪物] = dataRow;
				数据行怪物[dataRow] = 怪物;
				怪物数据表.Rows.Add(dataRow);
			}
		});
	}

	public static void 更新掉落统计(游戏怪物 怪物, List<KeyValuePair<游戏物品, long>> 物品)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			怪物掉落表[怪物] = 物品;
		});
	}

	public static void 添加封禁数据(string 地址, object 时间, bool 网络地址 = true)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (!封禁数据行.ContainsKey(地址))
			{
				DataRow dataRow = 封禁数据表.NewRow();
				dataRow["网络地址"] = (网络地址 ? 地址 : null);
				dataRow["物理地址"] = (网络地址 ? null : 地址);
				dataRow["到期时间"] = 时间;
				封禁数据行[地址] = dataRow;
				封禁数据表.Rows.Add(dataRow);
			}
		});
	}

	public static void 更新封禁数据(string 地址, object 时间, bool 网络地址 = true)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (封禁数据行.TryGetValue(地址, out var value))
			{
				if (!网络地址)
				{
					value["物理地址"] = 时间;
				}
				else
				{
					value["网络地址"] = 时间;
				}
			}
		});
	}

	public static void 移除封禁数据(string 地址)
	{
		主界面?.BeginInvoke((MethodInvoker)delegate
		{
			if (封禁数据行.TryGetValue(地址, out var value))
			{
				封禁数据行.Remove(地址);
				封禁数据表.Rows.Remove(value);
			}
		});
	}

	public 主窗口()
	{
		InitializeComponent();
		主界面 = this;
		string 系统公告内容 = Settings.Default.系统公告内容;
		公告数据表 = new Dictionary<DataGridViewRow, DateTime>();
		string[] array = 系统公告内容.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('\t');
			int index = 公告浏览表.Rows.Add();
			公告浏览表.Rows[index].Cells["公告间隔"].Value = array2[0];
			公告浏览表.Rows[index].Cells["公告次数"].Value = array2[1];
			公告浏览表.Rows[index].Cells["公告内容"].Value = array2[2];
		}
		角色浏览表.ColumnHeadersDefaultCellStyle.Font = (地图浏览表.ColumnHeadersDefaultCellStyle.Font = (怪物浏览表.ColumnHeadersDefaultCellStyle.Font = (掉落浏览表.ColumnHeadersDefaultCellStyle.Font = (封禁浏览表.ColumnHeadersDefaultCellStyle.Font = (角色浏览表.DefaultCellStyle.Font = (地图浏览表.DefaultCellStyle.Font = (怪物浏览表.DefaultCellStyle.Font = (封禁浏览表.DefaultCellStyle.Font = (掉落浏览表.DefaultCellStyle.Font = new Font("宋体", 9f))))))))));
		S_游戏数据目录.Text = (自定义类.游戏数据目录 = Settings.Default.游戏数据目录);
		S_数据备份目录.Text = (自定义类.数据备份目录 = Settings.Default.数据备份目录);
		S_客户连接端口.Value = (自定义类.客户连接端口 = Settings.Default.客户连接端口);
		S_门票接收端口.Value = (自定义类.门票接收端口 = Settings.Default.门票接收端口);
		S_封包限定数量.Value = (自定义类.封包限定数量 = Settings.Default.封包限定数量);
		S_异常屏蔽时间.Value = (自定义类.异常屏蔽时间 = Settings.Default.异常屏蔽时间);
		S_掉线判定时间.Value = (自定义类.掉线判定时间 = Settings.Default.掉线判定时间);
		S_游戏开放等级.Value = (自定义类.游戏开放等级 = Settings.Default.游戏开放等级);
		S_新手扶持等级.Value = (自定义类.新手扶持等级 = Settings.Default.新手扶持等级);
		S_装备特修折扣.Value = (自定义类.装备特修折扣 = Settings.Default.装备特修折扣);
		S_怪物额外爆率.Value = (自定义类.怪物额外爆率 = Settings.Default.怪物额外爆率);
		S_怪物经验倍率.Value = (自定义类.怪物经验倍率 = Settings.Default.怪物经验倍率);
		S_减收益等级差.Value = (自定义类.减收益等级差 = Settings.Default.减收益等级差);
		S_收益减少比率.Value = (自定义类.收益减少比率 = Settings.Default.收益减少比率);
		S_怪物诱惑时长.Value = (自定义类.怪物诱惑时长 = Settings.Default.怪物诱惑时长);
		S_物品归属时间.Value = (自定义类.物品归属时间 = Settings.Default.物品归属时间);
		S_物品清理时间.Value = (自定义类.物品清理时间 = Settings.Default.物品清理时间);
		Task.Run(delegate
		{
			Thread.Sleep(100);
			BeginInvoke((MethodInvoker)delegate
			{
				设置页面.Enabled = true;
				下方控件页.Enabled = false;
			});
			加载系统数据();
			加载客户数据();
			BeginInvoke((MethodInvoker)delegate
			{
				界面定时更新.Tick += 界面更新处理;
				角色浏览表.SelectionChanged += 界面更新处理;
				怪物浏览表.SelectionChanged += 界面更新处理;
				设置页面.Enabled = true;
				下方控件页.Enabled = true;
			});
		});
	}

	private void 保存数据库_Click(object sender, EventArgs e)
	{
		Task.Run(delegate
		{
			添加系统日志("正在保存客户数据...");
			游戏数据网关.保存数据();
			游戏数据网关.导出数据();
			添加系统日志("客户数据已经保存");
		});
	}

	private void 启动服务器_Click(object sender, EventArgs e)
	{
		服务启动回调();
		主程.启动服务();
		Settings.Default.Save();
		地图数据表 = new DataTable("地图数据表");
		地图数据行 = new Dictionary<游戏地图, DataRow>();
		地图数据表.Columns.Add("地图编号", typeof(string));
		地图数据表.Columns.Add("地图名字", typeof(string));
		地图数据表.Columns.Add("限制等级", typeof(string));
		地图数据表.Columns.Add("玩家数量", typeof(string));
		地图数据表.Columns.Add("固定怪物总数", typeof(string));
		地图数据表.Columns.Add("存活怪物总数", typeof(string));
		地图数据表.Columns.Add("怪物复活次数", typeof(string));
		地图数据表.Columns.Add("怪物掉落次数", typeof(string));
		地图数据表.Columns.Add("金币掉落总数", typeof(string));
		主界面.地图浏览表.DataSource = 地图数据表;
		怪物数据表 = new DataTable("怪物数据表");
		怪物数据行 = new Dictionary<游戏怪物, DataRow>();
		数据行怪物 = new Dictionary<DataRow, 游戏怪物>();
		怪物数据表.Columns.Add("模板编号", typeof(string));
		怪物数据表.Columns.Add("怪物名字", typeof(string));
		怪物数据表.Columns.Add("怪物等级", typeof(string));
		怪物数据表.Columns.Add("怪物经验", typeof(string));
		怪物数据表.Columns.Add("怪物级别", typeof(string));
		怪物数据表.Columns.Add("移动间隔", typeof(string));
		怪物数据表.Columns.Add("漫游间隔", typeof(string));
		怪物数据表.Columns.Add("仇恨范围", typeof(string));
		怪物数据表.Columns.Add("仇恨时长", typeof(string));
		主界面.怪物浏览表.DataSource = 怪物数据表;
		掉落数据表 = new DataTable("掉落数据表");
		怪物掉落表 = new Dictionary<游戏怪物, List<KeyValuePair<游戏物品, long>>>();
		掉落数据表.Columns.Add("物品名字", typeof(string));
		掉落数据表.Columns.Add("掉落数量", typeof(string));
		主界面.掉落浏览表.DataSource = 掉落数据表;
		主选项卡.SelectedIndex = 0;
	}

	private void 停止服务器_Click(object sender, EventArgs e)
	{
		if (MessageBox.Show("确定停止服务器?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
		{
			主程.停止服务();
			启动按钮.Enabled = true;
			停止按钮.Enabled = false;
		}
	}

	private void 关闭主界面_Click(object sender, FormClosingEventArgs e)
	{
		if (MessageBox.Show("确定关闭服务器?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
		{
			e.Cancel = true;
			return;
		}
		while (true)
		{
			Thread 主线程 = 主程.主线程;
			if (主线程 == null || !主线程.IsAlive)
			{
				break;
			}
			主程.停止服务();
			Thread.Sleep(1);
		}
		if (游戏数据网关.已经修改 && MessageBox.Show("客户数据已经修改但尚未保存, 需要保存数据吗?", "保存数据", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
		{
			游戏数据网关.保存数据();
			游戏数据网关.导出数据();
		}
	}

	private void 保存数据提醒_Tick(object sender, EventArgs e)
	{
		if (保存按钮.Enabled && 游戏数据网关.已经修改)
		{
			if (!(保存按钮.BackColor == Color.LightSteelBlue))
			{
				保存按钮.BackColor = Color.LightSteelBlue;
			}
			else
			{
				保存按钮.BackColor = Color.PaleVioletRed;
			}
		}
	}

	private void 清空系统日志_Click(object sender, EventArgs e)
	{
		系统日志.Clear();
		保存系统日志.Enabled = false;
		清空系统日志.Enabled = false;
	}

	private void 清空聊天日志_Click(object sender, EventArgs e)
	{
		聊天日志.Clear();
		保存聊天日志.Enabled = false;
		清空聊天日志.Enabled = false;
	}

	private void 清空命令日志_Click(object sender, EventArgs e)
	{
		命令日志.Clear();
		清空命令日志.Enabled = false;
	}

	private void 保存系统日志_Click(object sender, EventArgs e)
	{
		if (系统日志.Text != null && !(系统日志.Text == ""))
		{
			if (!Directory.Exists(".\\Log\\Sys"))
			{
				Directory.CreateDirectory(".\\Log\\Sys");
			}
			File.WriteAllText($".\\Log\\Sys\\{DateTime.Now:yyyy-MM-dd--HH-mm-ss}.txt", 系统日志.Text.Replace("\n", "\r\n"));
			添加系统日志("系统日志已成功保存");
		}
	}

	private void 保存聊天日志_Click(object sender, EventArgs e)
	{
		if (聊天日志.Text != null && !(聊天日志.Text == ""))
		{
			if (!Directory.Exists(".\\Log\\Chat"))
			{
				Directory.CreateDirectory(".\\Log\\Chat");
			}
			File.WriteAllText($".\\Log\\Chat\\{DateTime.Now:yyyy-MM-dd--HH-mm-ss}.txt", 系统日志.Text);
			添加系统日志("系统日志已成功保存");
		}
	}

	private void 重载系统数据_Click(object sender, EventArgs e)
	{
		设置页面.Enabled = false;
		下方控件页.Enabled = false;
		Task.Run(delegate
		{
			加载系统数据();
			BeginInvoke((MethodInvoker)delegate
			{
				设置页面.Enabled = true;
				下方控件页.Enabled = true;
			});
		});
	}

	private void 重载客户数据_Click(object sender, EventArgs e)
	{
		设置页面.Enabled = false;
		下方控件页.Enabled = false;
		Task.Run(delegate
		{
			加载客户数据();
			BeginInvoke((MethodInvoker)delegate
			{
				设置页面.Enabled = true;
				下方控件页.Enabled = true;
			});
		});
	}

	private void 选择数据目录_Click(object sender, EventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			Description = "请选择文件夹"
		};
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			if (sender == S_浏览数据目录)
			{
				Settings @default = Settings.Default;
				string text = (S_游戏数据目录.Text = folderBrowserDialog.SelectedPath);
				string text2 = text;
				text = (@default.游戏数据目录 = text2);
				自定义类.游戏数据目录 = text;
				Settings.Default.Save();
			}
			else if (sender == S_浏览备份目录)
			{
				Settings default2 = Settings.Default;
				string text = (S_数据备份目录.Text = folderBrowserDialog.SelectedPath);
				string text4 = text;
				text = (default2.数据备份目录 = text4);
				自定义类.数据备份目录 = text;
				Settings.Default.Save();
			}
			else if (sender == S_浏览合并目录)
			{
				S_合并数据目录.Text = folderBrowserDialog.SelectedPath;
			}
		}
	}

	private void 更改设置数值_Value(object sender, EventArgs e)
	{
		if (sender is NumericUpDown numericUpDown)
		{
			switch (numericUpDown.Name)
			{
				case "S_收益减少比率":
				{
					自定义类.收益减少比率 = (Settings.Default.收益减少比率 = numericUpDown.Value);
					break;
				}
				case "S_掉线判定时间":
				{
					自定义类.掉线判定时间 = (Settings.Default.掉线判定时间 = (ushort)numericUpDown.Value);
					break;
				}
				case "S_游戏开放等级":
				{
					自定义类.游戏开放等级 = (Settings.Default.游戏开放等级 = (byte)numericUpDown.Value);
					break;
				}
				case "S_怪物诱惑时长":
				{
					自定义类.怪物诱惑时长 = (Settings.Default.怪物诱惑时长 = (ushort)numericUpDown.Value);
					break;
				}
				case "S_怪物经验倍率":
				{
					自定义类.怪物经验倍率 = (Settings.Default.怪物经验倍率 = numericUpDown.Value);
					break;
				}
				case "S_门票接收端口":
				{
					自定义类.门票接收端口 = (Settings.Default.门票接收端口 = (ushort)numericUpDown.Value);
					break;
				}
				case "S_异常屏蔽时间":
				{
					自定义类.异常屏蔽时间 = (Settings.Default.异常屏蔽时间 = (ushort)numericUpDown.Value);
					break;
				}
				case "S_减收益等级差":
				{
					自定义类.减收益等级差 = (Settings.Default.减收益等级差 = (byte)numericUpDown.Value);
					break;
				}
				case "S_怪物额外爆率":
				{
					自定义类.怪物额外爆率 = (Settings.Default.怪物额外爆率 = numericUpDown.Value);
					break;
				}
				case "S_物品归属时间":
				{
					自定义类.物品归属时间 = (Settings.Default.物品归属时间 = (byte)numericUpDown.Value);
					break;
				}
				case "S_新手扶持等级":
				{
					自定义类.新手扶持等级 = (Settings.Default.新手扶持等级 = (byte)numericUpDown.Value);
					break;
				}
				case "S_装备特修折扣":
				{
					自定义类.装备特修折扣 = (Settings.Default.装备特修折扣 = numericUpDown.Value);
					break;
				}
				case "S_物品清理时间":
				{
					自定义类.物品清理时间 = (Settings.Default.物品清理时间 = (byte)numericUpDown.Value);
					break;
				}
				case "S_封包限定数量":
				{
					自定义类.封包限定数量 = (Settings.Default.封包限定数量 = (ushort)numericUpDown.Value);
					break;
				}
				case "S_客户连接端口":
				{
					自定义类.客户连接端口 = (Settings.Default.客户连接端口 = (ushort)numericUpDown.Value);
					break;
				}
				default:
				{
					MessageBox.Show("未知变量! " + numericUpDown.Name);
					break;
				}
			}
			Settings.Default.Save();
		}
	}

	private void 执行GM命令行_Press(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != Convert.ToChar(13) || GM命令文本.Text.Length <= 0)
		{
			return;
		}
		主选项卡.SelectedIndex = 0;
		日志选项卡.SelectedIndex = 2;
		添加命令日志("=> " + GM命令文本.Text);
		if (GM命令文本.Text[0] == '@')
		{
			GM命令 命令;
			if (GM命令文本.Text.Trim('@', ' ').Length == 0)
			{
				添加命令日志("<= 命令解析错误, GM命令不能为空. 输入 '@查看命令' 获取所有受支持的命令格式");
			}
			else if (GM命令.解析命令(GM命令文本.Text, out 命令))
			{
				if (命令.执行方式 != 0)
				{
					if (命令.执行方式 != 执行方式.优先后台执行)
					{
						if (命令.执行方式 != 执行方式.只能后台执行)
						{
							if (命令.执行方式 == 执行方式.只能空闲执行)
							{
								if (!主程.已经启动 && (主程.主线程 == null || !主程.主线程.IsAlive))
								{
									命令.执行命令();
								}
								else
								{
									添加命令日志("<= 命令执行失败, 当前命令只能在服务器未运行时执行, 请先关闭服务器");
								}
							}
						}
						else if (!主程.已经启动)
						{
							添加命令日志("<= 命令执行失败, 当前命令只能在服务器运行时执行, 请先启动服务器");
						}
						else
						{
							主程.外部命令.Enqueue(命令);
						}
					}
					else if (!主程.已经启动)
					{
						命令.执行命令();
					}
					else
					{
						主程.外部命令.Enqueue(命令);
					}
				}
				else
				{
					命令.执行命令();
				}
				e.Handled = true;
			}
		}
		else
		{
			添加命令日志("<= 命令解析错误, GM命令必须以 '@' 开头. 输入 '@查看命令' 获取所有受支持的命令格式");
		}
		GM命令文本.Clear();
	}

	private void 合并客户数据_Click(object sender, EventArgs e)
	{
		if (!主程.已经启动)
		{
			Dictionary<Type, 数据表基类> 数据类型表 = 游戏数据网关.数据类型表;
			if (数据类型表 == null || 数据类型表.Count == 0)
			{
				MessageBox.Show("需要先加载当前客户数据后才能与指定客户数据合并");
			}
			else if (Directory.Exists(S_合并数据目录.Text))
			{
				if (File.Exists(S_合并数据目录.Text + "\\Data.db"))
				{
					if (MessageBox.Show("即将执行数据合并操作\r\n\r\n此操作不可逆, 请做好数据备份\r\n\r\n确定要执行吗?", "危险操作", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
					{
						游戏数据网关.合并数据(S_合并数据目录.Text + "\\Data.db");
					}
				}
				else
				{
					MessageBox.Show("选择的目录中没有找到 Data.db 文件");
				}
			}
			else
			{
				MessageBox.Show("请选择有效的 Data.db 文件目录");
			}
		}
		else
		{
			MessageBox.Show("合并数据只能在服务器未运行时执行");
		}
	}

	private void 角色右键菜单_Click(object sender, EventArgs e)
	{
		if (sender is ToolStripMenuItem toolStripMenuItem && 主界面.角色浏览表.Rows.Count > 0 && 主界面.角色浏览表.SelectedRows.Count > 0)
		{
			DataRow row = (主界面.角色浏览表.Rows[主界面.角色浏览表.SelectedRows[0].Index].DataBoundItem as DataRowView).Row;
			if (toolStripMenuItem.Name == "右键菜单_复制账号名字")
			{
				Clipboard.SetDataObject(row["所属账号"]);
			}
			if (toolStripMenuItem.Name == "右键菜单_复制角色名字")
			{
				Clipboard.SetDataObject(row["角色名字"]);
			}
			if (toolStripMenuItem.Name == "右键菜单_复制网络地址")
			{
				Clipboard.SetDataObject(row["网络地址"]);
			}
			if (toolStripMenuItem.Name == "右键菜单_复制物理地址")
			{
				Clipboard.SetDataObject(row["物理地址"]);
			}
		}
	}

	private void 添加公告按钮_Click(object sender, EventArgs e)
	{
		int index = 公告浏览表.Rows.Add();
		公告浏览表.Rows[index].Cells["公告间隔"].Value = 5;
		公告浏览表.Rows[index].Cells["公告次数"].Value = 1;
		公告浏览表.Rows[index].Cells["公告内容"].Value = "请输入公告内容";
		string text = null;
		string text2;
		string text3;
		object obj3;
		string text4;
		for (int i = 0; i < 公告浏览表.Rows.Count; text4 = (string)obj3, text = text + text2 + "\t" + text3 + "\t" + text4 + "\r\n", i++)
		{
			object value = 公告浏览表.Rows[i].Cells["公告间隔"].Value;
			object obj;
			if (value == null)
			{
				obj = null;
			}
			else
			{
				obj = value.ToString();
				if (obj != null)
				{
					goto IL_00e0;
				}
			}
			obj = "";
			goto IL_00e0;
			IL_00e0:
			text2 = (string)obj;
			object value2 = 公告浏览表.Rows[i].Cells["公告次数"].Value;
			object obj2;
			if (value2 == null)
			{
				obj2 = null;
			}
			else
			{
				obj2 = value2.ToString();
				if (obj2 != null)
				{
					goto IL_012d;
				}
			}
			obj2 = "";
			goto IL_012d;
			IL_012d:
			text3 = (string)obj2;
			object value3 = 公告浏览表.Rows[i].Cells["公告内容"].Value;
			if (value3 != null)
			{
				obj3 = value3.ToString();
				if (obj3 != null)
				{
					continue;
				}
			}
			else
			{
				obj3 = null;
			}
			obj3 = "";
		}
		Settings.Default.系统公告内容 = text;
		Settings.Default.Save();
	}

	private void 删除公告按钮_Click(object sender, EventArgs e)
	{
		if (公告浏览表.Rows.Count == 0 || 公告浏览表.SelectedRows.Count == 0)
		{
			return;
		}
		DataGridViewRow key = 公告浏览表.Rows[公告浏览表.SelectedRows[0].Index];
		公告数据表.Remove(key);
		公告浏览表.Rows.RemoveAt(公告浏览表.SelectedRows[0].Index);
		string text = null;
		string text2;
		string text3;
		object obj3;
		string text4;
		for (int i = 0; i < 公告浏览表.Rows.Count; text4 = (string)obj3, text = text + text2 + "\t" + text3 + "\t" + text4 + "\r\n", i++)
		{
			object value = 公告浏览表.Rows[i].Cells["公告间隔"].Value;
			object obj;
			if (value == null)
			{
				obj = null;
			}
			else
			{
				obj = value.ToString();
				if (obj != null)
				{
					goto IL_00cd;
				}
			}
			obj = "";
			goto IL_00cd;
			IL_00cd:
			text2 = (string)obj;
			object value2 = 公告浏览表.Rows[i].Cells["公告次数"].Value;
			object obj2;
			if (value2 != null)
			{
				obj2 = value2.ToString();
				if (obj2 != null)
				{
					goto IL_011a;
				}
			}
			else
			{
				obj2 = null;
			}
			obj2 = "";
			goto IL_011a;
			IL_011a:
			text3 = (string)obj2;
			object value3 = 公告浏览表.Rows[i].Cells["公告内容"].Value;
			if (value3 == null)
			{
				obj3 = null;
			}
			else
			{
				obj3 = value3.ToString();
				if (obj3 != null)
				{
					continue;
				}
			}
			obj3 = "";
		}
		Settings.Default.系统公告内容 = text;
		Settings.Default.Save();
	}

	private void 开始公告按钮_Click(object sender, EventArgs e)
	{
		if (!主程.已经启动 || !停止按钮.Enabled)
		{
			Task.Run(delegate
			{
				MessageBox.Show("服务器未启动, 请先开启服务器");
			});
		}
		else
		{
			if (公告浏览表.Rows.Count == 0 || 公告浏览表.SelectedRows.Count == 0)
			{
				return;
			}
			DataGridViewRow dataGridViewRow = 公告浏览表.Rows[公告浏览表.SelectedRows[0].Index];
			if (int.TryParse(dataGridViewRow.Cells["公告间隔"].Value.ToString(), out var result) && result > 0)
			{
				if (!int.TryParse(dataGridViewRow.Cells["公告次数"].Value.ToString(), out var result2) || result2 <= 0)
				{
					Task.Run(delegate
					{
						MessageBox.Show("系统公告未能开启, 公告次数必须为大于0的整数");
					});
					return;
				}
				if (dataGridViewRow.Cells["公告内容"].Value == null || dataGridViewRow.Cells["公告内容"].Value.ToString().Length <= 0)
				{
					Task.Run(delegate
					{
						MessageBox.Show("系统公告未能开启, 公告内容不能为空");
					});
					return;
				}
				dataGridViewRow.ReadOnly = true;
				dataGridViewRow.Cells["公告状态"].Value = "√";
				dataGridViewRow.Cells["剩余次数"].Value = dataGridViewRow.Cells["公告次数"].Value;
				公告数据表.Add(dataGridViewRow, DateTime.Now);
				开始公告按钮.Enabled = false;
				停止公告按钮.Enabled = true;
			}
			else
			{
				Task.Run(delegate
				{
					MessageBox.Show("系统公告未能开启, 公告间隔必须为大于0的整数");
				});
			}
		}
	}

	private void 停止公告按钮_Click(object sender, EventArgs e)
	{
		if (公告浏览表.Rows.Count != 0 && 公告浏览表.SelectedRows.Count != 0)
		{
			DataGridViewRow dataGridViewRow = 公告浏览表.Rows[公告浏览表.SelectedRows[0].Index];
			公告数据表.Remove(dataGridViewRow);
			dataGridViewRow.ReadOnly = false;
			dataGridViewRow.Cells["公告状态"].Value = "";
			dataGridViewRow.Cells["公告计时"].Value = "";
			dataGridViewRow.Cells["剩余次数"].Value = 0;
			开始公告按钮.Enabled = true;
			停止公告按钮.Enabled = false;
		}
	}

	private void 定时发送公告_Tick(object sender, EventArgs e)
	{
		if (!主程.已经启动 || 公告数据表.Count == 0)
		{
			return;
		}
		DateTime now = DateTime.Now;
		foreach (KeyValuePair<DataGridViewRow, DateTime> item in 公告数据表.ToList())
		{
			item.Key.Cells["公告计时"].Value = (item.Value - now).ToString("hh\\:mm\\:ss");
			if (!(now > item.Value))
			{
				continue;
			}
			网络服务网关.发送公告(item.Key.Cells["公告内容"].Value.ToString(), 滚动播报: true);
			公告数据表[item.Key] = now.AddMinutes(Convert.ToInt32(item.Key.Cells["公告间隔"].Value));
			int num = Convert.ToInt32(item.Key.Cells["剩余次数"].Value) - 1;
			item.Key.Cells["剩余次数"].Value = num;
			if (num <= 0)
			{
				公告数据表.Remove(item.Key);
				item.Key.ReadOnly = false;
				item.Key.Cells["公告状态"].Value = "";
				if (item.Key.Selected)
				{
					开始公告按钮.Enabled = true;
					停止公告按钮.Enabled = false;
				}
			}
		}
	}

	private void 公告浏览表_SelectionChanged(object sender, EventArgs e)
	{
		if (公告浏览表.Rows.Count != 0 && 公告浏览表.SelectedRows.Count != 0)
		{
			DataGridViewRow key = 公告浏览表.Rows[公告浏览表.SelectedRows[0].Index];
			if (!公告数据表.ContainsKey(key))
			{
				开始公告按钮.Enabled = true;
				停止公告按钮.Enabled = false;
			}
			else
			{
				开始公告按钮.Enabled = false;
				停止公告按钮.Enabled = true;
			}
		}
		else
		{
			停止公告按钮.Enabled = false;
			开始公告按钮.Enabled = false;
		}
	}

	private void 公告浏览表_CellEndEdit(object sender, DataGridViewCellEventArgs e)
	{
		string text = null;
		string text2;
		string text3;
		object obj3;
		string text4;
		for (int i = 0; i < 公告浏览表.Rows.Count; text4 = (string)obj3, text = text + text2 + "\t" + text3 + "\t" + text4 + "\r\n", i++)
		{
			object value = 公告浏览表.Rows[i].Cells["公告间隔"].Value;
			object obj;
			if (value != null)
			{
				obj = value.ToString();
				if (obj != null)
				{
					goto IL_004f;
				}
			}
			else
			{
				obj = null;
			}
			obj = "";
			goto IL_004f;
			IL_004f:
			text2 = (string)obj;
			object value2 = 公告浏览表.Rows[i].Cells["公告次数"].Value;
			object obj2;
			if (value2 == null)
			{
				obj2 = null;
			}
			else
			{
				obj2 = value2.ToString();
				if (obj2 != null)
				{
					goto IL_009c;
				}
			}
			obj2 = "";
			goto IL_009c;
			IL_009c:
			text3 = (string)obj2;
			object value3 = 公告浏览表.Rows[i].Cells["公告内容"].Value;
			if (value3 != null)
			{
				obj3 = value3.ToString();
				if (obj3 != null)
				{
					continue;
				}
			}
			else
			{
				obj3 = null;
			}
			obj3 = "";
		}
		Settings.Default.系统公告内容 = text;
		Settings.Default.Save();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle28 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle29 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle30 = new System.Windows.Forms.DataGridViewCellStyle();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(游戏服务器.主窗口));
		this.主选项卡 = new System.Windows.Forms.TabControl();
		this.日志页面 = new System.Windows.Forms.TabPage();
		this.清空命令日志 = new System.Windows.Forms.Button();
		this.对象统计 = new System.Windows.Forms.Label();
		this.日志选项卡 = new System.Windows.Forms.TabControl();
		this.系统日志页面 = new System.Windows.Forms.TabPage();
		this.系统日志 = new System.Windows.Forms.RichTextBox();
		this.聊天日志页面 = new System.Windows.Forms.TabPage();
		this.聊天日志 = new System.Windows.Forms.RichTextBox();
		this.命令日志页面 = new System.Windows.Forms.TabPage();
		this.命令日志 = new System.Windows.Forms.RichTextBox();
		this.清空聊天日志 = new System.Windows.Forms.Button();
		this.保存聊天日志 = new System.Windows.Forms.Button();
		this.已经登录统计 = new System.Windows.Forms.Label();
		this.已经上线统计 = new System.Windows.Forms.Label();
		this.连接总数统计 = new System.Windows.Forms.Label();
		this.发送统计 = new System.Windows.Forms.Label();
		this.接收统计 = new System.Windows.Forms.Label();
		this.清空系统日志 = new System.Windows.Forms.Button();
		this.保存系统日志 = new System.Windows.Forms.Button();
		this.帧数统计 = new System.Windows.Forms.Label();
		this.角色页面 = new System.Windows.Forms.TabPage();
		this.角色详情选项卡 = new System.Windows.Forms.TabControl();
		this.角色数据_技能 = new System.Windows.Forms.TabPage();
		this.技能浏览表 = new System.Windows.Forms.DataGridView();
		this.角色数据_装备 = new System.Windows.Forms.TabPage();
		this.装备浏览表 = new System.Windows.Forms.DataGridView();
		this.角色数据_背包 = new System.Windows.Forms.TabPage();
		this.背包浏览表 = new System.Windows.Forms.DataGridView();
		this.角色数据_仓库 = new System.Windows.Forms.TabPage();
		this.仓库浏览表 = new System.Windows.Forms.DataGridView();
		this.角色浏览表 = new System.Windows.Forms.DataGridView();
		this.角色右键菜单 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.右键菜单_复制角色名字 = new System.Windows.Forms.ToolStripMenuItem();
		this.右键菜单_复制账号名字 = new System.Windows.Forms.ToolStripMenuItem();
		this.右键菜单_复制网络地址 = new System.Windows.Forms.ToolStripMenuItem();
		this.右键菜单_复制物理地址 = new System.Windows.Forms.ToolStripMenuItem();
		this.地图页面 = new System.Windows.Forms.TabPage();
		this.地图浏览表 = new System.Windows.Forms.DataGridView();
		this.怪物页面 = new System.Windows.Forms.TabPage();
		this.掉落浏览表 = new System.Windows.Forms.DataGridView();
		this.怪物浏览表 = new System.Windows.Forms.DataGridView();
		this.封禁页面 = new System.Windows.Forms.TabPage();
		this.封禁浏览表 = new System.Windows.Forms.DataGridView();
		this.公告页面 = new System.Windows.Forms.TabPage();
		this.开始公告按钮 = new System.Windows.Forms.Button();
		this.停止公告按钮 = new System.Windows.Forms.Button();
		this.删除公告按钮 = new System.Windows.Forms.Button();
		this.添加公告按钮 = new System.Windows.Forms.Button();
		this.公告浏览表 = new System.Windows.Forms.DataGridView();
		this.公告状态 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.公告间隔 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.公告次数 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.剩余次数 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.公告计时 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.公告内容 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.设置页面 = new System.Windows.Forms.TabPage();
		this.S_游戏数据分组 = new System.Windows.Forms.GroupBox();
		this.S_注意事项标签8 = new System.Windows.Forms.Label();
		this.S_注意事项标签7 = new System.Windows.Forms.Label();
		this.S_注意事项标签6 = new System.Windows.Forms.Label();
		this.S_注意事项标签5 = new System.Windows.Forms.Label();
		this.S_注意事项标签4 = new System.Windows.Forms.Label();
		this.S_注意事项标签3 = new System.Windows.Forms.Label();
		this.S_注意事项标签2 = new System.Windows.Forms.Label();
		this.S_注意事项标签1 = new System.Windows.Forms.Label();
		this.S_重载客户数据 = new System.Windows.Forms.Button();
		this.S_重载系统数据 = new System.Windows.Forms.Button();
		this.S_浏览合并目录 = new System.Windows.Forms.Button();
		this.S_浏览备份目录 = new System.Windows.Forms.Button();
		this.S_浏览数据目录 = new System.Windows.Forms.Button();
		this.S_合并客户数据 = new System.Windows.Forms.Button();
		this.S_合并数据目录 = new System.Windows.Forms.TextBox();
		this.S_合并目录标签 = new System.Windows.Forms.Label();
		this.S_数据备份目录 = new System.Windows.Forms.TextBox();
		this.S_游戏数据目录 = new System.Windows.Forms.TextBox();
		this.S_备份目录标签 = new System.Windows.Forms.Label();
		this.S_数据目录标签 = new System.Windows.Forms.Label();
		this.S_游戏设置分组 = new System.Windows.Forms.GroupBox();
		this.S_新手扶持标签 = new System.Windows.Forms.Label();
		this.S_新手扶持等级 = new System.Windows.Forms.NumericUpDown();
		this.S_物品归属标签 = new System.Windows.Forms.Label();
		this.S_物品归属时间 = new System.Windows.Forms.NumericUpDown();
		this.S_物品清理标签 = new System.Windows.Forms.Label();
		this.S_物品清理时间 = new System.Windows.Forms.NumericUpDown();
		this.S_诱惑时长标签 = new System.Windows.Forms.Label();
		this.S_怪物诱惑时长 = new System.Windows.Forms.NumericUpDown();
		this.S_收益衰减标签 = new System.Windows.Forms.Label();
		this.S_收益减少比率 = new System.Windows.Forms.NumericUpDown();
		this.S_收益等级标签 = new System.Windows.Forms.Label();
		this.S_减收益等级差 = new System.Windows.Forms.NumericUpDown();
		this.S_经验倍率标签 = new System.Windows.Forms.Label();
		this.S_怪物经验倍率 = new System.Windows.Forms.NumericUpDown();
		this.S_特修折扣标签 = new System.Windows.Forms.Label();
		this.S_装备特修折扣 = new System.Windows.Forms.NumericUpDown();
		this.S_怪物爆率标签 = new System.Windows.Forms.Label();
		this.S_怪物额外爆率 = new System.Windows.Forms.NumericUpDown();
		this.S_开放等级标签 = new System.Windows.Forms.Label();
		this.S_游戏开放等级 = new System.Windows.Forms.NumericUpDown();
		this.S_网络设置分组 = new System.Windows.Forms.GroupBox();
		this.S_掉线判定标签 = new System.Windows.Forms.Label();
		this.S_掉线判定时间 = new System.Windows.Forms.NumericUpDown();
		this.S_限定封包标签 = new System.Windows.Forms.Label();
		this.S_封包限定数量 = new System.Windows.Forms.NumericUpDown();
		this.S_屏蔽时间标签 = new System.Windows.Forms.Label();
		this.S_异常屏蔽时间 = new System.Windows.Forms.NumericUpDown();
		this.S_接收端口标签 = new System.Windows.Forms.Label();
		this.S_门票接收端口 = new System.Windows.Forms.NumericUpDown();
		this.S_监听端口标签 = new System.Windows.Forms.Label();
		this.S_客户连接端口 = new System.Windows.Forms.NumericUpDown();
		this.界面定时更新 = new System.Windows.Forms.Timer(this.components);
		this.下方控件页 = new System.Windows.Forms.Panel();
		this.保存按钮 = new System.Windows.Forms.Button();
		this.GM命令文本 = new System.Windows.Forms.TextBox();
		this.GM命令标签 = new System.Windows.Forms.Label();
		this.启动按钮 = new System.Windows.Forms.Button();
		this.停止按钮 = new System.Windows.Forms.Button();
		this.保存数据提醒 = new System.Windows.Forms.Timer(this.components);
		this.定时发送公告 = new System.Windows.Forms.Timer(this.components);
		this.主选项卡.SuspendLayout();
		this.日志页面.SuspendLayout();
		this.日志选项卡.SuspendLayout();
		this.系统日志页面.SuspendLayout();
		this.聊天日志页面.SuspendLayout();
		this.命令日志页面.SuspendLayout();
		this.角色页面.SuspendLayout();
		this.角色详情选项卡.SuspendLayout();
		this.角色数据_技能.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.技能浏览表).BeginInit();
		this.角色数据_装备.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.装备浏览表).BeginInit();
		this.角色数据_背包.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.背包浏览表).BeginInit();
		this.角色数据_仓库.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.仓库浏览表).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.角色浏览表).BeginInit();
		this.角色右键菜单.SuspendLayout();
		this.地图页面.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.地图浏览表).BeginInit();
		this.怪物页面.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.掉落浏览表).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.怪物浏览表).BeginInit();
		this.封禁页面.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.封禁浏览表).BeginInit();
		this.公告页面.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.公告浏览表).BeginInit();
		this.设置页面.SuspendLayout();
		this.S_游戏数据分组.SuspendLayout();
		this.S_游戏设置分组.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.S_新手扶持等级).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_物品归属时间).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_物品清理时间).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_怪物诱惑时长).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_收益减少比率).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_减收益等级差).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_怪物经验倍率).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_装备特修折扣).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_怪物额外爆率).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_游戏开放等级).BeginInit();
		this.S_网络设置分组.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.S_掉线判定时间).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_封包限定数量).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_异常屏蔽时间).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_门票接收端口).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.S_客户连接端口).BeginInit();
		this.下方控件页.SuspendLayout();
		base.SuspendLayout();
		this.主选项卡.AllowDrop = true;
		this.主选项卡.Controls.Add(this.日志页面);
		this.主选项卡.Controls.Add(this.角色页面);
		this.主选项卡.Controls.Add(this.地图页面);
		this.主选项卡.Controls.Add(this.怪物页面);
		this.主选项卡.Controls.Add(this.封禁页面);
		this.主选项卡.Controls.Add(this.公告页面);
		this.主选项卡.Controls.Add(this.设置页面);
		this.主选项卡.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.主选项卡.ItemSize = new System.Drawing.Size(170, 30);
		this.主选项卡.Location = new System.Drawing.Point(3, 3);
		this.主选项卡.Name = "主选项卡";
		this.主选项卡.SelectedIndex = 0;
		this.主选项卡.Size = new System.Drawing.Size(1194, 483);
		this.主选项卡.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
		this.主选项卡.TabIndex = 5;
		this.主选项卡.TabStop = false;
		this.日志页面.BackColor = System.Drawing.Color.Gainsboro;
		this.日志页面.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.日志页面.Controls.Add(this.清空命令日志);
		this.日志页面.Controls.Add(this.对象统计);
		this.日志页面.Controls.Add(this.日志选项卡);
		this.日志页面.Controls.Add(this.清空聊天日志);
		this.日志页面.Controls.Add(this.保存聊天日志);
		this.日志页面.Controls.Add(this.已经登录统计);
		this.日志页面.Controls.Add(this.已经上线统计);
		this.日志页面.Controls.Add(this.连接总数统计);
		this.日志页面.Controls.Add(this.发送统计);
		this.日志页面.Controls.Add(this.接收统计);
		this.日志页面.Controls.Add(this.清空系统日志);
		this.日志页面.Controls.Add(this.保存系统日志);
		this.日志页面.Controls.Add(this.帧数统计);
		this.日志页面.Location = new System.Drawing.Point(4, 34);
		this.日志页面.Name = "日志页面";
		this.日志页面.Padding = new System.Windows.Forms.Padding(3);
		this.日志页面.Size = new System.Drawing.Size(1186, 445);
		this.日志页面.TabIndex = 0;
		this.日志页面.Text = "日志";
		this.清空命令日志.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
		this.清空命令日志.Enabled = false;
		this.清空命令日志.Location = new System.Drawing.Point(915, 394);
		this.清空命令日志.Name = "清空命令日志";
		this.清空命令日志.Size = new System.Drawing.Size(258, 40);
		this.清空命令日志.TabIndex = 18;
		this.清空命令日志.Text = "清空命令日志";
		this.清空命令日志.UseVisualStyleBackColor = false;
		this.清空命令日志.Click += new System.EventHandler(清空命令日志_Click);
		this.对象统计.AutoSize = true;
		this.对象统计.ForeColor = System.Drawing.Color.Maroon;
		this.对象统计.Location = new System.Drawing.Point(915, 158);
		this.对象统计.Name = "对象统计";
		this.对象统计.Size = new System.Drawing.Size(75, 14);
		this.对象统计.TabIndex = 17;
		this.对象统计.Text = "对象统计:";
		this.日志选项卡.Controls.Add(this.系统日志页面);
		this.日志选项卡.Controls.Add(this.聊天日志页面);
		this.日志选项卡.Controls.Add(this.命令日志页面);
		this.日志选项卡.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.日志选项卡.ItemSize = new System.Drawing.Size(294, 20);
		this.日志选项卡.Location = new System.Drawing.Point(2, 2);
		this.日志选项卡.Name = "日志选项卡";
		this.日志选项卡.SelectedIndex = 0;
		this.日志选项卡.Size = new System.Drawing.Size(886, 438);
		this.日志选项卡.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
		this.日志选项卡.TabIndex = 16;
		this.系统日志页面.BackColor = System.Drawing.Color.Gainsboro;
		this.系统日志页面.Controls.Add(this.系统日志);
		this.系统日志页面.Location = new System.Drawing.Point(4, 24);
		this.系统日志页面.Name = "系统日志页面";
		this.系统日志页面.Padding = new System.Windows.Forms.Padding(3);
		this.系统日志页面.Size = new System.Drawing.Size(878, 410);
		this.系统日志页面.TabIndex = 0;
		this.系统日志页面.Text = "系统日志";
		this.系统日志.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.系统日志.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.系统日志.Location = new System.Drawing.Point(3, 3);
		this.系统日志.Name = "系统日志";
		this.系统日志.ReadOnly = true;
		this.系统日志.Size = new System.Drawing.Size(872, 404);
		this.系统日志.TabIndex = 0;
		this.系统日志.Text = "";
		this.聊天日志页面.BackColor = System.Drawing.Color.Gainsboro;
		this.聊天日志页面.Controls.Add(this.聊天日志);
		this.聊天日志页面.Location = new System.Drawing.Point(4, 24);
		this.聊天日志页面.Name = "聊天日志页面";
		this.聊天日志页面.Padding = new System.Windows.Forms.Padding(3);
		this.聊天日志页面.Size = new System.Drawing.Size(878, 410);
		this.聊天日志页面.TabIndex = 1;
		this.聊天日志页面.Text = "聊天日志";
		this.聊天日志.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.聊天日志.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.聊天日志.Location = new System.Drawing.Point(3, 3);
		this.聊天日志.Name = "聊天日志";
		this.聊天日志.ReadOnly = true;
		this.聊天日志.Size = new System.Drawing.Size(872, 404);
		this.聊天日志.TabIndex = 1;
		this.聊天日志.Text = "";
		this.命令日志页面.BackColor = System.Drawing.Color.Gainsboro;
		this.命令日志页面.Controls.Add(this.命令日志);
		this.命令日志页面.Location = new System.Drawing.Point(4, 24);
		this.命令日志页面.Name = "命令日志页面";
		this.命令日志页面.Size = new System.Drawing.Size(878, 410);
		this.命令日志页面.TabIndex = 2;
		this.命令日志页面.Text = "命令日志";
		this.命令日志.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.命令日志.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.命令日志.Location = new System.Drawing.Point(0, 0);
		this.命令日志.Name = "命令日志";
		this.命令日志.ReadOnly = true;
		this.命令日志.Size = new System.Drawing.Size(878, 410);
		this.命令日志.TabIndex = 2;
		this.命令日志.Text = "";
		this.清空聊天日志.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
		this.清空聊天日志.Enabled = false;
		this.清空聊天日志.Location = new System.Drawing.Point(915, 342);
		this.清空聊天日志.Name = "清空聊天日志";
		this.清空聊天日志.Size = new System.Drawing.Size(258, 40);
		this.清空聊天日志.TabIndex = 15;
		this.清空聊天日志.Text = "清空聊天日志";
		this.清空聊天日志.UseVisualStyleBackColor = false;
		this.清空聊天日志.Click += new System.EventHandler(清空聊天日志_Click);
		this.保存聊天日志.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
		this.保存聊天日志.Enabled = false;
		this.保存聊天日志.Location = new System.Drawing.Point(915, 238);
		this.保存聊天日志.Name = "保存聊天日志";
		this.保存聊天日志.Size = new System.Drawing.Size(258, 40);
		this.保存聊天日志.TabIndex = 14;
		this.保存聊天日志.Text = "保存聊天日志";
		this.保存聊天日志.UseVisualStyleBackColor = false;
		this.保存聊天日志.Click += new System.EventHandler(保存聊天日志_Click);
		this.已经登录统计.AutoSize = true;
		this.已经登录统计.ForeColor = System.Drawing.Color.Purple;
		this.已经登录统计.Location = new System.Drawing.Point(915, 48);
		this.已经登录统计.Name = "已经登录统计";
		this.已经登录统计.Size = new System.Drawing.Size(75, 14);
		this.已经登录统计.TabIndex = 13;
		this.已经登录统计.Text = "已经登录:";
		this.已经上线统计.AutoSize = true;
		this.已经上线统计.ForeColor = System.Drawing.Color.Purple;
		this.已经上线统计.Location = new System.Drawing.Point(915, 70);
		this.已经上线统计.Name = "已经上线统计";
		this.已经上线统计.Size = new System.Drawing.Size(75, 14);
		this.已经上线统计.TabIndex = 12;
		this.已经上线统计.Text = "已经上线:";
		this.连接总数统计.AutoSize = true;
		this.连接总数统计.ForeColor = System.Drawing.Color.Purple;
		this.连接总数统计.Location = new System.Drawing.Point(915, 26);
		this.连接总数统计.Name = "连接总数统计";
		this.连接总数统计.Size = new System.Drawing.Size(75, 14);
		this.连接总数统计.TabIndex = 11;
		this.连接总数统计.Text = "连接总数:";
		this.发送统计.AutoSize = true;
		this.发送统计.ForeColor = System.Drawing.Color.Teal;
		this.发送统计.Location = new System.Drawing.Point(915, 136);
		this.发送统计.Name = "发送统计";
		this.发送统计.Size = new System.Drawing.Size(75, 14);
		this.发送统计.TabIndex = 10;
		this.发送统计.Text = "已经发送:";
		this.接收统计.AutoSize = true;
		this.接收统计.ForeColor = System.Drawing.Color.Teal;
		this.接收统计.Location = new System.Drawing.Point(915, 114);
		this.接收统计.Name = "接收统计";
		this.接收统计.Size = new System.Drawing.Size(75, 14);
		this.接收统计.TabIndex = 9;
		this.接收统计.Text = "已经接收:";
		this.清空系统日志.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
		this.清空系统日志.Enabled = false;
		this.清空系统日志.Location = new System.Drawing.Point(915, 290);
		this.清空系统日志.Name = "清空系统日志";
		this.清空系统日志.Size = new System.Drawing.Size(258, 40);
		this.清空系统日志.TabIndex = 8;
		this.清空系统日志.Text = "清空系统日志";
		this.清空系统日志.UseVisualStyleBackColor = false;
		this.清空系统日志.Click += new System.EventHandler(清空系统日志_Click);
		this.保存系统日志.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
		this.保存系统日志.Enabled = false;
		this.保存系统日志.Location = new System.Drawing.Point(915, 186);
		this.保存系统日志.Name = "保存系统日志";
		this.保存系统日志.Size = new System.Drawing.Size(258, 40);
		this.保存系统日志.TabIndex = 7;
		this.保存系统日志.Text = "保存系统日志";
		this.保存系统日志.UseVisualStyleBackColor = false;
		this.保存系统日志.Click += new System.EventHandler(保存系统日志_Click);
		this.帧数统计.AutoSize = true;
		this.帧数统计.ForeColor = System.Drawing.Color.Blue;
		this.帧数统计.Location = new System.Drawing.Point(915, 92);
		this.帧数统计.Name = "帧数统计";
		this.帧数统计.Size = new System.Drawing.Size(75, 14);
		this.帧数统计.TabIndex = 1;
		this.帧数统计.Text = "后台帧数:";
		this.角色页面.BackColor = System.Drawing.Color.Gainsboro;
		this.角色页面.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.角色页面.Controls.Add(this.角色详情选项卡);
		this.角色页面.Controls.Add(this.角色浏览表);
		this.角色页面.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.角色页面.Location = new System.Drawing.Point(4, 34);
		this.角色页面.Name = "角色页面";
		this.角色页面.Padding = new System.Windows.Forms.Padding(3);
		this.角色页面.Size = new System.Drawing.Size(1186, 445);
		this.角色页面.TabIndex = 4;
		this.角色页面.Text = "角色";
		this.角色详情选项卡.Controls.Add(this.角色数据_技能);
		this.角色详情选项卡.Controls.Add(this.角色数据_装备);
		this.角色详情选项卡.Controls.Add(this.角色数据_背包);
		this.角色详情选项卡.Controls.Add(this.角色数据_仓库);
		this.角色详情选项卡.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.角色详情选项卡.ItemSize = new System.Drawing.Size(85, 20);
		this.角色详情选项卡.Location = new System.Drawing.Point(841, 3);
		this.角色详情选项卡.Name = "角色详情选项卡";
		this.角色详情选项卡.SelectedIndex = 0;
		this.角色详情选项卡.Size = new System.Drawing.Size(345, 434);
		this.角色详情选项卡.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
		this.角色详情选项卡.TabIndex = 2;
		this.角色数据_技能.BackColor = System.Drawing.Color.Gainsboro;
		this.角色数据_技能.Controls.Add(this.技能浏览表);
		this.角色数据_技能.Location = new System.Drawing.Point(4, 24);
		this.角色数据_技能.Name = "角色数据_技能";
		this.角色数据_技能.Padding = new System.Windows.Forms.Padding(3);
		this.角色数据_技能.Size = new System.Drawing.Size(337, 406);
		this.角色数据_技能.TabIndex = 0;
		this.角色数据_技能.Text = "技能";
		this.技能浏览表.AllowUserToAddRows = false;
		this.技能浏览表.AllowUserToDeleteRows = false;
		this.技能浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.技能浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.技能浏览表.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.技能浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.技能浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.技能浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.技能浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.技能浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.技能浏览表.DefaultCellStyle = dataGridViewCellStyle3;
		this.技能浏览表.Dock = System.Windows.Forms.DockStyle.Fill;
		this.技能浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.技能浏览表.Location = new System.Drawing.Point(3, 3);
		this.技能浏览表.MultiSelect = false;
		this.技能浏览表.Name = "技能浏览表";
		this.技能浏览表.ReadOnly = true;
		this.技能浏览表.RowHeadersVisible = false;
		this.技能浏览表.RowTemplate.Height = 23;
		this.技能浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.技能浏览表.Size = new System.Drawing.Size(331, 400);
		this.技能浏览表.TabIndex = 3;
		this.角色数据_装备.BackColor = System.Drawing.Color.Gainsboro;
		this.角色数据_装备.Controls.Add(this.装备浏览表);
		this.角色数据_装备.Location = new System.Drawing.Point(4, 24);
		this.角色数据_装备.Name = "角色数据_装备";
		this.角色数据_装备.Padding = new System.Windows.Forms.Padding(3);
		this.角色数据_装备.Size = new System.Drawing.Size(337, 406);
		this.角色数据_装备.TabIndex = 1;
		this.角色数据_装备.Text = "装备";
		this.装备浏览表.AllowUserToAddRows = false;
		this.装备浏览表.AllowUserToDeleteRows = false;
		this.装备浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.装备浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
		this.装备浏览表.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.装备浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.装备浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.装备浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.装备浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
		this.装备浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.装备浏览表.DefaultCellStyle = dataGridViewCellStyle6;
		this.装备浏览表.Dock = System.Windows.Forms.DockStyle.Fill;
		this.装备浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.装备浏览表.Location = new System.Drawing.Point(3, 3);
		this.装备浏览表.MultiSelect = false;
		this.装备浏览表.Name = "装备浏览表";
		this.装备浏览表.ReadOnly = true;
		this.装备浏览表.RowHeadersVisible = false;
		this.装备浏览表.RowTemplate.Height = 23;
		this.装备浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.装备浏览表.Size = new System.Drawing.Size(331, 400);
		this.装备浏览表.TabIndex = 4;
		this.角色数据_背包.BackColor = System.Drawing.Color.Gainsboro;
		this.角色数据_背包.Controls.Add(this.背包浏览表);
		this.角色数据_背包.Location = new System.Drawing.Point(4, 24);
		this.角色数据_背包.Name = "角色数据_背包";
		this.角色数据_背包.Size = new System.Drawing.Size(337, 406);
		this.角色数据_背包.TabIndex = 2;
		this.角色数据_背包.Text = "背包";
		this.背包浏览表.AllowUserToAddRows = false;
		this.背包浏览表.AllowUserToDeleteRows = false;
		this.背包浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.背包浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle7;
		this.背包浏览表.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.背包浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.背包浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.背包浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle8.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.背包浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle8;
		this.背包浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle9.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.背包浏览表.DefaultCellStyle = dataGridViewCellStyle9;
		this.背包浏览表.Dock = System.Windows.Forms.DockStyle.Fill;
		this.背包浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.背包浏览表.Location = new System.Drawing.Point(0, 0);
		this.背包浏览表.MultiSelect = false;
		this.背包浏览表.Name = "背包浏览表";
		this.背包浏览表.ReadOnly = true;
		this.背包浏览表.RowHeadersVisible = false;
		this.背包浏览表.RowTemplate.Height = 23;
		this.背包浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.背包浏览表.Size = new System.Drawing.Size(337, 406);
		this.背包浏览表.TabIndex = 4;
		this.角色数据_仓库.BackColor = System.Drawing.Color.Gainsboro;
		this.角色数据_仓库.Controls.Add(this.仓库浏览表);
		this.角色数据_仓库.Location = new System.Drawing.Point(4, 24);
		this.角色数据_仓库.Name = "角色数据_仓库";
		this.角色数据_仓库.Size = new System.Drawing.Size(337, 406);
		this.角色数据_仓库.TabIndex = 3;
		this.角色数据_仓库.Text = "仓库";
		this.仓库浏览表.AllowUserToAddRows = false;
		this.仓库浏览表.AllowUserToDeleteRows = false;
		this.仓库浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle10.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.仓库浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle10;
		this.仓库浏览表.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.仓库浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.仓库浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.仓库浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle11.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.仓库浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle11;
		this.仓库浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle12.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.仓库浏览表.DefaultCellStyle = dataGridViewCellStyle12;
		this.仓库浏览表.Dock = System.Windows.Forms.DockStyle.Fill;
		this.仓库浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.仓库浏览表.Location = new System.Drawing.Point(0, 0);
		this.仓库浏览表.MultiSelect = false;
		this.仓库浏览表.Name = "仓库浏览表";
		this.仓库浏览表.ReadOnly = true;
		this.仓库浏览表.RowHeadersVisible = false;
		this.仓库浏览表.RowTemplate.Height = 23;
		this.仓库浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.仓库浏览表.Size = new System.Drawing.Size(337, 406);
		this.仓库浏览表.TabIndex = 5;
		this.角色浏览表.AllowUserToAddRows = false;
		this.角色浏览表.AllowUserToDeleteRows = false;
		this.角色浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle13.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.角色浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle13;
		this.角色浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.角色浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.角色浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle14.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.角色浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle14;
		this.角色浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		this.角色浏览表.ContextMenuStrip = this.角色右键菜单;
		dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle15.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.角色浏览表.DefaultCellStyle = dataGridViewCellStyle15;
		this.角色浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.角色浏览表.Location = new System.Drawing.Point(0, 3);
		this.角色浏览表.MultiSelect = false;
		this.角色浏览表.Name = "角色浏览表";
		this.角色浏览表.ReadOnly = true;
		this.角色浏览表.RowHeadersVisible = false;
		this.角色浏览表.RowTemplate.Height = 23;
		this.角色浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.角色浏览表.Size = new System.Drawing.Size(839, 434);
		this.角色浏览表.TabIndex = 1;
		this.角色右键菜单.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.右键菜单_复制角色名字, this.右键菜单_复制账号名字, this.右键菜单_复制网络地址, this.右键菜单_复制物理地址 });
		this.角色右键菜单.Name = "角色右键菜单";
		this.角色右键菜单.Size = new System.Drawing.Size(149, 92);
		this.右键菜单_复制角色名字.Name = "右键菜单_复制角色名字";
		this.右键菜单_复制角色名字.Size = new System.Drawing.Size(148, 22);
		this.右键菜单_复制角色名字.Text = "复制角色名字";
		this.右键菜单_复制角色名字.Click += new System.EventHandler(角色右键菜单_Click);
		this.右键菜单_复制账号名字.Name = "右键菜单_复制账号名字";
		this.右键菜单_复制账号名字.Size = new System.Drawing.Size(148, 22);
		this.右键菜单_复制账号名字.Text = "复制账号名字";
		this.右键菜单_复制账号名字.Click += new System.EventHandler(角色右键菜单_Click);
		this.右键菜单_复制网络地址.Name = "右键菜单_复制网络地址";
		this.右键菜单_复制网络地址.Size = new System.Drawing.Size(148, 22);
		this.右键菜单_复制网络地址.Text = "复制网络地址";
		this.右键菜单_复制网络地址.Click += new System.EventHandler(角色右键菜单_Click);
		this.右键菜单_复制物理地址.Name = "右键菜单_复制物理地址";
		this.右键菜单_复制物理地址.Size = new System.Drawing.Size(148, 22);
		this.右键菜单_复制物理地址.Text = "复制物理地址";
		this.右键菜单_复制物理地址.Click += new System.EventHandler(角色右键菜单_Click);
		this.地图页面.BackColor = System.Drawing.Color.Gainsboro;
		this.地图页面.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.地图页面.Controls.Add(this.地图浏览表);
		this.地图页面.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.地图页面.Location = new System.Drawing.Point(4, 34);
		this.地图页面.Name = "地图页面";
		this.地图页面.Padding = new System.Windows.Forms.Padding(3);
		this.地图页面.Size = new System.Drawing.Size(1186, 445);
		this.地图页面.TabIndex = 1;
		this.地图页面.Text = "地图";
		this.地图浏览表.AllowUserToAddRows = false;
		this.地图浏览表.AllowUserToDeleteRows = false;
		this.地图浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle16.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.地图浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle16;
		this.地图浏览表.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.地图浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.地图浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.地图浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle17.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.地图浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle17;
		this.地图浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle18.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.地图浏览表.DefaultCellStyle = dataGridViewCellStyle18;
		this.地图浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.地图浏览表.Location = new System.Drawing.Point(0, 3);
		this.地图浏览表.MultiSelect = false;
		this.地图浏览表.Name = "地图浏览表";
		this.地图浏览表.ReadOnly = true;
		this.地图浏览表.RowHeadersVisible = false;
		this.地图浏览表.RowTemplate.Height = 23;
		this.地图浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.地图浏览表.Size = new System.Drawing.Size(1187, 434);
		this.地图浏览表.TabIndex = 2;
		this.怪物页面.BackColor = System.Drawing.Color.Gainsboro;
		this.怪物页面.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.怪物页面.Controls.Add(this.掉落浏览表);
		this.怪物页面.Controls.Add(this.怪物浏览表);
		this.怪物页面.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.怪物页面.Location = new System.Drawing.Point(4, 34);
		this.怪物页面.Name = "怪物页面";
		this.怪物页面.Size = new System.Drawing.Size(1186, 445);
		this.怪物页面.TabIndex = 2;
		this.怪物页面.Text = "怪物";
		this.掉落浏览表.AllowUserToAddRows = false;
		this.掉落浏览表.AllowUserToDeleteRows = false;
		this.掉落浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle19.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.掉落浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle19;
		this.掉落浏览表.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.掉落浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.掉落浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.掉落浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle20.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle20.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle20.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle20.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle20.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle20.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.掉落浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle20;
		this.掉落浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		dataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle21.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle21.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle21.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle21.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle21.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle21.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.掉落浏览表.DefaultCellStyle = dataGridViewCellStyle21;
		this.掉落浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.掉落浏览表.Location = new System.Drawing.Point(902, 3);
		this.掉落浏览表.MultiSelect = false;
		this.掉落浏览表.Name = "掉落浏览表";
		this.掉落浏览表.ReadOnly = true;
		this.掉落浏览表.RowHeadersVisible = false;
		this.掉落浏览表.RowTemplate.Height = 23;
		this.掉落浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.掉落浏览表.Size = new System.Drawing.Size(285, 434);
		this.掉落浏览表.TabIndex = 5;
		this.怪物浏览表.AllowUserToAddRows = false;
		this.怪物浏览表.AllowUserToDeleteRows = false;
		this.怪物浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle22.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.怪物浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle22;
		this.怪物浏览表.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.怪物浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.怪物浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.怪物浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle23.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle23.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle23.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle23.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle23.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle23.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.怪物浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle23;
		this.怪物浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle24.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle24.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle24.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle24.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle24.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle24.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.怪物浏览表.DefaultCellStyle = dataGridViewCellStyle24;
		this.怪物浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.怪物浏览表.Location = new System.Drawing.Point(0, 3);
		this.怪物浏览表.MultiSelect = false;
		this.怪物浏览表.Name = "怪物浏览表";
		this.怪物浏览表.ReadOnly = true;
		this.怪物浏览表.RowHeadersVisible = false;
		this.怪物浏览表.RowTemplate.Height = 23;
		this.怪物浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.怪物浏览表.Size = new System.Drawing.Size(896, 434);
		this.怪物浏览表.TabIndex = 3;
		this.封禁页面.BackColor = System.Drawing.Color.Gainsboro;
		this.封禁页面.Controls.Add(this.封禁浏览表);
		this.封禁页面.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.封禁页面.Location = new System.Drawing.Point(4, 34);
		this.封禁页面.Name = "封禁页面";
		this.封禁页面.Size = new System.Drawing.Size(1186, 445);
		this.封禁页面.TabIndex = 12;
		this.封禁页面.Text = "封禁";
		this.封禁浏览表.AllowUserToAddRows = false;
		this.封禁浏览表.AllowUserToDeleteRows = false;
		this.封禁浏览表.AllowUserToResizeRows = false;
		dataGridViewCellStyle25.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.封禁浏览表.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle25;
		this.封禁浏览表.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.封禁浏览表.BackgroundColor = System.Drawing.Color.LightGray;
		this.封禁浏览表.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.封禁浏览表.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
		dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle26.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle26.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle26.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle26.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle26.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.封禁浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle26;
		this.封禁浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		dataGridViewCellStyle27.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle27.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle27.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle27.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle27.SelectionBackColor = System.Drawing.SystemColors.ButtonShadow;
		dataGridViewCellStyle27.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle27.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.封禁浏览表.DefaultCellStyle = dataGridViewCellStyle27;
		this.封禁浏览表.GridColor = System.Drawing.SystemColors.ActiveCaption;
		this.封禁浏览表.Location = new System.Drawing.Point(126, 3);
		this.封禁浏览表.MultiSelect = false;
		this.封禁浏览表.Name = "封禁浏览表";
		this.封禁浏览表.ReadOnly = true;
		this.封禁浏览表.RowHeadersVisible = false;
		this.封禁浏览表.RowTemplate.Height = 23;
		this.封禁浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.封禁浏览表.Size = new System.Drawing.Size(857, 434);
		this.封禁浏览表.TabIndex = 6;
		this.公告页面.BackColor = System.Drawing.Color.Gainsboro;
		this.公告页面.Controls.Add(this.开始公告按钮);
		this.公告页面.Controls.Add(this.停止公告按钮);
		this.公告页面.Controls.Add(this.删除公告按钮);
		this.公告页面.Controls.Add(this.添加公告按钮);
		this.公告页面.Controls.Add(this.公告浏览表);
		this.公告页面.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.公告页面.Location = new System.Drawing.Point(4, 34);
		this.公告页面.Name = "公告页面";
		this.公告页面.Size = new System.Drawing.Size(1186, 445);
		this.公告页面.TabIndex = 13;
		this.公告页面.Text = "公告";
		this.开始公告按钮.Enabled = false;
		this.开始公告按钮.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.开始公告按钮.Location = new System.Drawing.Point(5, 397);
		this.开始公告按钮.Name = "开始公告按钮";
		this.开始公告按钮.Size = new System.Drawing.Size(294, 30);
		this.开始公告按钮.TabIndex = 7;
		this.开始公告按钮.Text = "开始选中公告";
		this.开始公告按钮.UseVisualStyleBackColor = true;
		this.开始公告按钮.Click += new System.EventHandler(开始公告按钮_Click);
		this.停止公告按钮.Enabled = false;
		this.停止公告按钮.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.停止公告按钮.Location = new System.Drawing.Point(299, 397);
		this.停止公告按钮.Name = "停止公告按钮";
		this.停止公告按钮.Size = new System.Drawing.Size(294, 30);
		this.停止公告按钮.TabIndex = 6;
		this.停止公告按钮.Text = "停止选中公告";
		this.停止公告按钮.UseVisualStyleBackColor = true;
		this.停止公告按钮.Click += new System.EventHandler(停止公告按钮_Click);
		this.删除公告按钮.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.删除公告按钮.Location = new System.Drawing.Point(887, 397);
		this.删除公告按钮.Name = "删除公告按钮";
		this.删除公告按钮.Size = new System.Drawing.Size(294, 30);
		this.删除公告按钮.TabIndex = 5;
		this.删除公告按钮.Text = "删除选中公告";
		this.删除公告按钮.UseVisualStyleBackColor = true;
		this.删除公告按钮.Click += new System.EventHandler(删除公告按钮_Click);
		this.添加公告按钮.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.添加公告按钮.Location = new System.Drawing.Point(593, 397);
		this.添加公告按钮.Name = "添加公告按钮";
		this.添加公告按钮.Size = new System.Drawing.Size(294, 30);
		this.添加公告按钮.TabIndex = 4;
		this.添加公告按钮.Text = "添加新公告行";
		this.添加公告按钮.UseVisualStyleBackColor = true;
		this.添加公告按钮.Click += new System.EventHandler(添加公告按钮_Click);
		this.公告浏览表.AllowUserToAddRows = false;
		this.公告浏览表.AllowUserToDeleteRows = false;
		this.公告浏览表.AllowUserToResizeColumns = false;
		this.公告浏览表.AllowUserToResizeRows = false;
		this.公告浏览表.BackgroundColor = System.Drawing.SystemColors.ControlLight;
		dataGridViewCellStyle28.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle28.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle28.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle28.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle28.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle28.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle28.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.公告浏览表.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle28;
		this.公告浏览表.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		this.公告浏览表.Columns.AddRange(this.公告状态, this.公告间隔, this.公告次数, this.剩余次数, this.公告计时, this.公告内容);
		dataGridViewCellStyle29.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle29.BackColor = System.Drawing.Color.LightGray;
		dataGridViewCellStyle29.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle29.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle29.SelectionBackColor = System.Drawing.SystemColors.ActiveBorder;
		dataGridViewCellStyle29.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle29.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.公告浏览表.DefaultCellStyle = dataGridViewCellStyle29;
		this.公告浏览表.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
		this.公告浏览表.Location = new System.Drawing.Point(5, 3);
		this.公告浏览表.MultiSelect = false;
		this.公告浏览表.Name = "公告浏览表";
		dataGridViewCellStyle30.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomCenter;
		dataGridViewCellStyle30.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle30.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		dataGridViewCellStyle30.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle30.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle30.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle30.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.公告浏览表.RowHeadersDefaultCellStyle = dataGridViewCellStyle30;
		this.公告浏览表.RowHeadersVisible = false;
		this.公告浏览表.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
		this.公告浏览表.RowTemplate.Height = 23;
		this.公告浏览表.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.公告浏览表.Size = new System.Drawing.Size(1178, 376);
		this.公告浏览表.TabIndex = 3;
		this.公告浏览表.TabStop = false;
		this.公告浏览表.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(公告浏览表_CellEndEdit);
		this.公告浏览表.SelectionChanged += new System.EventHandler(公告浏览表_SelectionChanged);
		this.公告状态.Frozen = true;
		this.公告状态.HeaderText = "";
		this.公告状态.Name = "公告状态";
		this.公告状态.ReadOnly = true;
		this.公告状态.Width = 20;
		this.公告间隔.DataPropertyName = "公告间隔";
		this.公告间隔.Frozen = true;
		this.公告间隔.HeaderText = "间隔分钟";
		this.公告间隔.Name = "公告间隔";
		this.公告间隔.Width = 60;
		this.公告次数.DataPropertyName = "公告次数";
		this.公告次数.Frozen = true;
		this.公告次数.HeaderText = "公告次数";
		this.公告次数.Name = "公告次数";
		this.公告次数.Width = 60;
		this.剩余次数.Frozen = true;
		this.剩余次数.HeaderText = "剩余次数";
		this.剩余次数.Name = "剩余次数";
		this.剩余次数.ReadOnly = true;
		this.剩余次数.Width = 60;
		this.公告计时.Frozen = true;
		this.公告计时.HeaderText = "公告计时";
		this.公告计时.Name = "公告计时";
		this.公告计时.ReadOnly = true;
		this.公告计时.Width = 90;
		this.公告内容.DataPropertyName = "公告内容";
		this.公告内容.Frozen = true;
		this.公告内容.HeaderText = "公告内容";
		this.公告内容.Name = "公告内容";
		this.公告内容.Width = 884;
		this.设置页面.BackColor = System.Drawing.Color.Gainsboro;
		this.设置页面.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.设置页面.Controls.Add(this.S_游戏数据分组);
		this.设置页面.Controls.Add(this.S_游戏设置分组);
		this.设置页面.Controls.Add(this.S_网络设置分组);
		this.设置页面.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.设置页面.Location = new System.Drawing.Point(4, 34);
		this.设置页面.Name = "设置页面";
		this.设置页面.Size = new System.Drawing.Size(1186, 445);
		this.设置页面.TabIndex = 11;
		this.设置页面.Text = "设置";
		this.S_游戏数据分组.Controls.Add(this.S_注意事项标签8);
		this.S_游戏数据分组.Controls.Add(this.S_注意事项标签7);
		this.S_游戏数据分组.Controls.Add(this.S_注意事项标签6);
		this.S_游戏数据分组.Controls.Add(this.S_注意事项标签5);
		this.S_游戏数据分组.Controls.Add(this.S_注意事项标签4);
		this.S_游戏数据分组.Controls.Add(this.S_注意事项标签3);
		this.S_游戏数据分组.Controls.Add(this.S_注意事项标签2);
		this.S_游戏数据分组.Controls.Add(this.S_注意事项标签1);
		this.S_游戏数据分组.Controls.Add(this.S_重载客户数据);
		this.S_游戏数据分组.Controls.Add(this.S_重载系统数据);
		this.S_游戏数据分组.Controls.Add(this.S_浏览合并目录);
		this.S_游戏数据分组.Controls.Add(this.S_浏览备份目录);
		this.S_游戏数据分组.Controls.Add(this.S_浏览数据目录);
		this.S_游戏数据分组.Controls.Add(this.S_合并客户数据);
		this.S_游戏数据分组.Controls.Add(this.S_合并数据目录);
		this.S_游戏数据分组.Controls.Add(this.S_合并目录标签);
		this.S_游戏数据分组.Controls.Add(this.S_数据备份目录);
		this.S_游戏数据分组.Controls.Add(this.S_游戏数据目录);
		this.S_游戏数据分组.Controls.Add(this.S_备份目录标签);
		this.S_游戏数据分组.Controls.Add(this.S_数据目录标签);
		this.S_游戏数据分组.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_游戏数据分组.Location = new System.Drawing.Point(635, 12);
		this.S_游戏数据分组.Name = "S_游戏数据分组";
		this.S_游戏数据分组.Size = new System.Drawing.Size(477, 413);
		this.S_游戏数据分组.TabIndex = 10;
		this.S_游戏数据分组.TabStop = false;
		this.S_游戏数据分组.Text = "游戏数据";
		this.S_注意事项标签8.AutoSize = true;
		this.S_注意事项标签8.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_注意事项标签8.ForeColor = System.Drawing.Color.Blue;
		this.S_注意事项标签8.Location = new System.Drawing.Point(40, 371);
		this.S_注意事项标签8.Name = "S_注意事项标签8";
		this.S_注意事项标签8.Size = new System.Drawing.Size(353, 12);
		this.S_注意事项标签8.TabIndex = 27;
		this.S_注意事项标签8.Text = "被误判为网络攻击的玩家需要等异常屏蔽时间结束后才能正常登陆";
		this.S_注意事项标签7.AutoSize = true;
		this.S_注意事项标签7.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_注意事项标签7.ForeColor = System.Drawing.Color.Blue;
		this.S_注意事项标签7.Location = new System.Drawing.Point(40, 351);
		this.S_注意事项标签7.Name = "S_注意事项标签7";
		this.S_注意事项标签7.Size = new System.Drawing.Size(425, 12);
		this.S_注意事项标签7.TabIndex = 26;
		this.S_注意事项标签7.Text = "网络卡顿会造成大量封包堆积, 封包限定数量太小容易误判服务器遭受网络攻击";
		this.S_注意事项标签6.AutoSize = true;
		this.S_注意事项标签6.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_注意事项标签6.ForeColor = System.Drawing.Color.Blue;
		this.S_注意事项标签6.Location = new System.Drawing.Point(40, 331);
		this.S_注意事项标签6.Name = "S_注意事项标签6";
		this.S_注意事项标签6.Size = new System.Drawing.Size(413, 12);
		this.S_注意事项标签6.TabIndex = 25;
		this.S_注意事项标签6.Text = "玩家停留在角色选择界面时, 客户端不会发送心跳包, 掉线判定时间不宜太短";
		this.S_注意事项标签5.AutoSize = true;
		this.S_注意事项标签5.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_注意事项标签5.ForeColor = System.Drawing.Color.Blue;
		this.S_注意事项标签5.Location = new System.Drawing.Point(40, 311);
		this.S_注意事项标签5.Name = "S_注意事项标签5";
		this.S_注意事项标签5.Size = new System.Drawing.Size(389, 12);
		this.S_注意事项标签5.TabIndex = 24;
		this.S_注意事项标签5.Text = "数据目录内文件夹名字和结构固定, 请勿随意修改, 也不要放入无关文件";
		this.S_注意事项标签4.AutoSize = true;
		this.S_注意事项标签4.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_注意事项标签4.ForeColor = System.Drawing.Color.Blue;
		this.S_注意事项标签4.Location = new System.Drawing.Point(40, 291);
		this.S_注意事项标签4.Name = "S_注意事项标签4";
		this.S_注意事项标签4.Size = new System.Drawing.Size(389, 12);
		this.S_注意事项标签4.TabIndex = 23;
		this.S_注意事项标签4.Text = "收益减少比率为超出等级差时, 每超出一级时减少设定比率的经验和爆率";
		this.S_注意事项标签3.AutoSize = true;
		this.S_注意事项标签3.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_注意事项标签3.ForeColor = System.Drawing.Color.Blue;
		this.S_注意事项标签3.Location = new System.Drawing.Point(40, 271);
		this.S_注意事项标签3.Name = "S_注意事项标签3";
		this.S_注意事项标签3.Size = new System.Drawing.Size(395, 12);
		this.S_注意事项标签3.TabIndex = 22;
		this.S_注意事项标签3.Text = "怪物爆率计算公式:1/(X - X * 怪物额外爆率),X表示随机多少次掉落一次";
		this.S_注意事项标签2.AutoSize = true;
		this.S_注意事项标签2.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_注意事项标签2.ForeColor = System.Drawing.Color.Blue;
		this.S_注意事项标签2.Location = new System.Drawing.Point(40, 251);
		this.S_注意事项标签2.Name = "S_注意事项标签2";
		this.S_注意事项标签2.Size = new System.Drawing.Size(257, 12);
		this.S_注意事项标签2.TabIndex = 20;
		this.S_注意事项标签2.Text = "本页所有时间设置项单位均为分钟, 请留意设置";
		this.S_注意事项标签1.AutoSize = true;
		this.S_注意事项标签1.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_注意事项标签1.ForeColor = System.Drawing.Color.Blue;
		this.S_注意事项标签1.Location = new System.Drawing.Point(17, 231);
		this.S_注意事项标签1.Name = "S_注意事项标签1";
		this.S_注意事项标签1.Size = new System.Drawing.Size(233, 12);
		this.S_注意事项标签1.TabIndex = 21;
		this.S_注意事项标签1.Text = "注: 合并客户数据为合区专用, 请谨慎使用";
		this.S_重载客户数据.Location = new System.Drawing.Point(17, 115);
		this.S_重载客户数据.Name = "S_重载客户数据";
		this.S_重载客户数据.Size = new System.Drawing.Size(444, 23);
		this.S_重载客户数据.TabIndex = 13;
		this.S_重载客户数据.Text = "重载客户数据";
		this.S_重载客户数据.UseVisualStyleBackColor = true;
		this.S_重载客户数据.Click += new System.EventHandler(重载客户数据_Click);
		this.S_重载系统数据.Location = new System.Drawing.Point(17, 86);
		this.S_重载系统数据.Name = "S_重载系统数据";
		this.S_重载系统数据.Size = new System.Drawing.Size(444, 23);
		this.S_重载系统数据.TabIndex = 12;
		this.S_重载系统数据.Text = "重载系统数据";
		this.S_重载系统数据.UseVisualStyleBackColor = true;
		this.S_重载系统数据.Click += new System.EventHandler(重载系统数据_Click);
		this.S_浏览合并目录.Location = new System.Drawing.Point(438, 166);
		this.S_浏览合并目录.Name = "S_浏览合并目录";
		this.S_浏览合并目录.Size = new System.Drawing.Size(23, 23);
		this.S_浏览合并目录.TabIndex = 11;
		this.S_浏览合并目录.Text = "S";
		this.S_浏览合并目录.UseVisualStyleBackColor = true;
		this.S_浏览合并目录.Click += new System.EventHandler(选择数据目录_Click);
		this.S_浏览备份目录.Location = new System.Drawing.Point(438, 55);
		this.S_浏览备份目录.Name = "S_浏览备份目录";
		this.S_浏览备份目录.Size = new System.Drawing.Size(23, 23);
		this.S_浏览备份目录.TabIndex = 10;
		this.S_浏览备份目录.Text = "S";
		this.S_浏览备份目录.UseVisualStyleBackColor = true;
		this.S_浏览备份目录.Click += new System.EventHandler(选择数据目录_Click);
		this.S_浏览数据目录.Location = new System.Drawing.Point(438, 25);
		this.S_浏览数据目录.Name = "S_浏览数据目录";
		this.S_浏览数据目录.Size = new System.Drawing.Size(23, 23);
		this.S_浏览数据目录.TabIndex = 9;
		this.S_浏览数据目录.Text = "S";
		this.S_浏览数据目录.UseVisualStyleBackColor = true;
		this.S_浏览数据目录.Click += new System.EventHandler(选择数据目录_Click);
		this.S_合并客户数据.Location = new System.Drawing.Point(17, 197);
		this.S_合并客户数据.Name = "S_合并客户数据";
		this.S_合并客户数据.Size = new System.Drawing.Size(444, 23);
		this.S_合并客户数据.TabIndex = 8;
		this.S_合并客户数据.Text = "合并客户数据";
		this.S_合并客户数据.UseVisualStyleBackColor = true;
		this.S_合并客户数据.Click += new System.EventHandler(合并客户数据_Click);
		this.S_合并数据目录.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_合并数据目录.Location = new System.Drawing.Point(114, 168);
		this.S_合并数据目录.Name = "S_合并数据目录";
		this.S_合并数据目录.Size = new System.Drawing.Size(328, 21);
		this.S_合并数据目录.TabIndex = 7;
		this.S_合并目录标签.AutoSize = true;
		this.S_合并目录标签.Location = new System.Drawing.Point(17, 173);
		this.S_合并目录标签.Name = "S_合并目录标签";
		this.S_合并目录标签.Size = new System.Drawing.Size(77, 12);
		this.S_合并目录标签.TabIndex = 6;
		this.S_合并目录标签.Text = "合并数据目录";
		this.S_数据备份目录.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_数据备份目录.Location = new System.Drawing.Point(114, 56);
		this.S_数据备份目录.Name = "S_数据备份目录";
		this.S_数据备份目录.ReadOnly = true;
		this.S_数据备份目录.Size = new System.Drawing.Size(328, 21);
		this.S_数据备份目录.TabIndex = 5;
		this.S_数据备份目录.Text = ".\\Backup";
		this.S_游戏数据目录.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_游戏数据目录.Location = new System.Drawing.Point(114, 27);
		this.S_游戏数据目录.Name = "S_游戏数据目录";
		this.S_游戏数据目录.ReadOnly = true;
		this.S_游戏数据目录.Size = new System.Drawing.Size(328, 21);
		this.S_游戏数据目录.TabIndex = 4;
		this.S_游戏数据目录.Text = ".\\Database";
		this.S_备份目录标签.AutoSize = true;
		this.S_备份目录标签.Location = new System.Drawing.Point(17, 61);
		this.S_备份目录标签.Name = "S_备份目录标签";
		this.S_备份目录标签.Size = new System.Drawing.Size(77, 12);
		this.S_备份目录标签.TabIndex = 3;
		this.S_备份目录标签.Text = "数据备份目录";
		this.S_数据目录标签.AutoSize = true;
		this.S_数据目录标签.Location = new System.Drawing.Point(17, 32);
		this.S_数据目录标签.Name = "S_数据目录标签";
		this.S_数据目录标签.Size = new System.Drawing.Size(77, 12);
		this.S_数据目录标签.TabIndex = 1;
		this.S_数据目录标签.Text = "游戏数据目录";
		this.S_游戏设置分组.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.S_游戏设置分组.Controls.Add(this.S_新手扶持标签);
		this.S_游戏设置分组.Controls.Add(this.S_新手扶持等级);
		this.S_游戏设置分组.Controls.Add(this.S_物品归属标签);
		this.S_游戏设置分组.Controls.Add(this.S_物品归属时间);
		this.S_游戏设置分组.Controls.Add(this.S_物品清理标签);
		this.S_游戏设置分组.Controls.Add(this.S_物品清理时间);
		this.S_游戏设置分组.Controls.Add(this.S_诱惑时长标签);
		this.S_游戏设置分组.Controls.Add(this.S_怪物诱惑时长);
		this.S_游戏设置分组.Controls.Add(this.S_收益衰减标签);
		this.S_游戏设置分组.Controls.Add(this.S_收益减少比率);
		this.S_游戏设置分组.Controls.Add(this.S_收益等级标签);
		this.S_游戏设置分组.Controls.Add(this.S_减收益等级差);
		this.S_游戏设置分组.Controls.Add(this.S_经验倍率标签);
		this.S_游戏设置分组.Controls.Add(this.S_怪物经验倍率);
		this.S_游戏设置分组.Controls.Add(this.S_特修折扣标签);
		this.S_游戏设置分组.Controls.Add(this.S_装备特修折扣);
		this.S_游戏设置分组.Controls.Add(this.S_怪物爆率标签);
		this.S_游戏设置分组.Controls.Add(this.S_怪物额外爆率);
		this.S_游戏设置分组.Controls.Add(this.S_开放等级标签);
		this.S_游戏设置分组.Controls.Add(this.S_游戏开放等级);
		this.S_游戏设置分组.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_游戏设置分组.Location = new System.Drawing.Point(324, 12);
		this.S_游戏设置分组.Name = "S_游戏设置分组";
		this.S_游戏设置分组.Size = new System.Drawing.Size(282, 413);
		this.S_游戏设置分组.TabIndex = 8;
		this.S_游戏设置分组.TabStop = false;
		this.S_游戏设置分组.Text = "游戏设置";
		this.S_新手扶持标签.AutoSize = true;
		this.S_新手扶持标签.Location = new System.Drawing.Point(28, 61);
		this.S_新手扶持标签.Name = "S_新手扶持标签";
		this.S_新手扶持标签.Size = new System.Drawing.Size(77, 12);
		this.S_新手扶持标签.TabIndex = 21;
		this.S_新手扶持标签.Text = "新手扶持等级";
		this.S_新手扶持等级.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_新手扶持等级.Location = new System.Drawing.Point(125, 57);
		this.S_新手扶持等级.Name = "S_新手扶持等级";
		this.S_新手扶持等级.Size = new System.Drawing.Size(109, 23);
		this.S_新手扶持等级.TabIndex = 20;
		this.S_新手扶持等级.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_新手扶持等级.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_物品归属标签.AutoSize = true;
		this.S_物品归属标签.Location = new System.Drawing.Point(28, 293);
		this.S_物品归属标签.Name = "S_物品归属标签";
		this.S_物品归属标签.Size = new System.Drawing.Size(77, 12);
		this.S_物品归属标签.TabIndex = 19;
		this.S_物品归属标签.Text = "物品归属时间";
		this.S_物品归属时间.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_物品归属时间.Location = new System.Drawing.Point(125, 289);
		this.S_物品归属时间.Name = "S_物品归属时间";
		this.S_物品归属时间.Size = new System.Drawing.Size(109, 23);
		this.S_物品归属时间.TabIndex = 18;
		this.S_物品归属时间.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_物品归属时间.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.S_物品归属时间.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_物品清理标签.AutoSize = true;
		this.S_物品清理标签.Location = new System.Drawing.Point(28, 264);
		this.S_物品清理标签.Name = "S_物品清理标签";
		this.S_物品清理标签.Size = new System.Drawing.Size(77, 12);
		this.S_物品清理标签.TabIndex = 17;
		this.S_物品清理标签.Text = "物品清理时间";
		this.S_物品清理时间.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_物品清理时间.Location = new System.Drawing.Point(125, 260);
		this.S_物品清理时间.Name = "S_物品清理时间";
		this.S_物品清理时间.Size = new System.Drawing.Size(109, 23);
		this.S_物品清理时间.TabIndex = 16;
		this.S_物品清理时间.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_物品清理时间.Value = new decimal(new int[4] { 4, 0, 0, 0 });
		this.S_物品清理时间.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_诱惑时长标签.AutoSize = true;
		this.S_诱惑时长标签.Location = new System.Drawing.Point(28, 235);
		this.S_诱惑时长标签.Name = "S_诱惑时长标签";
		this.S_诱惑时长标签.Size = new System.Drawing.Size(77, 12);
		this.S_诱惑时长标签.TabIndex = 15;
		this.S_诱惑时长标签.Text = "怪物诱惑时长";
		this.S_怪物诱惑时长.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_怪物诱惑时长.Location = new System.Drawing.Point(125, 231);
		this.S_怪物诱惑时长.Maximum = new decimal(new int[4] { 1200, 0, 0, 0 });
		this.S_怪物诱惑时长.Name = "S_怪物诱惑时长";
		this.S_怪物诱惑时长.Size = new System.Drawing.Size(109, 23);
		this.S_怪物诱惑时长.TabIndex = 14;
		this.S_怪物诱惑时长.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_怪物诱惑时长.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.S_怪物诱惑时长.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_收益衰减标签.AutoSize = true;
		this.S_收益衰减标签.Location = new System.Drawing.Point(28, 206);
		this.S_收益衰减标签.Name = "S_收益衰减标签";
		this.S_收益衰减标签.Size = new System.Drawing.Size(77, 12);
		this.S_收益衰减标签.TabIndex = 13;
		this.S_收益衰减标签.Text = "收益减少比率";
		this.S_收益减少比率.DecimalPlaces = 2;
		this.S_收益减少比率.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_收益减少比率.Location = new System.Drawing.Point(125, 202);
		this.S_收益减少比率.Maximum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.S_收益减少比率.Name = "S_收益减少比率";
		this.S_收益减少比率.Size = new System.Drawing.Size(109, 23);
		this.S_收益减少比率.TabIndex = 12;
		this.S_收益减少比率.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_收益减少比率.Value = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.S_收益减少比率.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_收益等级标签.AutoSize = true;
		this.S_收益等级标签.Location = new System.Drawing.Point(28, 177);
		this.S_收益等级标签.Name = "S_收益等级标签";
		this.S_收益等级标签.Size = new System.Drawing.Size(77, 12);
		this.S_收益等级标签.TabIndex = 11;
		this.S_收益等级标签.Text = "减收益等级差";
		this.S_减收益等级差.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_减收益等级差.Location = new System.Drawing.Point(125, 173);
		this.S_减收益等级差.Name = "S_减收益等级差";
		this.S_减收益等级差.Size = new System.Drawing.Size(109, 23);
		this.S_减收益等级差.TabIndex = 10;
		this.S_减收益等级差.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_减收益等级差.Value = new decimal(new int[4] { 10, 0, 0, 0 });
		this.S_减收益等级差.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_经验倍率标签.AutoSize = true;
		this.S_经验倍率标签.Location = new System.Drawing.Point(28, 148);
		this.S_经验倍率标签.Name = "S_经验倍率标签";
		this.S_经验倍率标签.Size = new System.Drawing.Size(77, 12);
		this.S_经验倍率标签.TabIndex = 9;
		this.S_经验倍率标签.Text = "怪物经验倍率";
		this.S_怪物经验倍率.DecimalPlaces = 2;
		this.S_怪物经验倍率.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_怪物经验倍率.Increment = new decimal(new int[4] { 5, 0, 0, 65536 });
		this.S_怪物经验倍率.Location = new System.Drawing.Point(125, 144);
		this.S_怪物经验倍率.Maximum = new decimal(new int[4] { 1000000, 0, 0, 0 });
		this.S_怪物经验倍率.Name = "S_怪物经验倍率";
		this.S_怪物经验倍率.Size = new System.Drawing.Size(109, 23);
		this.S_怪物经验倍率.TabIndex = 8;
		this.S_怪物经验倍率.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_怪物经验倍率.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.S_怪物经验倍率.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_特修折扣标签.AutoSize = true;
		this.S_特修折扣标签.Location = new System.Drawing.Point(28, 90);
		this.S_特修折扣标签.Name = "S_特修折扣标签";
		this.S_特修折扣标签.Size = new System.Drawing.Size(77, 12);
		this.S_特修折扣标签.TabIndex = 7;
		this.S_特修折扣标签.Text = "装备特修折扣";
		this.S_装备特修折扣.DecimalPlaces = 2;
		this.S_装备特修折扣.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_装备特修折扣.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.S_装备特修折扣.Location = new System.Drawing.Point(125, 86);
		this.S_装备特修折扣.Maximum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.S_装备特修折扣.Name = "S_装备特修折扣";
		this.S_装备特修折扣.Size = new System.Drawing.Size(109, 23);
		this.S_装备特修折扣.TabIndex = 6;
		this.S_装备特修折扣.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_装备特修折扣.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.S_装备特修折扣.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_怪物爆率标签.AutoSize = true;
		this.S_怪物爆率标签.Location = new System.Drawing.Point(28, 119);
		this.S_怪物爆率标签.Name = "S_怪物爆率标签";
		this.S_怪物爆率标签.Size = new System.Drawing.Size(77, 12);
		this.S_怪物爆率标签.TabIndex = 5;
		this.S_怪物爆率标签.Text = "怪物额外爆率";
		this.S_怪物额外爆率.DecimalPlaces = 2;
		this.S_怪物额外爆率.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_怪物额外爆率.Increment = new decimal(new int[4] { 5, 0, 0, 131072 });
		this.S_怪物额外爆率.Location = new System.Drawing.Point(125, 115);
		this.S_怪物额外爆率.Maximum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.S_怪物额外爆率.Name = "S_怪物额外爆率";
		this.S_怪物额外爆率.Size = new System.Drawing.Size(109, 23);
		this.S_怪物额外爆率.TabIndex = 4;
		this.S_怪物额外爆率.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_怪物额外爆率.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.S_怪物额外爆率.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_开放等级标签.AutoSize = true;
		this.S_开放等级标签.Location = new System.Drawing.Point(28, 32);
		this.S_开放等级标签.Name = "S_开放等级标签";
		this.S_开放等级标签.Size = new System.Drawing.Size(77, 12);
		this.S_开放等级标签.TabIndex = 3;
		this.S_开放等级标签.Text = "游戏开放等级";
		this.S_游戏开放等级.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_游戏开放等级.Location = new System.Drawing.Point(125, 28);
		this.S_游戏开放等级.Name = "S_游戏开放等级";
		this.S_游戏开放等级.Size = new System.Drawing.Size(109, 23);
		this.S_游戏开放等级.TabIndex = 2;
		this.S_游戏开放等级.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_游戏开放等级.Value = new decimal(new int[4] { 40, 0, 0, 0 });
		this.S_游戏开放等级.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_网络设置分组.Controls.Add(this.S_掉线判定标签);
		this.S_网络设置分组.Controls.Add(this.S_掉线判定时间);
		this.S_网络设置分组.Controls.Add(this.S_限定封包标签);
		this.S_网络设置分组.Controls.Add(this.S_封包限定数量);
		this.S_网络设置分组.Controls.Add(this.S_屏蔽时间标签);
		this.S_网络设置分组.Controls.Add(this.S_异常屏蔽时间);
		this.S_网络设置分组.Controls.Add(this.S_接收端口标签);
		this.S_网络设置分组.Controls.Add(this.S_门票接收端口);
		this.S_网络设置分组.Controls.Add(this.S_监听端口标签);
		this.S_网络设置分组.Controls.Add(this.S_客户连接端口);
		this.S_网络设置分组.Font = new System.Drawing.Font("宋体", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_网络设置分组.Location = new System.Drawing.Point(15, 12);
		this.S_网络设置分组.Name = "S_网络设置分组";
		this.S_网络设置分组.Size = new System.Drawing.Size(282, 196);
		this.S_网络设置分组.TabIndex = 0;
		this.S_网络设置分组.TabStop = false;
		this.S_网络设置分组.Text = "网络设置";
		this.S_掉线判定标签.AutoSize = true;
		this.S_掉线判定标签.Location = new System.Drawing.Point(27, 148);
		this.S_掉线判定标签.Name = "S_掉线判定标签";
		this.S_掉线判定标签.Size = new System.Drawing.Size(77, 12);
		this.S_掉线判定标签.TabIndex = 9;
		this.S_掉线判定标签.Text = "掉线判定时间";
		this.S_掉线判定时间.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_掉线判定时间.Location = new System.Drawing.Point(124, 144);
		this.S_掉线判定时间.Name = "S_掉线判定时间";
		this.S_掉线判定时间.Size = new System.Drawing.Size(109, 23);
		this.S_掉线判定时间.TabIndex = 8;
		this.S_掉线判定时间.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_掉线判定时间.Value = new decimal(new int[4] { 5, 0, 0, 0 });
		this.S_掉线判定时间.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_限定封包标签.AutoSize = true;
		this.S_限定封包标签.Location = new System.Drawing.Point(27, 90);
		this.S_限定封包标签.Name = "S_限定封包标签";
		this.S_限定封包标签.Size = new System.Drawing.Size(77, 12);
		this.S_限定封包标签.TabIndex = 7;
		this.S_限定封包标签.Text = "封包限定数量";
		this.S_封包限定数量.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_封包限定数量.Location = new System.Drawing.Point(124, 86);
		this.S_封包限定数量.Maximum = new decimal(new int[4] { 500, 0, 0, 0 });
		this.S_封包限定数量.Name = "S_封包限定数量";
		this.S_封包限定数量.Size = new System.Drawing.Size(109, 23);
		this.S_封包限定数量.TabIndex = 6;
		this.S_封包限定数量.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_封包限定数量.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.S_封包限定数量.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_屏蔽时间标签.AutoSize = true;
		this.S_屏蔽时间标签.Location = new System.Drawing.Point(27, 119);
		this.S_屏蔽时间标签.Name = "S_屏蔽时间标签";
		this.S_屏蔽时间标签.Size = new System.Drawing.Size(77, 12);
		this.S_屏蔽时间标签.TabIndex = 5;
		this.S_屏蔽时间标签.Text = "异常屏蔽时间";
		this.S_异常屏蔽时间.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_异常屏蔽时间.Location = new System.Drawing.Point(124, 115);
		this.S_异常屏蔽时间.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.S_异常屏蔽时间.Name = "S_异常屏蔽时间";
		this.S_异常屏蔽时间.Size = new System.Drawing.Size(109, 23);
		this.S_异常屏蔽时间.TabIndex = 4;
		this.S_异常屏蔽时间.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_异常屏蔽时间.Value = new decimal(new int[4] { 5, 0, 0, 0 });
		this.S_异常屏蔽时间.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_接收端口标签.AutoSize = true;
		this.S_接收端口标签.Location = new System.Drawing.Point(27, 61);
		this.S_接收端口标签.Name = "S_接收端口标签";
		this.S_接收端口标签.Size = new System.Drawing.Size(77, 12);
		this.S_接收端口标签.TabIndex = 3;
		this.S_接收端口标签.Text = "门票接收端口";
		this.S_门票接收端口.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_门票接收端口.Location = new System.Drawing.Point(124, 57);
		this.S_门票接收端口.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.S_门票接收端口.Name = "S_门票接收端口";
		this.S_门票接收端口.Size = new System.Drawing.Size(109, 23);
		this.S_门票接收端口.TabIndex = 2;
		this.S_门票接收端口.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_门票接收端口.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.S_门票接收端口.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.S_监听端口标签.AutoSize = true;
		this.S_监听端口标签.Location = new System.Drawing.Point(27, 32);
		this.S_监听端口标签.Name = "S_监听端口标签";
		this.S_监听端口标签.Size = new System.Drawing.Size(77, 12);
		this.S_监听端口标签.TabIndex = 1;
		this.S_监听端口标签.Text = "客户连接端口";
		this.S_客户连接端口.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.S_客户连接端口.Location = new System.Drawing.Point(124, 28);
		this.S_客户连接端口.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.S_客户连接端口.Name = "S_客户连接端口";
		this.S_客户连接端口.Size = new System.Drawing.Size(109, 23);
		this.S_客户连接端口.TabIndex = 0;
		this.S_客户连接端口.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.S_客户连接端口.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.S_客户连接端口.ValueChanged += new System.EventHandler(更改设置数值_Value);
		this.界面定时更新.Interval = 95000000;
		this.下方控件页.BackColor = System.Drawing.Color.Transparent;
		this.下方控件页.Controls.Add(this.保存按钮);
		this.下方控件页.Controls.Add(this.GM命令文本);
		this.下方控件页.Controls.Add(this.GM命令标签);
		this.下方控件页.Controls.Add(this.启动按钮);
		this.下方控件页.Controls.Add(this.停止按钮);
		this.下方控件页.Location = new System.Drawing.Point(3, 485);
		this.下方控件页.Name = "下方控件页";
		this.下方控件页.Size = new System.Drawing.Size(1194, 55);
		this.下方控件页.TabIndex = 6;
		this.保存按钮.BackColor = System.Drawing.Color.LightSteelBlue;
		this.保存按钮.Font = new System.Drawing.Font("宋体", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.保存按钮.ForeColor = System.Drawing.Color.FromArgb(0, 0, 192);
		this.保存按钮.Image = (System.Drawing.Image)resources.GetObject("保存按钮.Image");
		this.保存按钮.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.保存按钮.Location = new System.Drawing.Point(787, 3);
		this.保存按钮.Name = "保存按钮";
		this.保存按钮.Size = new System.Drawing.Size(131, 38);
		this.保存按钮.TabIndex = 17;
		this.保存按钮.Text = "保存数据";
		this.保存按钮.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.保存按钮.UseVisualStyleBackColor = false;
		this.保存按钮.Click += new System.EventHandler(保存数据库_Click);
		this.GM命令文本.BackColor = System.Drawing.SystemColors.InactiveCaption;
		this.GM命令文本.Location = new System.Drawing.Point(79, 13);
		this.GM命令文本.Name = "GM命令文本";
		this.GM命令文本.Size = new System.Drawing.Size(696, 21);
		this.GM命令文本.TabIndex = 16;
		this.GM命令文本.KeyPress += new System.Windows.Forms.KeyPressEventHandler(执行GM命令行_Press);
		this.GM命令标签.AutoSize = true;
		this.GM命令标签.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.GM命令标签.Location = new System.Drawing.Point(15, 15);
		this.GM命令标签.Name = "GM命令标签";
		this.GM命令标签.Size = new System.Drawing.Size(61, 14);
		this.GM命令标签.TabIndex = 13;
		this.GM命令标签.Text = "GM命令:";
		this.GM命令标签.TextAlign = System.Drawing.ContentAlignment.BottomRight;
		this.启动按钮.BackColor = System.Drawing.Color.LightSteelBlue;
		this.启动按钮.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.启动按钮.Font = new System.Drawing.Font("宋体", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.启动按钮.ForeColor = System.Drawing.Color.Green;
		this.启动按钮.Image = (System.Drawing.Image)resources.GetObject("启动按钮.Image");
		this.启动按钮.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.启动按钮.Location = new System.Drawing.Point(925, 3);
		this.启动按钮.Name = "启动按钮";
		this.启动按钮.Size = new System.Drawing.Size(131, 38);
		this.启动按钮.TabIndex = 12;
		this.启动按钮.Text = "启动服务";
		this.启动按钮.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.启动按钮.UseVisualStyleBackColor = false;
		this.启动按钮.Click += new System.EventHandler(启动服务器_Click);
		this.停止按钮.BackColor = System.Drawing.Color.LightSteelBlue;
		this.停止按钮.Enabled = false;
		this.停止按钮.Font = new System.Drawing.Font("宋体", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.停止按钮.ForeColor = System.Drawing.Color.Brown;
		this.停止按钮.Image = (System.Drawing.Image)resources.GetObject("停止按钮.Image");
		this.停止按钮.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.停止按钮.Location = new System.Drawing.Point(1062, 3);
		this.停止按钮.Name = "停止按钮";
		this.停止按钮.Size = new System.Drawing.Size(131, 38);
		this.停止按钮.TabIndex = 11;
		this.停止按钮.Text = "停止服务";
		this.停止按钮.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.停止按钮.UseVisualStyleBackColor = false;
		this.停止按钮.Click += new System.EventHandler(停止服务器_Click);
		this.保存数据提醒.Enabled = true;
		this.保存数据提醒.Interval = 500;
		this.保存数据提醒.Tick += new System.EventHandler(保存数据提醒_Tick);
		this.定时发送公告.Tick += new System.EventHandler(定时发送公告_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1198, 529);
		base.Controls.Add(this.下方控件页);
		base.Controls.Add(this.主选项卡);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.Name = "主窗口";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "游戏服务器";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(关闭主界面_Click);
		this.主选项卡.ResumeLayout(false);
		this.日志页面.ResumeLayout(false);
		this.日志页面.PerformLayout();
		this.日志选项卡.ResumeLayout(false);
		this.系统日志页面.ResumeLayout(false);
		this.聊天日志页面.ResumeLayout(false);
		this.命令日志页面.ResumeLayout(false);
		this.角色页面.ResumeLayout(false);
		this.角色详情选项卡.ResumeLayout(false);
		this.角色数据_技能.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.技能浏览表).EndInit();
		this.角色数据_装备.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.装备浏览表).EndInit();
		this.角色数据_背包.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.背包浏览表).EndInit();
		this.角色数据_仓库.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.仓库浏览表).EndInit();
		((System.ComponentModel.ISupportInitialize)this.角色浏览表).EndInit();
		this.角色右键菜单.ResumeLayout(false);
		this.地图页面.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.地图浏览表).EndInit();
		this.怪物页面.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.掉落浏览表).EndInit();
		((System.ComponentModel.ISupportInitialize)this.怪物浏览表).EndInit();
		this.封禁页面.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.封禁浏览表).EndInit();
		this.公告页面.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.公告浏览表).EndInit();
		this.设置页面.ResumeLayout(false);
		this.S_游戏数据分组.ResumeLayout(false);
		this.S_游戏数据分组.PerformLayout();
		this.S_游戏设置分组.ResumeLayout(false);
		this.S_游戏设置分组.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.S_新手扶持等级).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_物品归属时间).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_物品清理时间).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_怪物诱惑时长).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_收益减少比率).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_减收益等级差).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_怪物经验倍率).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_装备特修折扣).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_怪物额外爆率).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_游戏开放等级).EndInit();
		this.S_网络设置分组.ResumeLayout(false);
		this.S_网络设置分组.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.S_掉线判定时间).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_封包限定数量).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_异常屏蔽时间).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_门票接收端口).EndInit();
		((System.ComponentModel.ISupportInitialize)this.S_客户连接端口).EndInit();
		this.下方控件页.ResumeLayout(false);
		this.下方控件页.PerformLayout();
		base.ResumeLayout(false);
	}

	static 主窗口()
	{
	}
}
